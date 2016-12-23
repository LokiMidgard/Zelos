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

        internal BigInteger Prime { get; }
        public int MaxShips { get; }
        public Scanner Scan { get; }

        private Node ShadowNode { get; }
        public Initilizer Initilize { get; }

        public Map(Prototype.Map prototyp, int maxShips)
        {
            this.generatedCryptoNodes = prototyp.Nodes.Select(x => new CryptoNode { PrototypeNode = x, TrueNode = new Node(this) }).ToArray();
            this.prototypeLookup = this.generatedCryptoNodes.ToDictionary(x => x.PrototypeNode);
            this.MaxShips = maxShips;
            this.Scan = new Scanner(this);
            this.Initilize = new Initilizer(this);
            this.ShadowNode = new Node(this);
        }


        public class Initilizer
        {
            private readonly Map parent;
            public Initilizer(Map parent)
            {
                this.parent = parent;
            }

            public IEnumerable<HandOverToPhase1> Phase0()
            {
                foreach (var item in this.parent.generatedCryptoNodes)
                    yield return new HandOverToPhase1(item.TrueNode.Initilize.Phase0(), item.PrototypeNode);
            }

            public IEnumerable<HandOverToPhase2> Phase1(IEnumerable<HandOverToPhase1> fromPhaseOne)
            {
                foreach (var item in fromPhaseOne)
                {
                    var cn = this.parent.prototypeLookup[item.PrototypeNode];
                    var ownExponented = cn.TrueNode.Initilize.Phase1(item.Value);
                    this.parent.CryptoLookup = this.parent.generatedCryptoNodes.ToDictionary(x => x.TrueNode.Z);
                    yield return new HandOverToPhase2(ownExponented, cn.TrueNode.Z);
                }
            }

            public void Phase2(IEnumerable<HandOverToPhase2> fromPhaseTwo)
            {
                foreach (var item in fromPhaseTwo)
                {
                    var cn = this.parent.CryptoLookup[item.Z];
                    cn.TrueNode.Initilize.Phase2(item.OtherExponented);
                }
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
                internal BigInteger Z { get; }

                public HandOverToPhase2(BigInteger ownExponented, BigInteger z)
                {
                    this.OtherExponented = ownExponented;
                    this.Z = z;
                }
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
                this.position = ownPositions.ToArray();
                this.toProbe = positionsToProbe.ToArray();
                var blendFactor = CryptoHelper.Random(this.parent.Prime);
                foreach (var node in this.toProbe)
                {
                    var cryptoNode = this.parent.prototypeLookup[node];
                    yield return new PreparedScan(cryptoNode.TrueNode.Scan.Prepare(blendFactor));
                }
            }

            public IEnumerable<PreparedPosition> PreparePositions(IEnumerable<PreparedScan> scanns)
            {
                for (int i = 0; i < this.parent.MaxShips; i++)
                {
                    Node cryptoNode;
                    if (this.position.Length < i)
                        cryptoNode = this.parent.prototypeLookup[this.position[i]].TrueNode;
                    else
                        cryptoNode = this.parent.ShadowNode;

                    var scannArray = scanns.ToArray();

                    for (int j = 0; j < scannArray.Length; j++)
                        yield return new PreparedPosition(cryptoNode.Scan.Position(scannArray[j].Value), j);
                }
            }

            /// <summary>
            /// Returns the nodes where your openent needs to tell you his units.
            /// </summary>
            /// <param name="positions"></param>
            /// <returns></returns>
            public IEnumerable<Prototype.Node> ExecuteProbe(IEnumerable<PreparedPosition> positions)
            {
                foreach (var p in positions)
                {
                    var crypto = this.parent.prototypeLookup[this.toProbe[p.Index]];
                    var found = crypto.TrueNode.Scan.Scan(p.Value);
                    if (found)
                        yield return this.toProbe[p.Index];
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
