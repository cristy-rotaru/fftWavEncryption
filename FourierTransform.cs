using System;
using System.Numerics;

namespace fftWavEncryption
{
    class FourierTransform
    {
        private const int segmentSize = 0x2000;

        /// <summary>
        /// This function calculates direct DFT on a float vector. This function might extend the vector in order for it's size to match the algorith's requirements.
        /// </summary>
        /// <param name="samples">The vector on which direct DFT will be calculated.</param>
        /// <returns>A complex vector with DFT values.</returns>
        public static Complex[] FFT_segmented(float[] samples)
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

            int sizeExtended;
            if (size % segmentSize == 0)
            {
                sizeExtended = size;
            }
            else
            {
                sizeExtended = (size / segmentSize + 1) * segmentSize;
            }

            Complex[] vector = new Complex[sizeExtended];
            for (int i = 0; i < sizeExtended; i += segmentSize)
            {
                Complex[] segment = new Complex[segmentSize];
                for (int j = 0; j < segmentSize; ++j)
                {
                    segment[j] = (i + j < size) ? new Complex(samples[i + j], 0.0) : new Complex(0.0, 0.0);
                }

                Accord.Math.FourierTransform.FFT(segment, Accord.Math.FourierTransform.Direction.Forward);

                Array.Copy(segment, 0, vector, i, segmentSize);
            }

            return vector;
        }

        /// <summary>
        /// This function yilds the reverse result of calling FFT_segmented
        /// </summary>
        /// <param name="frequences">A complex vector with the spectrum of the signal.</param>
        /// <returns>Return the time samples of the signal.</returns>
        public static float[] IFFT_segmented(Complex[] frequences)
        {
            if (frequences == null)
            {
                return null;
            }

            int size = frequences.Length;

            if (size == 0)
            {
                return new float[0];
            }

            if (size % segmentSize != 0) // size is power of 2
            {
                throw new Exception("Vector size does not match specifications.");
            }

            float[] samples = new float[size];
            for (int i = 0; i < size; i += segmentSize)
            {
                Complex[] segment = new Complex[segmentSize];
                Array.Copy(frequences, i, segment, 0, segmentSize);

                Accord.Math.FourierTransform.FFT(segment, Accord.Math.FourierTransform.Direction.Backward);

                for (int j = 0; j < segmentSize; ++j)
                {
                    samples[i + j] = (float)segment[j].Real;
                }
            }

            return samples;
        }
    }
}
