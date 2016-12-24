using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using FogOfWar.Prototype;

namespace FogOfWar
{
    public class Map
    {
        private readonly CryptoNode[] generatedCryptoNodes;
        private readonly Dictionary<Prototype.Node, CryptoNode> prototypeLookup;
        private Dictionary<BigInteger, CryptoNode> CryptoLookup;

        internal BigInteger Prime { get; private set; }
        public int MaxShips { get; }
        public Scanner Scan { get; }

        private Node ShadowNode { get; }
        public Initilizer Initilize { get; }
        public bool IsInitilzied => this.Initilize.Phase == Initilizer.PhaseState.Finished;

        public Map(Prototype.Map prototyp, int maxShips, BigInteger prime)
        {
            this.generatedCryptoNodes = prototyp.Nodes.Select(x => new CryptoNode { PrototypeNode = x, TrueNode = new Node(this) }).ToArray();
            this.prototypeLookup = this.generatedCryptoNodes.ToDictionary(x => x.PrototypeNode);
            this.MaxShips = maxShips;
            this.Scan = new Scanner(this);
            this.Initilize = new Initilizer(this);
            this.ShadowNode = new Node(this);
            this.Prime = prime;
        }


        public class Initilizer
        {
            internal PhaseState Phase { get; private set; }
            private readonly Map parent;
            public Initilizer(Map parent)
            {
                this.parent = parent;
            }

            public IEnumerable<HandOverToPhase1> Phase0()
            {
                //this.parent.Prime = CryptoHelper.GeneratePrime();
                if (this.Phase != PhaseState.Phase0)
                    throw new InvalidOperationException();
                this.Phase = PhaseState.Phase1;

                return InternalPhase0().ToArray();

                IEnumerable<HandOverToPhase1> InternalPhase0()
                {
                    foreach (var item in this.parent.generatedCryptoNodes)
                        yield return new HandOverToPhase1(item.TrueNode.Initilize.Phase0(), item.PrototypeNode);
                }
            }


            public IEnumerable<HandOverToPhase2> Phase1(IEnumerable<HandOverToPhase1> fromPhaseOne)
            {
                if (this.Phase != PhaseState.Phase1)
                    throw new InvalidOperationException();
                this.Phase = PhaseState.Phase2;

                return InternalPhase1(fromPhaseOne).ToArray();

                IEnumerable<HandOverToPhase2> InternalPhase1(IEnumerable<HandOverToPhase1> fromPhase1)
                {
                    foreach (var item in fromPhase1)
                    {
                        var cn = this.parent.prototypeLookup[item.PrototypeNode];
                        var ownExponented = cn.TrueNode.Initilize.Phase1(item.Value);
                        yield return new HandOverToPhase2(ownExponented, cn.PrototypeNode);
                    }

                }
            }

            public void Phase2(IEnumerable<HandOverToPhase2> fromPhaseTwo)
            {
                if (this.Phase != PhaseState.Phase2)
                    throw new InvalidOperationException();

                foreach (var item in fromPhaseTwo)
                {
                    var cn = this.parent.prototypeLookup[item.PrototypeNode];
                    cn.TrueNode.Initilize.Phase2(item.OtherExponented);
                }
                this.parent.CryptoLookup = this.parent.generatedCryptoNodes.ToDictionary(x => x.TrueNode.Z);

                this.Phase = PhaseState.Finished;
            }



            public class HandOverToPhase1
            {
                internal BigInteger Value { get; }
                internal Prototype.Node PrototypeNode { get; }


                public HandOverToPhase1(BigInteger bigInteger, Prototype.Node prototypeNode)
                {
                    this.Value = bigInteger;
                    this.PrototypeNode = prototypeNode;
                }
            }

            public class HandOverToPhase2
            {
                internal BigInteger OtherExponented { get; }
                internal Prototype.Node PrototypeNode { get; }

                public HandOverToPhase2(BigInteger ownExponented, Prototype.Node prototypeNode)
                {
                    this.OtherExponented = ownExponented;
                    this.PrototypeNode = prototypeNode;
                }
            }
            internal enum PhaseState
            {
                Phase0,
                Phase1,
                Phase2,
                Finished
            }

        }

        public class Scanner
        {
            private readonly Map parent;
            private Prototype.Node[] position;
            private Prototype.Node[] toProbe;

            internal Scanner(Map parent)
            {
                this.parent = parent;
            }

            public IEnumerable<PreparedScan> PrepareForPrope(IEnumerable<Prototype.Node> ownPositions, IEnumerable<Prototype.Node> positionsToProbe)
            {
                (var blendFactor, var inverseBlendfactor) = CryptoHelper.GenerateExponent(this.parent.Prime);
                IEnumerable<PreparedScan> InternalPrepareForPrope()
                {
                    foreach (var node in this.toProbe)
                    {
                        var cryptoNode = this.parent.prototypeLookup[node];
                        yield return new PreparedScan(cryptoNode.TrueNode.Scan.Prepare(blendFactor, inverseBlendfactor));
                    }
                }

                this.position = ownPositions.ToArray();
                if (this.position.Length > this.parent.MaxShips)
                    throw new ArgumentOutOfRangeException("To many Positions");
                this.toProbe = positionsToProbe.ToArray();
                return InternalPrepareForPrope().ToArray();

            }

            public IEnumerable<PreparedPosition> PreparePositions(IEnumerable<PreparedScan> scanns)
            {
                return InternalPreparePositions().ToArray();

                IEnumerable<PreparedPosition> InternalPreparePositions()
                {
                    for (int i = 0; i < this.parent.MaxShips; i++)
                    {
                        Node cryptoNode;
                        if (i < this.position.Length)
                            cryptoNode = this.parent.prototypeLookup[this.position[i]].TrueNode;
                        else
                            cryptoNode = this.parent.ShadowNode;

                        var scannArray = scanns.ToArray();

                        for (int j = 0; j < scannArray.Length; j++)
                            yield return new PreparedPosition(cryptoNode.Scan.Position(scannArray[j].Value), j);
                    }
                }
            }

            /// <summary>
            /// Returns the nodes where your openent needs to tell you his units.
            /// </summary>
            /// <param name="positions"></param>
            /// <returns></returns>
            public IEnumerable<Prototype.Node> ExecuteProbe(IEnumerable<PreparedPosition> positions)
            {
                return InternalExecuteProbe().ToArray();
                IEnumerable<Prototype.Node> InternalExecuteProbe()
                {
                    foreach (var p in positions.GroupBy(x => x.Index, x => x.Value))
                    {
                        var node = this.toProbe[p.Key];
                        var crypto = this.parent.prototypeLookup[node];
                        var found = crypto.TrueNode.Scan.Scan(p);
                        if (found)
                            yield return node;
                    }
                }
            }

            public class PreparedPosition
            {
                internal BigInteger Value { get; }

                internal int Index { get; }

                public PreparedPosition(BigInteger value, int index)
                {
                    this.Value = value;
                    this.Index = index;
                }
            }

            public class PreparedScan
            {
                internal BigInteger Value { get; }

                internal PreparedScan(BigInteger value)
                {
                    this.Value = value;
                }
            }

        }


        private class CryptoNode
        {
            public Prototype.Node PrototypeNode { get; set; }
            public Node TrueNode { get; set; }
        }
    }
}
