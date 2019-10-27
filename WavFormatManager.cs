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
            int blockSize = (int)((uint)fileFormatDescriptor[20] | ((uint)fileFormatDescriptor[21] << 8));
            this.bitsPerSample = (int)((uint)fileFormatDescriptor[22] | ((uint)fileFormatDescriptor[23] << 8));
            if (this.bitsPerSample > 32)
            {
                throw new Exception("Unsupported sample size.");
            }

            Byte[] dataHeader = new Byte[8];
            inputFile.Read(dataHeader, 0, 8);
            if (dataHeader[0] != 'd' | dataHeader[1] != 'a' | dataHeader[2] != 't' | dataHeader[3] != 'a')
            {
                throw new Exception("Data chunck not found.");
            }

            UInt32 dataSize = (UInt32)dataHeader[4] | ((UInt32)dataHeader[5] << 8) | ((UInt32)dataHeader[6] << 16) | ((UInt32)dataHeader[7] << 24);
            UInt32 sampleCount = dataSize / (UInt32)blockSize;

            // this will be used to normalize the samples when converting to float
            UInt64 maxSampleValue = ((UInt64)1 << this.bitsPerSample) - 1;

            // allocating memory for sample buffers
            this.channelLeft = new float[sampleCount];
            if (this.channelCount == 2)
            {
                this.channelRight = new float[sampleCount];
            }
            else
            {
                this.channelRight = null;
            }

            // reading, decoding and normalizing samples block by block
            Byte[] block = new Byte[blockSize];
            for (UInt32 sampleIndex = 0; sampleIndex < sampleCount; ++sampleIndex)
            {
                inputFile.Read(block, 0, blockSize);
                if (this.channelCount == 1)
                {
                    UInt64 sampleQuanta = 0;
                    for (int i = 0; i < blockSize; ++i)
                    {
                        sampleQuanta |= ((UInt64)block[i] << (i * 8));
                    }
                    Double sampleNormalized = (Double)sampleQuanta / (Double)maxSampleValue;
                    this.channelLeft[sampleIndex] = (float)sampleNormalized;
                }
                else // if (this.channelCount == 2)
                {
                    UInt64 sampleQuantaLeft = 0, sampleQuantaRight = 0; ;
                    for (int i = 0; i < blockSize / 2; ++i)
                    {
                        sampleQuantaLeft |= ((UInt64)block[i] << (i * 8));
                        sampleQuantaRight |= ((UInt64)block[i + blockSize / 2] << (int)(i * 8));
                    }
                    Double sampleNormalizedLeft = (Double)sampleQuantaLeft / (Double)maxSampleValue;
                    Double sampleNormalizedRight = (Double)sampleQuantaRight / (Double)maxSampleValue;
                    this.channelLeft[sampleIndex] = (float)sampleNormalizedLeft;
                    this.channelRight[sampleIndex] = (float)sampleNormalizedRight;
                }
            }

            inputFile.Close();
        }

        public void EncodeFile(String filename)
        {
            // checking all the conditions before starting encoding
            if (this.channelCount == 0)
            {
                throw new Exception("No audio channel has been specified.");
            }
            if ((this.channelLeft == null) || (this.channelCount == 2 && this.channelRight == null))
            {
                throw new Exception("Channel data has not been specified.");
            }
            if (this.channelCount == 2 && this.channelLeft.Length != this.channelRight.Length)
            {
                throw new Exception("Channel sizes don't match.");
            }
            if (this.sampleRate == SampleRate.NullSampleRate)
            {
                throw new Exception("Sample rate has not been specified.");
            }
            if (this.bitsPerSample == 0)
            {
                throw new Exception("Sample size has not been specified.");
            }

            FileStream outputFile = new FileStream(filename, FileMode.Create, FileAccess.Write);

            // see http://soundfile.sapp.org/doc/WaveFormat/ for a description of the wav format

            // calculating parameter sizes to be written to the file header and format descriptor
            UInt16 blockSize = (UInt16)(this.channelCount * (this.bitsPerSample / 8));
            UInt32 fileDataSize = (UInt32)(blockSize * this.channelLeft.Length);
            UInt32 fileByteRate = blockSize * (UInt32)this.sampleRate;
            UInt32 fileChunkSize = fileDataSize + 36;

            // writing .wav header
            Byte[] fileHeader = new Byte[12];
            fileHeader[0] = (Byte)'R';
            fileHeader[1] = (Byte)'I';
            fileHeader[2] = (Byte)'F';
            fileHeader[3] = (Byte)'F';
            fileHeader[4] = (Byte)(fileChunkSize & 0xFF);
            fileHeader[5] = (Byte)((fileChunkSize >> 8) & 0xFF);
            fileHeader[6] = (Byte)((fileChunkSize >> 16) & 0xFF);
            fileHeader[7] = (Byte)((fileChunkSize >> 24) & 0xFF);
            fileHeader[8] = (Byte)'W';
            fileHeader[9] = (Byte)'A';
            fileHeader[10] = (Byte)'V';
            fileHeader[11] = (Byte)'E';
            outputFile.Write(fileHeader, 0, 12);

            // writing .wav format descriptor
            Byte[] fileFormatDescriptor = new Byte[24];
            fileFormatDescriptor[0] = (Byte)'f';
            fileFormatDescriptor[1] = (Byte)'m';
            fileFormatDescriptor[2] = (Byte)'t';
            fileFormatDescriptor[3] = (Byte)' ';
            fileFormatDescriptor[4] = 0x10; // chunck size
            fileFormatDescriptor[5] = 0x00;
            fileFormatDescriptor[6] = 0x00;
            fileFormatDescriptor[7] = 0x00;
            fileFormatDescriptor[8] = 0x01; // PCM format
            fileFormatDescriptor[9] = 0x00;
            fileFormatDescriptor[10] = (Byte)this.channelCount;
            fileFormatDescriptor[11] = 0x00;
            fileFormatDescriptor[12] = (Byte)((UInt32)this.sampleRate & 0xFF);
            fileFormatDescriptor[13] = (Byte)(((UInt32)this.sampleRate >> 8) & 0xFF);
            fileFormatDescriptor[14] = (Byte)(((UInt32)this.sampleRate >> 16) & 0xFF);
            fileFormatDescriptor[15] = (Byte)(((UInt32)this.sampleRate >> 24) & 0xFF);
            fileFormatDescriptor[16] = (Byte)(fileByteRate & 0xFF);
            fileFormatDescriptor[17] = (Byte)((fileByteRate >> 8) & 0xFF);
            fileFormatDescriptor[18] = (Byte)((fileByteRate >> 16) & 0xFF);
            fileFormatDescriptor[19] = (Byte)((fileByteRate >> 24) & 0xFF);
            fileFormatDescriptor[20] = (Byte)blockSize;
            fileFormatDescriptor[21] = 0x00;
            fileFormatDescriptor[22] = (Byte)(this.bitsPerSample & 0xFF);
            fileFormatDescriptor[23] = (Byte)((this.bitsPerSample >> 8) & 0xFF);
            outputFile.Write(fileFormatDescriptor, 0, 24);

            // writing .wav data description
            Byte[] dataDescription = new Byte[8];
            dataDescription[0] = (Byte)'d';
            dataDescription[1] = (Byte)'a';
            dataDescription[2] = (Byte)'t';
            dataDescription[3] = (Byte)'a';
            dataDescription[4] = (Byte)(fileDataSize & 0xFF);
            dataDescription[5] = (Byte)((fileDataSize >> 8) & 0xFF);
            dataDescription[6] = (Byte)((fileDataSize >> 16) & 0xFF);
            dataDescription[7] = (Byte)((fileDataSize >> 24) & 0xFF);
            outputFile.Write(dataDescription, 0, 8);

            // writing sound data
            UInt64 maxSampleValue = ((UInt64)1 << this.bitsPerSample) - 1;
            Byte[] block = new Byte[blockSize];
            for (int sampleIndex = 0; sampleIndex < this.channelLeft.Length; ++sampleIndex)
            {
                if (this.channelCount == 1)
                {
                    UInt32 sampleQuanta = (UInt32)((Double)this.channelLeft[sampleIndex] * (Double)maxSampleValue);
                    for (int i = 0; i < blockSize; ++i)
                    {
                        block[i] = (Byte)((sampleQuanta >> (i * 8)) & 0xFF);
                    }
                }
                else // if (this.channelCount == 2)
                {
                    UInt32 sampleQuantaLeft = (UInt32)((Double)this.channelLeft[sampleIndex] * (Double)maxSampleValue);
                    UInt32 sampleQuantaRight = (UInt32)((Double)this.channelRight[sampleIndex] * (Double)maxSampleValue);
                    for (int i = 0; i < blockSize / 2; ++i)
                    {
                        block[i] = (Byte)((sampleQuantaLeft >> (i * 8)) & 0xFF);
                        block[i + blockSize / 2] = (Byte)((sampleQuantaRight >> (i * 8)) & 0xFF);
                    }
                }

                outputFile.Write(block, 0, blockSize);
            }

            outputFile.Flush();
            outputFile.Close();
        }
    }
}
