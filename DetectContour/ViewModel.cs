
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using SpatialMaps;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using Emgu.CV.CvEnum;
using System.Windows.Shapes;

namespace DetectContour
{
    public class ViewModel : BindableBase
    {
        private Canvas _hostageCanvas;
        public IOService InputOutputService = new DesktopIOService();
        public const string PngFilter = "Image|*.bmp;*.png;*.jpg;*.jpeg";

        private BitmapImage _currentImage;
        public BitmapImage CurrentImage
        {
            get
            {
                return _currentImage;
            }
            set
            {
                if (value != _currentImage)
                {
                    _currentImage = value;
                    OnPropertyChanged(nameof(CurrentImage));
                }
            }
        }

        public ViewModel(Canvas canvasToDraw)
        {
            _hostageCanvas = canvasToDraw;
            OpenImageCommand = new DelegateCommand(openImage);
            SaveContoursCommand = new DelegateCommand(saveContours);
        }

        private void saveContours()
        {
            throw new NotImplementedException();
        }

        private void openImage()
        {
            try
            {
                var fileName = InputOutputService.GetFileNameForRead(null, null, null);
                if (string.IsNullOrEmpty(fileName)) return;
                CurrentImage = new BitmapImage(new Uri(fileName));
                var image = ReadImage(fileName);
                var imageProcessed = Preprocess(image);
                var contours = GetContours(imageProcessed);
                var lines = ConvertToLines(contours);
                DrawOnCanvas(lines);
            }
            catch (Exception ex)
            {
                InputOutputService.PrintToScreen(ex.Message, MessageSeverity.Error);
            }
        }

        public DelegateCommand OpenImageCommand { get; private set; }
        public DelegateCommand SaveContoursCommand { get; private set; }
        private Image<Bgr, byte> ReadImage(string fileName)
        {
            return new Image<Bgr, byte>((Bitmap)Bitmap.FromFile(fileName));
        }
        private UMat Preprocess(Image<Bgr, byte> frame)
        {
            frame.Resize(400, 400, Emgu.CV.CvEnum.Inter.Linear, true);
            //Convert the image to grayscale and filter out the noise
            UMat uimage = new UMat();
            CvInvoke.CvtColor(frame, uimage, ColorConversion.Bgr2Gray);
            //use image pyr to remove noise
            UMat pyrDown = new UMat();
            CvInvoke.PyrDown(uimage, pyrDown);
            CvInvoke.PyrUp(pyrDown, uimage);
            return uimage;
        }
        // reference http://www.codeproject.com/Articles/196168/Contour-Analysis-for-Image-Recognition-in-C
        private LineSegment2D[] GetContours(UMat uimage)
        {
            #region Canny and edge detection

            double cannyThreshold = 180.0;
            double cannyThresholdLinking = 120.0;
            UMat cannyEdges = new UMat();

            CvInvoke.Canny(uimage, cannyEdges, cannyThreshold, cannyThresholdLinking);

            LineSegment2D[] lines = CvInvoke.HoughLinesP(
               cannyEdges,
               1, //Distance resolution in pixel-related units
               Math.PI / 45.0, //Angle resolution measured in radians.
               10, //threshold
               1, //min Line width
               30); //gap between lines

            #endregion

            return lines;
        }
        private List<Line> ConvertToLines(LineSegment2D[] lineSegments)
        {
            List<Line> lines = new List<Line>(lineSegments.Count());
            foreach (var line in lineSegments)
            {
                var temp = new Line();
                temp.X1 = line.P1.X;
                temp.Y1 = line.P1.Y;
                temp.X2 = line.P2.X;
                temp.Y2 = line.P2.Y;
                lines.Add(temp);
            }
            return lines;
        }

        public void DrawOnCanvas(List<Line> lines)
        {
            _hostageCanvas.Children.Clear();
            foreach (var line in lines)
            {
                line.Stroke = System.Windows.Media.Brushes.Brown;
                line.StrokeThickness = 1;
                _hostageCanvas.Children.Add(line);
            }
        }
    }
}
