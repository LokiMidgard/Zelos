using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Zelos.Common.Crypto
{
    public sealed class CalculateHash : IDisposable
    {
        private readonly IncrementalHash hashAlgorithm;

        private CalculateHash(HashAlgorithmName algorithmName)
        {
            this.hashAlgorithm = IncrementalHash.CreateHash(algorithmName);
        }

        public static CalculateHash Create(HashAlgorithms algorithm = HashAlgorithms.SHA256)
        {
            HashAlgorithmName algorithmName;

            switch (algorithm)
            {
                case HashAlgorithms.MD5:
                    algorithmName = HashAlgorithmName.MD5;
                    break;
                case HashAlgorithms.SHA1:
                    algorithmName = HashAlgorithmName.SHA1;
                    break;
                default:
                case HashAlgorithms.SHA256:
                    algorithmName = HashAlgorithmName.SHA256;
                    break;
                case HashAlgorithms.SHA384:
                    algorithmName = HashAlgorithmName.SHA384;
                    break;
                case HashAlgorithms.SHA512:
                    algorithmName = HashAlgorithmName.SHA512;
                    break;
            }
            return new CalculateHash(algorithmName);
        }

        public void AddData(byte[] data)
        {
            this.hashAlgorithm.AppendData(data);
        }

        public void Dispose()
        {
            this.hashAlgorithm.Dispose();
        }

        public byte[] GetResult()
        {
            return this.hashAlgorithm.GetHashAndReset();
        }
    }

    public enum HashAlgorithms
    {
        MD5,
        SHA1,
        SHA256,
        SHA384,
        SHA512,
    }
}
