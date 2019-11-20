using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftWavEncryption
{
    class WaveStreamProvider : IWaveProvider
    {
        private WaveFormat streamFormat;

        private float[] bufferMono, bufferLeft, bufferRight;
        private int currentBufferIndex;

        public WaveStreamProvider(WavFormatManager wfm)
        {
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
        }

        WaveFormat IWaveProvider.WaveFormat
        {
            get { return this.streamFormat; }
        }

        int IWaveProvider.Read(byte[] buffer, int offset, int count)
        {
            int increment = this.streamFormat.Channels == 1 ? 2 : 4;
            int initialIndex = this.currentBufferIndex;

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

            return (this.currentBufferIndex - initialIndex) * increment;
        }
    }
}
