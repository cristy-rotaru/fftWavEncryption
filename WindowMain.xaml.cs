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

        private void buttonStopPlayback_Click(object sender, RoutedEventArgs e)
        {
            StreamPlayer.Stop();
            textBlockNowPlaying.Text = "";
        }

        private void ActionEncrypt(SoundPanel sp)
        {
            Thread th = new Thread(new ThreadStart(() => { ThreadFunctionActionEncrypt(sp.GetFormatManager(), sp.GetDisplayName()); }));
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
        }

        private void ActionDecrypt(SoundPanel sp)
        {
            Thread th = new Thread(new ThreadStart(() => { ThreadFunctionActionDecrypt(sp.GetFormatManager(), sp.GetDisplayName()); }));
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
        }

        private void ActionPlay(SoundPanel sp)
        {
            StreamPlayer.Play(sp.GetFormatManager());
            textBlockNowPlaying.Text = "Now playing: " + sp.GetDisplayName();
        }

        private void ActionSave(SoundPanel sp)
        {
            Thread th = new Thread(new ThreadStart(() => { ThreadFunctionActionSave(sp.GetFormatManager(), sp.GetDisplayName()); }));
            th.Start();
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

        private void ThreadFunctionActionEncrypt(WavFormatManager wfm, String originalName)
        {
            WindowPasswordInput wpi = new WindowPasswordInput(WindowPasswordInput.ProcessType.ENCRYPTION, originalName);

            if (wpi.ShowDialog() == true)
            {
                WavFormatManager encryptedSound = new WavFormatManager();
                encryptedSound.SetBitsPerSample(24);
                encryptedSound.SetSampleRate(wfm.GetSampleRate());

                if (wfm.GetNumberOfChannels() == 1)
                {
                    encryptedSound.SetChannelCount(1);

                    Complex[] frq = FourierTransform.FFT_segmented(wfm.ReadAudioMono());
                    CryptoFFT.Encrypt(frq, wpi.GetPassword());
                    encryptedSound.WriteAudioMono(FourierTransform.IFFT_segmented(frq));
                }
                else
                {
                    encryptedSound.SetChannelCount(2);

                    Complex[] frqLeft = FourierTransform.FFT_segmented(wfm.ReadAudioLeft());
                    Complex[] frqRight = FourierTransform.FFT_segmented(wfm.ReadAudioRight());
                    CryptoFFT.Encrypt(frqLeft, wpi.GetPassword());
                    CryptoFFT.Encrypt(frqRight, wpi.GetPassword());
                    encryptedSound.WriteAudioLeft(FourierTransform.IFFT_segmented(frqLeft));
                    encryptedSound.WriteAudioRight(FourierTransform.IFFT_segmented(frqRight));
                }

                Dispatcher.Invoke(new AddSoundPanelDelegate(CommitProcessedSoundToInterface), encryptedSound, wpi.GetName());
            }
        }

        private void ThreadFunctionActionSave(WavFormatManager wfm, String originalName)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = originalName + ".wav";
            if (sfd.ShowDialog() == true)
            {
                String filename = sfd.FileName;
                wfm.SetBitsPerSample(24);
                wfm.EncodeFile(filename);
            }
        }

        private void ThreadFunctionActionDecrypt(WavFormatManager wfm, String originalName)
        {
            WindowPasswordInput wpi = new WindowPasswordInput(WindowPasswordInput.ProcessType.DECRYPTION, originalName);

            if (wpi.ShowDialog() == true)
            {
                WavFormatManager encryptedSound = new WavFormatManager();
                encryptedSound.SetBitsPerSample(24);
                encryptedSound.SetSampleRate(wfm.GetSampleRate());

                if (wfm.GetNumberOfChannels() == 1)
                {
                    encryptedSound.SetChannelCount(1);

                    Complex[] frq = FourierTransform.FFT_segmented(wfm.ReadAudioMono());
                    CryptoFFT.Decrypt(frq, wpi.GetPassword());
                    encryptedSound.WriteAudioMono(FourierTransform.IFFT_segmented(frq));
                }
                else
                {
                    encryptedSound.SetChannelCount(2);

                    Complex[] frqLeft = FourierTransform.FFT_segmented(wfm.ReadAudioLeft());
                    Complex[] frqRight = FourierTransform.FFT_segmented(wfm.ReadAudioRight());
                    CryptoFFT.Decrypt(frqLeft, wpi.GetPassword());
                    CryptoFFT.Decrypt(frqRight, wpi.GetPassword());
                    encryptedSound.WriteAudioLeft(FourierTransform.IFFT_segmented(frqLeft));
                    encryptedSound.WriteAudioRight(FourierTransform.IFFT_segmented(frqRight));
                }

                Dispatcher.Invoke(new AddSoundPanelDelegate(CommitProcessedSoundToInterface), encryptedSound, wpi.GetName());
            }
        }

        private void CommitLoadToInterface(WavFormatManager wfm, String displayName)
        {
            buttonAddSoundFromFile.IsEnabled = true;

            if (wfm != null)
            {
                SoundPanel sp = new SoundPanel(wfm, displayName);
                sp.SetEncryptButtonHandler(new Action(() => { ActionEncrypt(sp); }));
                sp.SetDecryptButtonHandler(new Action(() => { ActionDecrypt(sp); }));
                sp.SetPlayButtonHandler(new Action(() => { ActionPlay(sp); }));
                sp.SetSaveButtonHandler(new Action(() => { ActionSave(sp); }));

                int z = soundPanelList.Count;

                stackPanelSoundItems.Children.Insert(z, sp);
                soundPanelList.Add(sp);
            }
        }

        private void CommitProcessedSoundToInterface(WavFormatManager wfm, String displayName)
        {
            SoundPanel sp = new SoundPanel(wfm, displayName);
            sp.SetEncryptButtonHandler(new Action(() => { ActionEncrypt(sp); }));
            sp.SetDecryptButtonHandler(new Action(() => { ActionDecrypt(sp); }));
            sp.SetPlayButtonHandler(new Action(() => { ActionPlay(sp); }));
            sp.SetSaveButtonHandler(new Action(() => { ActionSave(sp); }));

            int z = soundPanelList.Count;

            stackPanelSoundItems.Children.Insert(z, sp);
            soundPanelList.Add(sp);
        }
    }
}
