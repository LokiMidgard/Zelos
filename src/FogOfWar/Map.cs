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

            public Task<HandOverToPhase1> Phase0Async()
            {
                //this.parent.Prime = CryptoHelper.GeneratePrime();
                if (this.Phase != PhaseState.Phase0)
                    throw new InvalidOperationException();
                this.Phase = PhaseState.Phase1;

                return Task.Run(() =>
                {
                    var result = new HandOverToPhase1() { Nodes = new NodeToPhase1[this.parent.generatedCryptoNodes.Length] };
                    for (int i = 0; i < result.Nodes.Length; i++)
                    {
                        var item = this.parent.generatedCryptoNodes[i];
                        result.Nodes[i] = new NodeToPhase1(item.TrueNode.Initilize.Phase0(), item.PrototypeNode);
                    }
                    result.ShadowNode = new NodeToPhase1(this.parent.ShadowNode.Initilize.Phase0(), null);
                    return result;
                });
            }


            public Task<HandOverToPhase2> Phase1Async(HandOverToPhase1 fromPhaseOne)
            {
                if (this.Phase != PhaseState.Phase1)
                    throw new InvalidOperationException();
                this.Phase = PhaseState.Phase2;

                return Task.Run(() =>
                {
                    var result = new HandOverToPhase2() { Nodes = new NodeToPhase2[fromPhaseOne.Nodes.Length] };

                    for (int i = 0; i < result.Nodes.Length; i++)
                    {
                        var item = fromPhaseOne.Nodes[i];
                        var cn = this.parent.prototypeLookup[item.PrototypeNode];
                        var ownExponented = cn.TrueNode.Initilize.Phase1(item.Value);
                        result.Nodes[i] = new NodeToPhase2(ownExponented, cn.PrototypeNode);
                    }
                    result.ShadowNode = new NodeToPhase2(this.parent.ShadowNode.Initilize.Phase1(fromPhaseOne.ShadowNode.Value), null);
                    return result;
                });
            }

            public Task Phase2Async(HandOverToPhase2 fromPhaseTwo)
            {
                if (this.Phase != PhaseState.Phase2)
                    throw new InvalidOperationException();

                return Task.Run(() =>
               {

                   foreach (var item in fromPhaseTwo.Nodes)
                   {
                       var cn = this.parent.prototypeLookup[item.PrototypeNode];
                       cn.TrueNode.Initilize.Phase2(item.OtherExponented);
                   }
                   this.parent.ShadowNode.Initilize.Phase2(fromPhaseTwo.ShadowNode.OtherExponented);
                   this.parent.CryptoLookup = this.parent.generatedCryptoNodes.ToDictionary(x => x.TrueNode.Z);

                   this.Phase = PhaseState.Finished;
               });
            }


            public class HandOverToPhase1
            {
                internal NodeToPhase1[] Nodes { get; set; }
                internal NodeToPhase1 ShadowNode { get; set; }
            }
            public class HandOverToPhase2
            {
                internal NodeToPhase2[] Nodes { get; set; }
                internal NodeToPhase2 ShadowNode { get; set; }
            }

            internal class NodeToPhase1
            {
                internal BigInteger Value { get; }
                internal Prototype.Node PrototypeNode { get; }


                public NodeToPhase1(BigInteger bigInteger, Prototype.Node prototypeNode)
                {
                    this.Value = bigInteger;
                    this.PrototypeNode = prototypeNode;
                }
            }

            internal class NodeToPhase2
            {
                internal BigInteger OtherExponented { get; }
                internal Prototype.Node PrototypeNode { get; }

                public NodeToPhase2(BigInteger ownExponented, Prototype.Node prototypeNode)
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

            public Task<PScan> PrepareForPropeAsync(IEnumerable<Prototype.Node> ownPositions, IEnumerable<Prototype.Node> positionsToProbe)
            {
                (var blendFactor, var inverseBlendfactor) = CryptoHelper.GenerateExponent(this.parent.Prime);
                this.position = ownPositions.ToArray();
                this.toProbe = positionsToProbe.ToArray();
                var result = new PScan() { Scanns = new PreparedScan[this.toProbe.Length] };
                if (this.position.Length > this.parent.MaxShips)
                    throw new ArgumentOutOfRangeException("To many Positions");
                return Task.Run(() =>
                {
                    Parallel.For(0, result.Scanns.Length, i =>
                    {
                        var node = this.toProbe[i];
                        var cryptoNode = this.parent.prototypeLookup[node];
                        result.Scanns[i] = new PreparedScan(cryptoNode.TrueNode.Scan.Prepare(blendFactor, inverseBlendfactor));

                    });
                    return result;
                });


            }

            public Task<PPosition> PreparePositionsAsync(PScan scanns)
            {
                var result = new PPosition() { Positions = new PreparedPosition[this.parent.MaxShips * scanns.Scanns.Length] };

                return Task.Run(() =>
               {
                   Parallel.For(0, result.Positions.Length, index =>
                   {
                       var i = index / scanns.Scanns.Length;
                       var j = index % scanns.Scanns.Length;
                       Node cryptoNode;
                       if (i < this.position.Length)
                           cryptoNode = this.parent.prototypeLookup[this.position[i]].TrueNode;
                       else
                           cryptoNode = this.parent.ShadowNode;

                       //for (int j = 0; j < scanns.Scanns.Length; j++)
                       result.Positions[i * scanns.Scanns.Length + j] = new PreparedPosition(cryptoNode.Scan.Position(scanns.Scanns[j].Value), j);
                   });
                   return result;
               });
            }

            /// <summary>
            /// Returns the nodes where your openent needs to tell you his units.
            /// </summary>
            /// <param name="positions"></param>
            /// <returns></returns>
            public async Task<ICollection<Prototype.Node>> ExecuteProbeAsync(PPosition positions)
            {

                var searching = await Task.WhenAll(positions.Positions.GroupBy(x => x.Index, x => x.Value).Select(async p =>
                {
                    var node = this.toProbe[p.Key];
                    var crypto = this.parent.prototypeLookup[node];
                    var found = await crypto.TrueNode.Scan.ScanAsync(p);
                    if (found)
                        return node;
                    return null;

                }));

                return searching.Where(x => x != null).ToArray();
            }


            public class PPosition
            {
                internal PreparedPosition[] Positions { get; set; }
            }

            internal class PreparedPosition
            {
                internal BigInteger Value { get; }

                internal int Index { get; }

                public PreparedPosition(BigInteger value, int index)
                {
                    this.Value = value;
                    this.Index = index;
                }
            }

            public class PScan
            {
                internal PreparedScan[] Scanns { get; set; }
            }

            internal class PreparedScan
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
