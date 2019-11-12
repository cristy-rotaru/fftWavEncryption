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
        public static void Encrypt(Complex[] fftVector, String password = "password")
        {
            if (fftVector == null)
            {
                throw new Exception("Null vector as parameter.");
            }

            if (fftVector.Length % FourierTransform.segmentSize != 0)
            {
                throw new Exception("Vector size does not match the specifications.");
            }

            int[] encryptionSequence = GenrateEncryptionSequence(password);

            for (int i = 0; i < fftVector.Length; i += FourierTransform.segmentSize)
            {
                Complex[] temp = new Complex[FourierTransform.segmentSize];
                Array.Copy(fftVector, i, temp, 0, FourierTransform.segmentSize);

                for (int j = 1; j < (FourierTransform.segmentSize >> 1); ++j)
                {
                    fftVector[i + encryptionSequence[j - 1]] = temp[j];
                    fftVector[i + FourierTransform.segmentSize - encryptionSequence[j - 1]] = temp[FourierTransform.segmentSize - j];
                }
            }
        }

        public static void Decrypt(Complex[] fftVector, String password = "password")
        {
            if (fftVector == null)
            {
                throw new Exception("Null vector as parameter.");
            }

            if (fftVector.Length % FourierTransform.segmentSize != 0)
            {
                throw new Exception("Vector size does not match the specifications.");
            }

            int[] decryptionSequence = GenerateDecryptionSequence(password);

            for (int i = 0; i < fftVector.Length; i += FourierTransform.segmentSize)
            {
                Complex[] temp = new Complex[FourierTransform.segmentSize];
                Array.Copy(fftVector, i, temp, 0, FourierTransform.segmentSize);

                for (int j = 1; j < (FourierTransform.segmentSize >> 1); ++j)
                {
                    fftVector[i + decryptionSequence[j - 1]] = temp[j];
                    fftVector[i + FourierTransform.segmentSize - decryptionSequence[j - 1]] = temp[FourierTransform.segmentSize - j];
                }
            }
        }

        private static int[] GenrateEncryptionSequence(String password)
        {
            if (password == null)
            {
                throw new Exception("Null object parameter.");
            }

            if (password.Length < 8)
            {
                throw new Exception("Password too short");
            }

            int sequenceLength = (FourierTransform.segmentSize >> 1) - 1;
            List<int> tempSequence = new List<int>();
            int[] encryptionSequence = new int[sequenceLength];
            for (int i = 1; i <= sequenceLength; ++i)
            {
                tempSequence.Add(i);
            }

            int j = 0;
            for (int i = 0; i < sequenceLength; ++i)
            {
                int k = j;
                int num = 0;
                for (int l = 0; l < 4; ++l)
                {
                    num |= (int)password[k] << (8 * l);
                    ++k;
                    k %= password.Length;
                }

                encryptionSequence[i] = tempSequence[num % tempSequence.Count];
                tempSequence.RemoveAt(num % tempSequence.Count);

                ++j;
                j %= password.Length;
            }

            return encryptionSequence;
        }

        private static int[] GenerateDecryptionSequence(String password)
        {
            int[] encryptionSequence = GenrateEncryptionSequence(password);
            int[] decryptionSequence = new int[encryptionSequence.Length];

            for (int i = 0; i < encryptionSequence.Length; ++i)
            {
                int idx = encryptionSequence[i] - 1;
                decryptionSequence[idx] = i + 1;
            }

            return decryptionSequence;
        }
    }
}
