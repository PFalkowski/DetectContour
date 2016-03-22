
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

namespace DetectContour
{
    public class ViewModel : BindableBase
    {
        public IOService InputOutputService = new DesktopIOService();
        public const string PngFilter = "PNG|*.png";
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
            var fileName = InputOutputService.GetFileNameForRead(null, null, PngFilter);
            if (string.IsNullOrEmpty(fileName)) return;
            CurrentImage = new BitmapImage(new Uri(fileName));
        }

        public DelegateCommand OpenImageCommand { get; private set; }
        public DelegateCommand SaveContoursCommand { get; private set; }
    }
}
