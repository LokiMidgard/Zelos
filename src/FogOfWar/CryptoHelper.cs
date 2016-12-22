using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace FogOfWar
{
    class CryptoHelper
    {
        private static readonly RandomNumberGenerator rng;

        static CryptoHelper()
        {
            rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        }


        public static BigInteger GeneratePrime()
        {
            throw new NotImplementedException();
        }

        public static BigInteger Random(BigInteger max)
        {
            var bytes = max.ToByteArray();
            rng.GetBytes(bytes);
            var randomNumber = new BigInteger(bytes);
            if (randomNumber < max)
                return randomNumber;
            return Random(max);
        }
    }
}
