using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace fftWavEncryption
{
    class WaveStreamProvider : IWaveProvider
    {
        private WaveFormat streamFormat;

        private float[] bufferMono, bufferLeft, bufferRight;
        private int currentBufferIndex;

        private WindowMain.PlotGraphsDelegate plotMethod;
        private Window handle;

        private Thread plotterThread;
        private bool stopThread;

        public WaveStreamProvider(WavFormatManager wfm, WindowMain.PlotGraphsDelegate plotDelegate, Window windowHandle)
        {
            this.plotMethod = plotDelegate;
            this.handle = windowHandle;

            this.streamFormat = new WaveFormat((int)wfm.GetSampleRate(), 16, wfm.GetNumberOfChannels());

            if (this.streamFormat.Channels == 1)
            {
                this.bufferMono = wfm.ReadAudioMono();
            }
            else
            {
                this.bufferLeft = wfm.ReadAudioLeft();
                this.bufferRight = wfm.ReadAudioRight();
            }

            currentBufferIndex = 0;

            this.stopThread = false;
            this.plotterThread = new Thread(new ThreadStart(this.PlotterThreadFunction));
            this.plotterThread.Start();
        }

        WaveFormat IWaveProvider.WaveFormat
        {
            get { return this.streamFormat; }
        }

        int IWaveProvider.Read(byte[] buffer, int offset, int count)
        {
            int increment, initialIndex;

            lock (this)
            {
                increment = this.streamFormat.Channels == 1 ? 2 : 4;
                initialIndex = this.currentBufferIndex;

                for (int i = 0; i < count; i += increment)
                {
                    if (this.streamFormat.Channels == 1)
                    {
                        if (this.currentBufferIndex >= this.bufferMono.Length)
                        {
                            break;
                        }

                        Int16 sampleMono = (Int16)(this.bufferMono[this.currentBufferIndex] * 0x7FFF);

                        buffer[offset + i] = (Byte)(sampleMono & 0x00FF);
                        buffer[offset + i + 1] = (Byte)((sampleMono & 0xFF00) >> 8);
                    }
                    else
                    {
                        if (this.currentBufferIndex >= this.bufferLeft.Length)
                        {
                            break;
                        }

                        Int16 sampleLeft = (Int16)(this.bufferLeft[this.currentBufferIndex] * 0x7FFF);
                        Int16 sampleRight = (Int16)(this.bufferRight[this.currentBufferIndex] * 0x7FFF);

                        buffer[offset + i] = (Byte)(sampleLeft & 0x00FF);
                        buffer[offset + i + 1] = (Byte)((sampleLeft & 0xFF00) >> 8);
                        buffer[offset + i + 2] = (Byte)(sampleRight & 0x00FF);
                        buffer[offset + i + 3] = (Byte)((sampleRight & 0xFF00) >> 8);
                    }

                    ++this.currentBufferIndex;
                }

                if ((this.currentBufferIndex - initialIndex) * increment < count)
                {
                    this.stopThread = true;
                }
            }

            return (this.currentBufferIndex - initialIndex) * increment;
        }

        public void StopPlotting()
        {
            lock (this)
            {
                this.stopThread = true;
            }
        }

        private void PlotterThreadFunction()
        {
            float[] monoWave = null, leftWave = null, rightWave = null, monoFourier = null, leftFourier = null, rightFourier = null;

            if (this.streamFormat.Channels == 1)
            {
                monoWave = new float[4096];
                monoFourier = new float[2047];
            }
            else
            {
                leftWave = new float[4096];
                rightWave = new float[4096];

                leftFourier = new float[2047];
                rightFourier = new float[2047];
            }

            int currentPosition = 0;

            while (true)
            {
                bool shutdown;

                lock (this)
                {
                    shutdown = this.stopThread;
                }

                if (shutdown)
                {
                    return;
                }

                int nextPosition;

                lock (this)
                {
                    nextPosition = this.currentBufferIndex;
                }

                if (nextPosition >= (currentPosition + 1024))
                {
                    if (this.streamFormat.Channels == 1)
                    {
                        Array.Copy(monoWave, 1024, monoWave, 0, 3072);
                        Array.Copy(this.bufferMono, currentPosition, monoWave, 3072, 1024);

                        Complex[] monoWaveComplex = new Complex[4096];
                        for (int i = 0; i < 4096; ++i)
                        {
                            monoWaveComplex[i] = new Complex(monoWave[i], 0.0);
                        }

                        Accord.Math.FourierTransform.FFT(monoWaveComplex, Accord.Math.FourierTransform.Direction.Forward);

                        for (int i = 0; i < 2047; ++i)
                        {
                            monoFourier[i] = (float)Math.Log10(200 * monoWaveComplex[i + 1].Magnitude);
                        }

                        this.handle.Dispatcher.BeginInvoke(new Action(() => { this.plotMethod(monoWave, monoWave, monoFourier, monoFourier); }));
                    }
                    else
                    {
                        Array.Copy(leftWave, 1024, leftWave, 0, 3072);
                        Array.Copy(this.bufferLeft, currentPosition, leftWave, 3072, 1024);

                        Array.Copy(rightWave, 1024, rightWave, 0, 3072);
                        Array.Copy(this.bufferRight, currentPosition, rightWave, 3072, 1024);

                        Complex[] leftWaveComplex = new Complex[4096];
                        Complex[] rightWaveComplex = new Complex[4096];
                        for (int i = 0; i < 4096; ++i)
                        {
                            leftWaveComplex[i] = new Complex(leftWave[i], 0.0);
                            rightWaveComplex[i] = new Complex(rightWave[i], 0.0);
                        }

                        Accord.Math.FourierTransform.FFT(leftWaveComplex, Accord.Math.FourierTransform.Direction.Forward);
                        Accord.Math.FourierTransform.FFT(rightWaveComplex, Accord.Math.FourierTransform.Direction.Forward);

                        for (int i = 0; i < 2047; ++i)
                        {
                            leftFourier[i] = (float)Math.Log10(200 * leftWaveComplex[i + 1].Magnitude);
                            rightFourier[i] = (float)Math.Log10(200 * rightWaveComplex[i + 1].Magnitude);
                        }

                        this.handle.Dispatcher.BeginInvoke(new Action(() => { this.plotMethod(leftWave, rightWave, leftFourier, rightFourier); }));
                    }

                    currentPosition += 1024;
                }
            }
        }
    }
}
