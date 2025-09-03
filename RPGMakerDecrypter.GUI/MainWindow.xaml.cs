using System.Windows;
using RPGMakerDecrypter.GUI.ViewModels;

namespace RPGMakerDecrypter.GUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}