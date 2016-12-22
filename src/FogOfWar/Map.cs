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
        private readonly IEnumerable<CryptoNode> generatedCryptoNodes;
        private readonly Dictionary<Prototype.Node, CryptoNode> prototypeLookup;
        private readonly Dictionary<BigInteger, CryptoNode> CryptoLookup;

        public BigInteger Prime { get; }
        public int MaxShips { get; }
        public Scanner Scan { get; }

        private Node ShadowNode { get; }

        public Map(Prototype.Map prototyp, int maxShips)
        {
            this.generatedCryptoNodes = prototyp.Nodes.Select(x => new CryptoNode { PrototypeNode = x, TrueNode = new Node(this) }).ToArray();
            this.prototypeLookup = this.generatedCryptoNodes.ToDictionary(x => x.PrototypeNode);
            this.CryptoLookup = this.generatedCryptoNodes.ToDictionary(x => x.TrueNode.Z);
            this.MaxShips = maxShips;
            this.Scan = new Scanner(this);
            this.ShadowNode = new Node(this);
        }

        public class Scanner
        {
            private readonly Map parent;
            private Prototype.Node[] position;
            private Prototype.Node[] toProbe;

            public Scanner(Map parent)
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

            public IEnumerable<PreparedPosition> PreparePositions(IEnumerable<PreparedPosition> scanns)
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
                public BigInteger Value { get; }

                public int Index { get; }

                public PreparedPosition(BigInteger value, int index)
                {
                    this.Value = value;
                    this.Index = index;
                }
            }

            public class PreparedScan
            {
                public BigInteger Value { get; }

                public PreparedScan(BigInteger value)
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
