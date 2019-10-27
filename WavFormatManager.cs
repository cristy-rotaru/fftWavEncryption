using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftWavEncryption
{
    enum SampleRate
    {
        NullSampleRate = 0,
        S8000 = 8000,
        S11025 = 11025,
        S16000 = 16000,
        S22050 = 22050,
        S32000 = 32000,
        S37800 = 37800,
        S44056 = 44056,
        S44100 = 44100,
        S47250 = 44720,
        S48000 = 48000,
        S50000 = 50000,
        S50400 = 50400,
        S64000 = 64000,
        S88200 = 88200,
        S96000 = 96000
    }

    /// <summary>
    /// Class <c>WavFormatManager</c> can decode and encode .wav files. Supports only PCM encoding with up to 2 channels.
    /// </summary>
    class WavFormatManager
    {
        // stores the number of channels
        private int channelCount;
        private float[] channelLeft;
        private float[] channelRight;
        SampleRate sampleRate;
        int bitsPerSample;
        int blockSize;

        /// <summary>
        /// This constructor will create an empty .wav container to be filled with user data.
        /// </summary>
        public WavFormatManager()
        {
            this.channelCount = 0;
            this.channelLeft = null;
            this.channelRight = null;
            this.sampleRate = SampleRate.NullSampleRate;
            this.bitsPerSample = 0;
            this.blockSize = 0;
        }

        /// <summary>
        /// This constructor will create a .wav container and will fill it with the data stored in the file given as parameter.
        /// </summary>
        /// <param name="filename">Name of the file to be decoded.</param>
        public WavFormatManager(String filename) : this()
        {
            this.DecodeFile(filename);
        }

        public void DecodeFile(String filename)
        {
            FileStream inputFile = new FileStream(filename, FileMode.Open, FileAccess.Read);

            // reads the header and checks for the RIFF format identifier with WAVE container
            Byte[] fileHeader = new Byte[12];
            int readCount = inputFile.Read(fileHeader, 0, 12);
            if (readCount < 12)
            {
                throw new Exception("Not a WAV file.");
            }
            // checks the RIFF identifier
            if (fileHeader[0] != 'R' || fileHeader[1] != 'I' || fileHeader[2] != 'F' || fileHeader[3] != 'F')
            {
                throw new Exception("Not a WAV file.");
            }
            // checks actual filesize with filesize from fileheader
            UInt32 fileSize = (UInt32)fileHeader[4] | ((UInt32)fileHeader[5]) << 8 | ((UInt32)fileHeader[6]) << 16 | ((UInt32)fileHeader[7]) << 24;
            fileSize += 8;
            if (fileSize != new FileInfo(filename).Length)
            {
                throw new Exception("Invalid file header.");
            }
            // checks for WAVE container type
            if (fileHeader[8] != 'W' || fileHeader[9] != 'A' || fileHeader[10] != 'V' || fileHeader[11] != 'E')
            {
                throw new Exception("Not a WAV file.");
            }

            // reads the format descriptor and checks for a supported and valid format
            Byte[] fileFormatDescriptor = new Byte[24];
            readCount = inputFile.Read(fileFormatDescriptor, 0, 24);
            if (readCount < 24)
            {
                throw new Exception("Invalid format descriptor.");
            }
            if (fileFormatDescriptor[0] != 'f' || fileFormatDescriptor[1] != 'm' || fileFormatDescriptor[2] != 't' || fileFormatDescriptor[3] != ' ')
            {
                throw new Exception("Invalid format descriptor.");
            }
            // checks the size of the chunk to match the size of a standard PCM chunk size
            UInt32 chunkSize = (UInt32)fileFormatDescriptor[4] | ((UInt32)fileFormatDescriptor[5] << 8) | ((UInt32)fileFormatDescriptor[6] << 16) | ((UInt32)fileFormatDescriptor[7] << 24);
            if (chunkSize != 16)
            {
                throw new Exception("Format not supported.");
            }
            // checks the audio format
            UInt16 audioFormat = (UInt16)((UInt16)fileFormatDescriptor[8] | ((UInt16)fileFormatDescriptor[9] << 8));
            if (audioFormat != 1)
            {
                throw new Exception("Format not supported.");
            }
            // checks the number of channels
            this.channelCount = (int)((uint)fileFormatDescriptor[10] | ((uint)fileFormatDescriptor[11] << 8));
            if (this.channelCount > 2)
            {
                throw new Exception("Format not supported.");
            }
            // checks for a valid sample rate
            UInt32 sampleRateValue = (UInt32)fileFormatDescriptor[12] | ((UInt32)fileFormatDescriptor[13] << 8) | ((UInt32)fileFormatDescriptor[14] << 16) | ((UInt32)fileFormatDescriptor[15] << 24);
            if (Enum.IsDefined(typeof(SampleRate), (Int32)sampleRateValue))
            {
                this.sampleRate = (SampleRate)sampleRateValue;
            }
            else
            {
                throw new Exception("Invalid sample rate.");
            }
            // reading sample size data
            this.blockSize = (int)((uint)fileFormatDescriptor[20] | ((uint)fileFormatDescriptor[21] << 8));
            this.bitsPerSample = (int)((uint)fileFormatDescriptor[22] | ((uint)fileFormatDescriptor[23] << 8));

            Byte[] dataHeader = new Byte[8];
            inputFile.Read(dataHeader, 0, 8);
            if (dataHeader[0] != 'd' | dataHeader[1] != 'a' | dataHeader[2] != 't' | dataHeader[3] != 'a')
            {
                throw new Exception("Data chunck not found.");
            }

            UInt32 dataSize = (UInt32)dataHeader[4] | ((UInt32)dataHeader[5] << 8) | ((UInt32)dataHeader[6] << 16) | ((UInt32)dataHeader[7] << 24);
            UInt32 sampleCount = dataSize / (UInt32)this.blockSize;
        }
    }
}
