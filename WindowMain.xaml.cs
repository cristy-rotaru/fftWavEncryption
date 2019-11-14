using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

namespace fftWavEncryption
{
    /// <summary>
    /// Interaction logic for WindowMain.xaml
    /// </summary>
    public partial class WindowMain : Window
    {
        private delegate void AddSoundPanelDelegate(WavFormatManager wfm, String displayName);

        List<SoundPanel> soundPanelList = new List<SoundPanel>();

        public WindowMain()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void buttonAddSoundFromFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Wav files|*.wav|All files|*";

            if (ofd.ShowDialog() == true)
            {
                String fileName = ofd.FileName;

                buttonAddSoundFromFile.IsEnabled = false;

                Thread loaderThread = new Thread(new ParameterizedThreadStart(ThreadFunctionLoadSoundFromFile));
                loaderThread.Start(fileName);
            }
        }

        private void ThreadFunctionLoadSoundFromFile(Object fileName)
        {
            String stringFileName = (String)fileName;

            WavFormatManager wfm = new WavFormatManager();

            try
            {
                wfm.DecodeFile(stringFileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
                Dispatcher.Invoke(new AddSoundPanelDelegate(CommitLoadToInterface), null, "");
                return;
            }

            Dispatcher.Invoke(new AddSoundPanelDelegate(CommitLoadToInterface), wfm, Path.GetFileNameWithoutExtension(stringFileName));
        }

        private void CommitLoadToInterface(WavFormatManager wfm, String displayName)
        {
            buttonAddSoundFromFile.IsEnabled = true;

            if (wfm != null)
            {
                SoundPanel sp = new SoundPanel(wfm, displayName);
                sp.SetEncryptButtonHandler(new Action(() => { MessageBox.Show("Encrypt"); }));
                sp.SetDecryptButtonHandler(new Action(() => { MessageBox.Show("Decrypt"); }));
                sp.SetPlayButtonHandler(new Action(() => { MessageBox.Show("Play"); }));

                int z = soundPanelList.Count;

                stackPanelSoundItems.Children.Insert(z, sp);
                soundPanelList.Add(sp);
            }
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

                    SaveFileDialog sfd_ = new SaveFileDialog();
                    if (sfd_.ShowDialog() == true)
                    {
                        filename = sfd_.FileName;
                        wfm.SetBitsPerSample(32);
                        wfm.EncodeFile(filename);
                    }

                    leftF = FourierTransform.FFT_segmented(wfm.ReadAudioLeft());
                    rightF = FourierTransform.FFT_segmented(wfm.ReadAudioRight());
                    CryptoFFT.Decrypt(leftF);
                    CryptoFFT.Decrypt(rightF);
                    wfm.WriteAudioLeft(FourierTransform.IFFT_segmented(leftF));
                    wfm.WriteAudioRight(FourierTransform.IFFT_segmented(rightF));
                }

                SaveFileDialog sfd = new SaveFileDialog();
                if (sfd.ShowDialog() == true)
                {
                    filename = sfd.FileName;
                    wfm.SetBitsPerSample(32);
                    wfm.EncodeFile(filename);
                }
            }
        }
    }
}
