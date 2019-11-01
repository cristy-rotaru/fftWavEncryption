using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace fftWavEncryption
{
    class FourierTransform
    {
        public static Complex[] FFT(float[] samples)
        {
            if (samples == null)
            {
                return null;
            }

            int size = samples.Length;

            if (size == 0)
            {
                return new Complex[0];
            }

            Complex[] vector = new Complex[size];
            for (int i = 0; i < size; ++i)
            {
                vector[i] = new Complex(samples[i], 0);
            }

            if ((size & (size - 1)) == 0) // size is power of 2
            {

            }
            else
            {

            }

            return null;
        }

        /// <summary>
        /// This function uses the Cooley-Tukey decimation-in-time radix-2 algorithm to calculate DFT on a vector who's size is a power of 2. The transformation happens in place.
        /// </summary>
        /// <param name="vector">Array to be processed.</param>
        /// <param name="inverse">Specifies if the DFT is inverse or not.</param>
        private static void FFT_radix2(Complex[] vector, bool inverse)
        {
            if (vector == null)
            {
                throw new Exception("Null array as parameter.");
            }

            int size = vector.Length;
            int levels = 0;
            for (int x = size; x > 1; x >>= 1)
            {
                ++levels;
            }

            if ((size & (size - 1)) != 0)
            {
                throw new Exception("Array size is not a power of 2.");
            }

            // computing the trigonometric table
            Complex[] expTable = new Complex[size >> 1];
            double coefficient = (inverse ? 2 : -2) * Math.PI / size;
            for (int i = 0; i < expTable.Length; ++i)
            {
                expTable[i] = Complex.Exp(new Complex(0, i * coefficient));
            }

            // bit-reverse permutation
            for (int i = 0; i < size; ++i)
            {
                int j = (int)((UInt32)ReverseBits(i) >> (32 - levels));
                if (j > i)
                {
                    Complex swap = vector[i];
                    vector[i] = vector[j];
                    vector[j] = swap;
                }
            }

            // Cooley-Tukey decimation-in-time radix-2 DFT
            for (int len = 2; ; len <<= 1)
            {
                int halfLen = len >> 1;
                int tableStep = size / len;

                for (int i = 0; i < size; i += len)
                {
                    for (int j = i, k = 0; j < i + halfLen; ++j, k += tableStep)
                    {
                        Complex temp = vector[j + halfLen] * expTable[k];
                        vector[j + halfLen] = vector[j] - temp;
                        vector[j] += temp;
                    }
                }

                if (len == size)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Reverses the bits of the number given as parameter.
        /// </summary>
        /// <param name="x">Number who's bits will be reversed.</param>
        /// <returns>Returns the result of the bit reversal.</returns>
        private static int ReverseBits(int x)
        {
            int result = 0;

            for (int i = 0; i < sizeof(int) * 8; i++, x >>= 1)
            {
                result = (result << 1) | (x & 1);
            }

            return result;
        }
    }
}
