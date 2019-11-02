using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
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
            Thread th = new Thread(new ThreadStart(ThreadFunction));
            th.Start();
        }

        private void ThreadFunction()
        {
            String filename;
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                filename = ofd.FileName;
                WavFormatManager wfm = new WavFormatManager(filename);

                if (wfm.GetNumberOfChannels() == 1)
                {
                    Complex[] monoF = FourierTransform.FFT(wfm.ReadAudioMono());
                    wfm.WriteAudioMono(FourierTransform.IFFT(monoF));
                }
                else
                {
                    Complex[] leftF = FourierTransform.FFT(wfm.ReadAudioLeft());
                    Complex[] rightF = FourierTransform.FFT(wfm.ReadAudioRight());
                    wfm.WriteAudioLeft(FourierTransform.IFFT(leftF));
                    wfm.WriteAudioRight(FourierTransform.IFFT(rightF));
                }

                SaveFileDialog sfd = new SaveFileDialog();
                if (sfd.ShowDialog() == true)
                {
                    filename = sfd.FileName;
                    wfm.SetBitsPerSample(24);
                    wfm.EncodeFile(filename);
                }
            }
        }
    }
}
