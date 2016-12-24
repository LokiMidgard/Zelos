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

        private BigInteger PrivateExponent { get; set; }

        public Initilizer Initilize { get; }
        public Scanner Scan { get; }
        public bool IsInitilized => this.Initilize.Phase == Initilizer.PhaseState.Finished;

        public Node(Map map)
        {
            this.Map = map;
            this.Initilize = new Initilizer(this);
            this.Scan = new Scanner(this);


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
#if DEBUG
                var inverse = CalculateInverse(blendFactor);
                var test = BigInteger.ModPow(scanvalue, inverse, this.parent.Map.Prime);
                if (test != this.parent.Z)
                    throw new Exception();
#endif
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

            public bool Scan(IEnumerable<BigInteger> collection)
            {
                if (this.State != ScannerState.Prepared)
                    throw new InvalidOperationException();

                try
                {
                    var inverseBlendFactor = CalculateInverse(this.blendFactor);
                    var inverseExponent = CalculateInverse(this.parent.PrivateExponent);
                    foreach (var toScann in collection)
                    {
                        var stripedZ = BigInteger.ModPow(BigInteger.ModPow(toScann, inverseBlendFactor, this.parent.Map.Prime), inverseExponent, this.parent.Map.Prime);
                        if (stripedZ == this.parent.originalZ)
                            return true;
                    }
                    return false;
                }
                finally
                {
                    this.State = ScannerState.None;
                }
            }

            private BigInteger CalculateInverse(BigInteger privateExponent)
            {
                //// https://de.wikipedia.org/w/index.php?title=Erweiterter_euklidischer_Algorithmus&oldid=159924219#Rekursive_Variante_2
                //(BigInteger d, BigInteger s, BigInteger t) extended_euclid(BigInteger a, BigInteger b)
                //{
                //    if (b == 0)
                //        return (a, 1, 0);
                //    var (d1, s1, t1) = extended_euclid(b, a % b);
                //    var (d, s, t) = (d1, t1, s1 - (a / b) * t1);
                //    return (d, s, t);
                //}

                //var inverse = extended_euclid(privateExponent, this.parent.Map.Prime).s;
                //while (inverse < 0)
                //    inverse += this.parent.Map.Prime;
                //return inverse;
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
                this.parent.PrivateExponent = CryptoHelper.GeneratePrime();

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

                if (this.parent.originalZ == 1)
                    this.parent.originalZ = 2;

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

            //private BigInteger CalculateInverse(BigInteger privateExponent)
            //{
            //    // Hope I understood it corect.
            //    var x = extended_euclid(privateExponent, parent.Map.Prime);
            //    return x.s;
            //    return BigInteger.ModPow(privateExponent, this.parent.Map.Prime - 2, this.parent.Map.Prime);
            //}

            //// https://de.wikipedia.org/w/index.php?title=Erweiterter_euklidischer_Algorithmus&oldid=159924219#Rekursive_Variante_2
            //public (BigInteger d, BigInteger s, BigInteger t) extended_euclid(BigInteger a, BigInteger b)
            //{
            //    if (b == 0)
            //        return (a, 1, 0);
            //    var (d1, s1, t1) = extended_euclid(b, a % b);
            //    var (d, s, t) = (d1, t1, s1 - (a / b) * t1);
            //    return (d, s, t);
            //}

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
