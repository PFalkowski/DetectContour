
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

namespace DetectContour
{
    public class ViewModel : BindableBase
    {
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

        public ViewModel()
        {
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
                GetContours(fileName);
            }
            catch (Exception ex)
            {
                InputOutputService.PrintToScreen(ex.Message, MessageSeverity.Error);
            }
        }
        
        public DelegateCommand OpenImageCommand { get; private set; }
        public DelegateCommand SaveContoursCommand { get; private set; }

        private LineSegment2D[] GetContours(string fileName)
        {
            Image<Bgr, byte> frame;
            double cannyThreshold = 180.0;
            frame = new Image<Bgr, byte>((Bitmap)Bitmap.FromFile(fileName));
            frame.Resize(400, 400, Emgu.CV.CvEnum.Inter.Linear, true);
            //Convert the image to grayscale and filter out the noise
            UMat uimage = new UMat();
            CvInvoke.CvtColor(frame, uimage, ColorConversion.Bgr2Gray);
            //use image pyr to remove noise
            UMat pyrDown = new UMat();
            CvInvoke.PyrDown(uimage, pyrDown);
            CvInvoke.PyrUp(pyrDown, uimage);

            #region Canny and edge detection

            double cannyThresholdLinking = 120.0;
            UMat cannyEdges = new UMat();
            CvInvoke.Canny(uimage, cannyEdges, cannyThreshold, cannyThresholdLinking);

            LineSegment2D[] lines = CvInvoke.HoughLinesP(
               cannyEdges,
               1, //Distance resolution in pixel-related units
               Math.PI / 45.0, //Angle resolution measured in radians.
               20, //threshold
               30, //min Line width
               10); //gap between lines

            #endregion

            return lines;
        }
    }
}
