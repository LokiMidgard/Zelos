using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Zelos.Common.Crypto;

namespace Zelos.FogOfWar
{
    internal class Node
    {
        private BigInteger originalZ;

        public Map Map { get; }

        public BigInteger Z { get; private set; }

        private BigInteger PrivateExponent { get; set; }
        private BigInteger InverseExponent { get; set; }

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
            private BigInteger inverseBlendFactor;

            public ScannerState State { get; private set; } = ScannerState.None;

            public Scanner(Node parent)
            {
                this.parent = parent;
            }

            public BigInteger Prepare(BigInteger blendFactor, BigInteger inverseBlendFactor)
            {
                if (this.State != ScannerState.None)
                    throw new InvalidOperationException();
                this.blendFactor = blendFactor;
                this.inverseBlendFactor = inverseBlendFactor;
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
                var encrypted = BigInteger.ModPow(prepared, this.parent.InverseExponent, this.parent.Map.Prime);
                return encrypted;
            }

            public async Task<bool> ScanAsync(IEnumerable<BigInteger> collection)
            {
                if (this.State != ScannerState.Prepared)
                    throw new InvalidOperationException();

                try
                {
                    var result = await Task.Run(() => Parallel.ForEach(collection, (element, state) =>
                    {
                        var stripedZ = BigInteger.ModPow(BigInteger.ModPow(element, this.inverseBlendFactor, this.parent.Map.Prime), this.parent.InverseExponent, this.parent.Map.Prime);
                        if (stripedZ == this.parent.originalZ)
                            state.Stop();
                    }));

                    return !result.IsCompleted; // When completed then we did not find any value
                }
                finally
                {
                    this.State = ScannerState.None;
                }
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
                (this.parent.PrivateExponent, this.parent.InverseExponent) = Generate.InversableExponent(parent.Map.Prime);

                this.initialZ = Generate.Random(this.parent.Map.Prime); // create our part of Z
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
                this.parent.originalZ = (otherZ ^ this.initialZ) % this.parent.Map.Prime;

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
