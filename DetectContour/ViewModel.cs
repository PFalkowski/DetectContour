using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectContour
{
    public class ViewModel : BindableBase
    {
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
            throw new NotImplementedException();
        }

        public DelegateCommand OpenImageCommand { get; private set; }
        public DelegateCommand SaveContoursCommand { get; private set; }
    }
}
