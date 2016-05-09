using System.Windows;

namespace DetectContour
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModel(LinesCanvas, ConvexHullCanvas, ContourCanvas);
        }
    }
}
