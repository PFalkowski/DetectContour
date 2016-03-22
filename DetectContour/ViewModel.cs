
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

namespace DetectContour
{
    public class ViewModel : BindableBase
    {
        public IOService InputOutputService = new DesktopIOService();
        public const string PngFilter = "Image|*.bmp;*.png;*.jpg;*.jpeg";
        Image<Bgr, byte> frame;

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
        private BitmapImage _contoursImage;
        public BitmapImage ContoursImage
        {
            get
            {
                return _contoursImage;
            }
            set
            {
                if (value != _contoursImage)
                {
                    _contoursImage = value;
                    OnPropertyChanged(nameof(ContoursImage));
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
                ReadToBitmap(fileName);
                GetContours();
            }
            catch (Exception ex)
            {
                InputOutputService.PrintToScreen(ex.Message, MessageSeverity.Error);
            }
        }

        private void ReadToBitmap(string fileName)
        {
            frame = new Image<Bgr, byte>((Bitmap)Bitmap.FromFile(fileName));
            _currentBitmap = (Bitmap)Bitmap.FromFile(fileName);
        }

        public DelegateCommand OpenImageCommand { get; private set; }
        public DelegateCommand SaveContoursCommand { get; private set; }

        public const string outputFileName = "output.png";
        private System.Drawing.Bitmap _currentBitmap = null;

        private void GetContours()
        {
        }
    }
}
