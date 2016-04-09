
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
        private readonly Canvas _hostageCanvas;
        private Canvas _hostageCanvas2;
        private readonly IOService InputOutputService = new DesktopIOService();
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

        public ViewModel(Canvas canvasToDraw, Canvas boundingRectCanvas)
        {
            _hostageCanvas = canvasToDraw;
            _hostageCanvas2 = boundingRectCanvas;
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
                DrawOnCanvas(_hostageCanvas, lines);
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
            frame = frame.Resize(400, 400, Emgu.CV.CvEnum.Inter.Cubic, true);
            //frame = frame.ThresholdBinary(new Bgr(200, 200, 200), new Bgr(255, 255, 255));
            //frame = frame.DrawPolyline()
            //Convert the image to grayscale and filter out the noise
            var uimage = new UMat();
            CvInvoke.CvtColor(frame, uimage, ColorConversion.Bgr2Gray);
            //use image pyr to remove noise
            var pyrDown = new UMat();
            CvInvoke.PyrDown(uimage, pyrDown);
            CvInvoke.PyrUp(pyrDown, uimage);
            //CvInvoke.
            return uimage;
        }
        // reference http://www.codeproject.com/Articles/196168/Contour-Analysis-for-Image-Recognition-in-C
        private static LineSegment2D[] GetContours(UMat uimage)
        {
            #region Canny and edge detection

            var cannyThreshold = 180.0;
            var cannyThresholdLinking = 120.0;
            var cannyEdges = new UMat();

            CvInvoke.Canny(uimage, cannyEdges, cannyThreshold, cannyThresholdLinking);

            var lines = CvInvoke.HoughLinesP(
               cannyEdges,
               1, //Distance resolution in pixel-related units
               Math.PI / 45.0, //Angle resolution measured in radians.
               20, //threshold
               12, //min Line width
               30); //gap between lines

            #endregion

            return lines;
        }
        private static IEnumerable<Line> ConvertToLines(LineSegment2D[] lineSegments)
        {
            var lines = new List<Line>(lineSegments.Length);
            foreach (var line in lineSegments)
            {
                var temp = new Line
                {
                    X1 = line.P1.X,
                    Y1 = line.P1.Y,
                    X2 = line.P2.X,
                    Y2 = line.P2.Y
                };
                lines.Add(temp);
            }
            return lines;
        }

        private static void DrawOnCanvas(Panel canvas, IEnumerable<Line> lines)
        {
            canvas.Children.Clear();
            foreach (var line in lines)
            {
                line.Stroke = System.Windows.Media.Brushes.Brown;
                line.StrokeThickness = 1;
                canvas.Children.Add(line);
            }
        }
    }
}
