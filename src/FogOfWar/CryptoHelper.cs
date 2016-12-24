using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace FogOfWar
{
    public static class CryptoHelper
    {
        private static readonly RandomNumberGenerator rng;

        static CryptoHelper()
        {
            rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        }


        public static BigInteger GeneratePrime()
        {
            //return 199;
            using (var rsa = System.Security.Cryptography.RSA.Create())
            {
                var prime = new BigInteger(rsa.ExportParameters(true).P.Reverse().Concat(new byte[] { 0 }).ToArray());
                ThrowIfIsNotPrime(prime); // Only Debug Build
                return prime;
            }
        }
        [Conditional("DEBUG")]
        private static void ThrowIfIsNotPrime(BigInteger a)
        {
            if (!IsPrime(a))
                throw new ArgumentException();
        }


        // http://stackoverflow.com/a/2945445/3485361
        private static bool IsPrime(BigInteger a)
        {
            if (a == 1)
                return false;
            if (a == 2 || a == 3)
                return true;
            if ((a & 1) == 0)
                return false;
            if (!((a + 1) % 6 != 0 || (a - 1) % 6 != 0))
                return false;
            var q = Sqrt(a) + 1;
            for (int v = 3; v < q; v += 2)
                if (a % v == 0)
                    return false;
            return true;
        }

        // http://stackoverflow.com/a/3432579/3485361
        private static BigInteger Sqrt(BigInteger x)
        {
            int b = 15; // this is the next bit we try 
            uint r = 0; // r will contain the result
            uint r2 = 0; // here we maintain r squared
            while (b >= 0)
            {
                uint sr2 = r2;
                uint sr = r;
                // compute (r+(1<<b))**2, we have r**2 already.
                r2 += (uint)((r << (1 + b)) + (1 << (b + b)));
                r += (uint)(1 << b);
                if (r2 > x)
                {
                    r = sr;
                    r2 = sr2;
                }
                b--;
            }
            return r;
        }

        public static BigInteger Random(BigInteger max)
        {
            var bytes = max.ToByteArray();
            rng.GetBytes(bytes);
            var randomNumber = new BigInteger(bytes);
            if (randomNumber < 0)
                randomNumber = BigInteger.Negate(randomNumber);
            if (randomNumber < max)
                return randomNumber;
            return Random(max);
        }
    }
}
