
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
using System.IO;
using Emgu.CV.CvEnum;
using System.Windows.Shapes;
using Emgu.CV.Util;

namespace DetectContour
{
    public class ViewModel : BindableBase
    {
        private readonly Canvas _hostageCanvas;
        private readonly Canvas _hostageCanvas2;
        private readonly Canvas _hostageCanvas3;
        private readonly IOService _inputOutputService = new DesktopIOService();
        private List<PointF> _convexHull = new List<PointF>();
        private List<PointF> _contour = new List<PointF>();
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
                    OnPropertyChanged();
                }
            }
        }

        public ViewModel(Canvas canvasToDraw, Canvas convexHullCanvas, Canvas ContourCanvas)
        {
            _hostageCanvas = canvasToDraw;
            _hostageCanvas2 = convexHullCanvas;
            _hostageCanvas3 = ContourCanvas;
            OpenImageCommand = new DelegateCommand(OpenImage);
            SaveContoursCommand = new DelegateCommand(SaveConvexHull);
            SaveCannyCommand = new DelegateCommand(SaveContours);
        }

        private void Save(List<PointF> points)
        {
            try
            {
                if (points?.Count > 0)
                {
                    var path = _inputOutputService.GetFileNameForWrite(null, null, null);
                    path = System.IO.Path.ChangeExtension(path, ".xml");
                    if (!string.IsNullOrEmpty(path))
                    {
                        var converted = Helper.ConvertPointFToC2DPointDummy(points);
                        var xdoc = converted.SerializeToXDoc();
                        xdoc.Save(path);
                    }
                }
            }
            catch (IOException ex)
            {
                _inputOutputService.PrintToScreen(ex.Message, MessageSeverity.Error);
            }
        }
        private void SaveContours()
        {
            Save(_contour);
        }

        private void SaveConvexHull()
        {
            Save(_convexHull);
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
                _convexHull = GetConvexHull(pointCollection).ToList();
                DrawOnCanvas(_hostageCanvas2, _convexHull);
                var pre_contours = GetContours(imageProcessed);
                var contours = ConvertToLines(pre_contours).ToList();
                _contour = ConvertToPointCollection(pre_contours).ToList();
                DrawOnCanvas(_hostageCanvas3, contours);
            }
            catch (Exception ex)
            {
                _inputOutputService.PrintToScreen(ex.Message, MessageSeverity.Error);
            }
        }



        public DelegateCommand OpenImageCommand { get; private set; }
        public DelegateCommand SaveContoursCommand { get; private set; }
        public DelegateCommand SaveCannyCommand { get; private set; }
        
        private Image<Bgr, byte> ReadImage(string fileName)
        {
            return new Image<Bgr, byte>((Bitmap)System.Drawing.Image.FromFile(fileName));
        }
        private UMat Preprocess(Image<Bgr, byte> frame)
        {
            frame = frame.Resize(400, 400, Emgu.CV.CvEnum.Inter.Linear, true);
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
        private static LineSegment2D[] GetContours(IInputOutputArray img)
        {
            #region Canny and edge detection

            var cannyThreshold = 180;
            var cannyThresholdLinking = 120;
            var cannyEdges = new UMat();

            CvInvoke.Canny(img, cannyEdges, cannyThreshold, cannyThresholdLinking);

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
        //private static VectorOfVectorOfPoint GetContours(IInputOutputArray img)
        //{
        //    VectorOfVectorOfPoint contoursDetected = new VectorOfVectorOfPoint(100000000);
        //    CvInvoke.FindContours(img, contoursDetected, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
        //    FindLargestContour(img, contoursDetected);
        //    return contoursDetected;
        //}
        //private static IEnumerable<Line> ConvertToLines(VectorOfVectorOfPoint points)
        //{

        //    var lines = new List<Line>();
        //    PointF previous = new PointF();
        //    //for (int j = 0; j < points.Size; ++j)
        //    //{
        //    for (int i = 0; i < points[0].Size; ++i)
        //    {
        //        if (i == 0)
        //        {
        //            previous = new PointF(points[0][i].X, points[0][i].Y);
        //        }
        //        else
        //        {
        //            var current = new PointF(points[0][i].X, points[0][i].Y);
        //            var line = new Line();
        //            line.X1 = previous.X;
        //            line.X2 = current.X;
        //            line.Y1 = previous.Y;
        //            line.Y2 = current.X;
        //            lines.Add(line);
        //            previous = current;
        //        }
        //    }
        //    //}
        //    return lines;
        //}
        //public static VectorOfPoint FindLargestContour(IInputOutputArray cannyEdges, IInputOutputArray result)
        //{
        //    int largest_contour_index = 0;
        //    double largest_area = 0;
        //    VectorOfPoint largestContour;

        //    using (Mat hierachy = new Mat())
        //    using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
        //    {
        //        IOutputArray hirarchy;

        //        CvInvoke.FindContours(cannyEdges, contours, hierachy, RetrType.Tree, ChainApproxMethod.ChainApproxNone);

        //        for (int i = 0; i < contours.Size; i++)
        //        {
        //            MCvScalar color = new MCvScalar(0, 0, 255);

        //            double a = CvInvoke.ContourArea(contours[i], false);  //  Find the area of contour
        //            if (a > largest_area)
        //            {
        //                largest_area = a;
        //                largest_contour_index = i;                //Store the index of largest contour
        //            }

        //            CvInvoke.DrawContours(result, contours, largest_contour_index, new MCvScalar(255, 0, 0));
        //        }

        //        CvInvoke.DrawContours(result, contours, largest_contour_index, new MCvScalar(0, 0, 255), 3, LineType.EightConnected, hierachy);
        //        largestContour = new VectorOfPoint(contours[largest_contour_index].ToArray());
        //    }

        //    return largestContour;
        //}
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
