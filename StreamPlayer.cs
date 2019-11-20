using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftWavEncryption
{
    class StreamPlayer
    {
        private static WaveOut wavePlayer;

        public static void Play(WavFormatManager wfm)
        {
            wavePlayer?.Stop();
            wavePlayer = new WaveOut();

            wavePlayer.Init(new WaveStreamProvider(wfm));
            wavePlayer.Play();
        }

        public static void Stop()
        {
            wavePlayer?.Stop();
        }
    }
}
