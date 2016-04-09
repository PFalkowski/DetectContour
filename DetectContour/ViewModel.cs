
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using SpatialMaps;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using Emgu.CV.CvEnum;
using System.Windows.Shapes;

namespace DetectContour
{
    public class ViewModel : BindableBase
    {
        private readonly Canvas _hostageCanvas;
        private readonly Canvas _hostageCanvas2;
        private readonly Canvas _hostageCanvas3;
        private readonly IOService _inputOutputService = new DesktopIOService();
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

        public ViewModel(Canvas canvasToDraw, Canvas convexHullCanvas, Canvas ContourCanvas)
        {
            _hostageCanvas = canvasToDraw;
            _hostageCanvas2 = convexHullCanvas;
            _hostageCanvas3 = ContourCanvas;
            OpenImageCommand = new DelegateCommand(OpenImage);
            SaveContoursCommand = new DelegateCommand(SaveContours);
        }

        private void SaveContours()
        {
            throw new NotImplementedException();
        }

        private void OpenImage()
        {
            try
            {
                var fileName = _inputOutputService.GetFileNameForRead(null, null, null);
                if (string.IsNullOrEmpty(fileName)) return;
                CurrentImage = new BitmapImage(new Uri(fileName));
                var image = ReadImage(fileName);
                var imageProcessed = Preprocess(image);
                var canny = GetCanny(imageProcessed);
                var lines = ConvertToLines(canny);
                DrawOnCanvas(_hostageCanvas, lines);
                var pointCollection = ConvertToPointCollection(canny);
                var convexHull = GetConvexHull(pointCollection);
                DrawOnCanvas(_hostageCanvas2, convexHull);
            }
            catch (Exception ex)
            {
                _inputOutputService.PrintToScreen(ex.Message, MessageSeverity.Error);
            }
        }



        public DelegateCommand OpenImageCommand { get; private set; }
        public DelegateCommand SaveContoursCommand { get; private set; }
        private Image<Bgr, byte> ReadImage(string fileName)
        {
            return new Image<Bgr, byte>((Bitmap)System.Drawing.Image.FromFile(fileName));
        }
        private UMat Preprocess(Image<Bgr, byte> frame)
        {
            frame = frame.Resize(400, 400, Emgu.CV.CvEnum.Inter.Cubic, true);
            //frame = frame.ThresholdBinary(new Bgr(250, 250, 250), new Bgr(255, 255, 255));

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
        private static LineSegment2D[] GetCanny(IInputArrayOfArrays uimage)
        {
            #region Canny and edge detection

            var cannyThreshold = 180;
            var cannyThresholdLinking = 120;
            var cannyEdges = new UMat();

            CvInvoke.Canny(uimage, cannyEdges, cannyThreshold, cannyThresholdLinking);

            var lines = CvInvoke.HoughLinesP(
               cannyEdges,
               1, //Distance resolution in pixel-related units
               Math.PI / 45.0, //Angle resolution measured in radians.
               22, //threshold
               12, //min Line width
               10); //gap between lines
            
            #endregion

            return lines;
        }
        private static IEnumerable<Line> ConvertToLines(IReadOnlyCollection<LineSegment2D> lineSegments)
        {
            var lines = new List<Line>(lineSegments.Count);
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
        private static IEnumerable<PointF> ConvertToPointCollection(IReadOnlyCollection<LineSegment2D> lineSegments)
        {
            var collection = new List<PointF>();
            foreach (var l in lineSegments)
            {
                collection.Add(l.P1);
                collection.Add(l.P2);
            }
            return collection;
        }
        
        private static IEnumerable<PointF> GetConvexHull(IEnumerable<PointF> points)
        {
            var pts = points.ToArray();
            var result = CvInvoke.ConvexHull(pts, true);
            return result;
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
        private static void DrawOnCanvas(Panel canvas, IEnumerable<PointF> points)
        {
            canvas.Children.Clear();
            var first = true;
            var firstX = 0.0;
            var firstY = 0.0;
            var lastX = 0.0;
            var lastY = 0.0;
            foreach (var point in points)
            {
                if (first)
                {
                    firstX = lastX = point.X;
                    firstY = lastY = point.Y;

                    first = false;
                }
                else
                {
                    var line = new Line
                    {
                        Stroke = System.Windows.Media.Brushes.Blue,
                        StrokeThickness = 1,
                        X1 = lastX,
                        Y1 = lastY,
                        X2 = point.X,
                        Y2 = point.Y
                    };

                    canvas.Children.Add(line);
                    lastX = point.X;
                    lastY = point.Y;
                }
            }
            var lastLine = new Line
            {
                Stroke = System.Windows.Media.Brushes.Blue,
                StrokeThickness = 1,
                X1 = lastX,
                Y1 = lastY,
                X2 = firstX,
                Y2 = firstY
            };
            canvas.Children.Add(lastLine);

        }
    }
}
