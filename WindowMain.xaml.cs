using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
                    Complex[] monoF = FourierTransform.FFT_segmented(wfm.ReadAudioMono());
                    wfm.WriteAudioMono(FourierTransform.IFFT_segmented(monoF));
                }
                else
                {
                    Complex[] leftF = FourierTransform.FFT_segmented(wfm.ReadAudioLeft());
                    Complex[] rightF = FourierTransform.FFT_segmented(wfm.ReadAudioRight());
                    CryptoFFT.Encrypt(leftF);
                    CryptoFFT.Encrypt(rightF);
                    wfm.WriteAudioLeft(FourierTransform.IFFT_segmented(leftF));
                    wfm.WriteAudioRight(FourierTransform.IFFT_segmented(rightF));
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
