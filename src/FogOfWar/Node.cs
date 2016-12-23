using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FogOfWar
{
    internal class Node
    {
        private BigInteger originalZ;

        public Map Map { get; }

        public BigInteger Z { get; private set; }

        private BigInteger PrivateExponent { get; }

        public Initilizer Initilize { get; }
        public Scanner Scan { get; }
        public bool IsInitilized => this.Initilize.Phase == Initilizer.PhaseState.Finished;

        public Node(Map map)
        {
            this.Map = map;
            this.Initilize = new Initilizer(this);
            this.Scan = new Scanner(this);


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
                // Hope I understood it corect.
                return BigInteger.ModPow(privateExponent, this.parent.Map.Prime - 2, this.parent.Map.Prime);
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
            private BigInteger initialZ;
            internal PhaseState Phase { get; private set; }
            public Initilizer(Node parent)
            {
                this.parent = parent;
                this.Phase = PhaseState.Phase0;
            }

            /// <summary>
            /// First generate your part of Z and send it to the other Party
            /// </summary>
            /// <returns>Your part of Z</returns>
            public BigInteger Phase0()
            {
                if (this.Phase != PhaseState.Phase0)
                    throw new InvalidOperationException();
                this.initialZ = CryptoHelper.Random(this.parent.Map.Prime); // create our part of Z
                this.Phase = PhaseState.Phase1;
                return this.initialZ;
            }

            /// <summary>
            /// Using the others Z to create the Original Z and Z ^ your exponent
            /// </summary>
            /// <param name="otherZ">the others part of Z</param>
            /// <returns>Original Z ^ private exponent</returns>
            public BigInteger Phase1(BigInteger otherZ)
            {
                if (this.Phase != PhaseState.Phase1)
                    throw new InvalidOperationException();
                this.parent.originalZ = otherZ ^ this.initialZ; // should be safe because z must only be smaler than p and z_1 ^ z_2 <= Min(z_1,z_2) <= p

                var firstExponent = BigInteger.ModPow(this.parent.originalZ, this.parent.PrivateExponent, this.parent.Map.Prime);
                this.Phase = PhaseState.Phase2;
                return firstExponent;
            }

            /// <summary>
            /// Calculats Z using the other Partys Original Z ^ other Private exponent . After this Initialisation is Finished.
            /// </summary>
            /// <param name="exchange">Original Z ^ other Private exponent </param>
            public void Phase2(BigInteger exchange)
            {
                if (this.Phase != PhaseState.Phase2)
                    throw new InvalidOperationException();

                var combined = BigInteger.ModPow(exchange, this.parent.PrivateExponent, this.parent.Map.Prime);
                this.parent.Z = combined;
                this.Phase = PhaseState.Finished;
            }

            internal enum PhaseState
            {
                Phase0,
                Phase1,
                Phase2,
                Finished
            }
        }
    }
}
