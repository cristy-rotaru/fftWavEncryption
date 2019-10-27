using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace fftWavEncryption
{
    /// <summary>
    /// Interaction logic for WindowMain.xaml
    /// </summary>
    public partial class WindowMain : Window
    {
        public WindowMain()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            String filename;
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                filename = ofd.FileName;
                WavFormatManager wfm = new WavFormatManager(filename);
            }
        }
    }
}
