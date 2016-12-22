using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FogOfWar
{
    class Node
    {
        private readonly BigInteger originalZ;

        public Map Map { get; }

        public BigInteger Z { get; private set; }

        internal BigInteger PrivateExponent { get; }

        public Initilizer Initilize { get; }

        public Node(Map map)
        {
            this.Map = map;
            this.Initilize = new Initilizer(this);
            this.Z = CryptoHelper.Random(this.Map.Prime);
            this.originalZ = this.Z;
            this.PrivateExponent = CryptoHelper.Random(this.Map.Prime - 1);
        }

        public class Scanner
        {
            private readonly Node parent;
            private BigInteger blendFactor;

            public ScannerState State { get; private set; } = ScannerState.None;

            public Scanner(Node parent)
            {
                this.parent = parent;
            }

            public BigInteger Prepare(BigInteger blendFactor)
            {
                if (this.State != ScannerState.None)
                    throw new InvalidOperationException();
                this.blendFactor = blendFactor;
                var scanvalue = BigInteger.ModPow(this.parent.Z, this.blendFactor, this.parent.Map.Prime);
                this.State = ScannerState.Prepared;
                return scanvalue;
            }

            /// <summary>
            /// Call This Method on every transmited prepared BigInterger (Only if your Ship is on this field)
            /// </summary>
            /// <param name="prepared"></param>
            /// <returns></returns>
            public BigInteger Position(BigInteger prepared)
            {
                var inverse = CalculateInverse(this.parent.PrivateExponent);
                var encrypted = BigInteger.ModPow(this.parent.Z, inverse, this.parent.Map.Prime);
                return encrypted;
            }

            public bool Scan(BigInteger toScann)
            {
                if (this.State != ScannerState.Prepared)
                    throw new InvalidOperationException();
                var inverseBlendFactor = CalculateInverse(this.blendFactor);
                var inverseExponent = CalculateInverse(this.parent.PrivateExponent);
                var stripedZ = BigInteger.ModPow(BigInteger.ModPow(toScann, inverseBlendFactor, this.parent.Map.Prime), inverseExponent, this.parent.Map.Prime);
                this.State = ScannerState.None;
                return stripedZ == this.parent.originalZ;
            }

            private BigInteger CalculateInverse(BigInteger privateExponent)
            {
                throw new NotImplementedException();
            }

            public enum ScannerState
            {
                None,
                Prepared,
            }
        }

        public class Initilizer
        {
            private readonly Node parent;

            public Initilizer(Node parent)
            {
                this.parent = parent;
            }

            /// <summary>
            /// Adds the own Secreet Exponent to Z (mod Prime)
            /// </summary>
            /// <returns></returns>
            public (BigInteger originalZ, BigInteger exchange) Phase1()
            {
                var firstExponent = BigInteger.ModPow(this.parent.Z, this.parent.PrivateExponent, this.parent.Map.Prime);
                return (this.parent.Z, firstExponent);
            }

            public void Phase2(BigInteger exchange)
            {
                var combined = BigInteger.ModPow(exchange, this.parent.PrivateExponent, this.parent.Map.Prime);
                this.parent.Z = combined;
            }
        }
    }
}
