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
        private static WaveStreamProvider wsp;

        public static void Play(WavFormatManager wfm, WindowMain.PlotGraphsDelegate plotDelegate, WindowMain handle)
        {
            wavePlayer?.Stop();
            wsp?.StopPlotting();
            wavePlayer = new WaveOut();
            wsp = new WaveStreamProvider(wfm, plotDelegate, handle);

            wavePlayer.DesiredLatency = 80;
            wavePlayer.Init(wsp);
            wavePlayer.Play();
        }

        public static void Stop()
        {
            wavePlayer?.Stop();
            wsp?.StopPlotting();
        }
    }
}
