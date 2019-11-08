using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace fftWavEncryption
{
    class CryptoFFT
    {
        public static void Encrypt(Complex[] fftVector)
        {
            if (fftVector == null)
            {
                throw new Exception("Null vector as parameter.");
            }

            if (fftVector.Length % FourierTransform.segmentSize != 0)
            {
                throw new Exception("Vector size does not match the specifications.");
            }

            for (int i = 0; i < fftVector.Length; i += FourierTransform.segmentSize)
            {
                Complex swap;

                for (int j = 1; j < (FourierTransform.segmentSize >> 2); ++j)
                {
                    swap = fftVector[i + j];
                    fftVector[i + j] = fftVector[i + (FourierTransform.segmentSize >> 1) - j];
                    fftVector[i + (FourierTransform.segmentSize >> 1) - j] = swap;

                    swap = fftVector[i + j + (FourierTransform.segmentSize >> 1)];
                    fftVector[i + j + (FourierTransform.segmentSize >> 1)] = fftVector[i + FourierTransform.segmentSize - j];
                    fftVector[i + FourierTransform.segmentSize - j] = swap;
                }
            }
        }
    }
}
