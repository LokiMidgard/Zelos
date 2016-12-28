using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Zelos.Common.Crypto;
using Zelos.Scribe;

namespace Zelos.Mapping.FogOfWar
{
    public class FogMap<TNode, TEdged>
    {
        private readonly EntangelNode[] generatedCryptoNodes;
        private readonly Dictionary<Node<TNode, TEdged>, EntangelNode> prototypeLookup;
        private readonly Dictionary<int, EntangelNode> prototypeIdLookup;
        private Dictionary<BigInteger, EntangelNode> CryptoLookup;

        internal BigInteger Prime { get; private set; }
        public int MaxShips { get; }
        public Scanner Scan { get; }

        private CryptoNode<TNode, TEdged> ShadowNode { get; }
        public Initilizer Initilize { get; }
        public bool IsInitilzied => this.Initilize.Phase == Initilizer.PhaseState.Finished;

        public Map<TNode, TEdged> PrototypeMap { get; }

        private FogMap(Map<TNode, TEdged> prototyp, int maxShips, BigInteger prime)
        {
            this.generatedCryptoNodes = prototyp.Nodes.Select(x => new EntangelNode { PrototypeNode = x, TrueNode = new CryptoNode<TNode, TEdged>(this) }).ToArray();
            this.prototypeLookup = this.generatedCryptoNodes.ToDictionary(x => x.PrototypeNode);
            this.prototypeIdLookup = this.generatedCryptoNodes.ToDictionary(x => x.PrototypeNode.id);
            this.PrototypeMap = prototyp;
            this.MaxShips = maxShips;
            this.Scan = new Scanner(this);
            this.Initilize = new Initilizer(this);
            this.ShadowNode = new CryptoNode<TNode, TEdged>(this);
            this.Prime = prime;
        }

        public static FogMap<TNode, TEdged> Create(Map<TNode, TEdged> prototyp, int maxShips, BigInteger prime)
        {
            return new FogMap<TNode, TEdged>(prototyp, maxShips, prime);
        }

        public class Initilizer
        {
            internal PhaseState Phase { get; private set; }
            private readonly FogMap<TNode, TEdged> parent;
            public Initilizer(FogMap<TNode, TEdged> parent)
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
                    var result = new HandOverToPhase1() { Nodes = new NodeToPhase1[this.parent.generatedCryptoNodes.Length], Map = this.parent.PrototypeMap };
                    for (int i = 0; i < result.Nodes.Length; i++)
                    {
                        var item = this.parent.generatedCryptoNodes[i];
                        result.Nodes[i] = new NodeToPhase1() { Value = item.TrueNode.Initilize.Phase0(), PrototypeIdNode = item.PrototypeNode.id };
                    }
                    result.ShadowNode = new NodeToPhase1() { Value = this.parent.ShadowNode.Initilize.Phase0(), PrototypeIdNode = -1 };
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

                    if (!fromPhaseOne.Map.DeepEquals(this.parent.PrototypeMap))
                        throw new ArgumentException("Using diferent Maps");

                    var result = new HandOverToPhase2() { Nodes = new NodeToPhase2[fromPhaseOne.Nodes.Length] };

                    for (int i = 0; i < result.Nodes.Length; i++)
                    {
                        var item = fromPhaseOne.Nodes[i];
                        var cn = this.parent.prototypeIdLookup[item.PrototypeIdNode];
                        var ownExponented = cn.TrueNode.Initilize.Phase1(item.Value);
                        result.Nodes[i] = new NodeToPhase2() { OtherExponented = ownExponented, PrototypeIdNode = cn.PrototypeNode.id };
                    }
                    result.ShadowNode = new NodeToPhase2() { OtherExponented = this.parent.ShadowNode.Initilize.Phase1(fromPhaseOne.ShadowNode.Value), PrototypeIdNode = -1 };
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
                       var cn = this.parent.prototypeIdLookup[item.PrototypeIdNode];
                       cn.TrueNode.Initilize.Phase2(item.OtherExponented);
                   }
                   this.parent.ShadowNode.Initilize.Phase2(fromPhaseTwo.ShadowNode.OtherExponented);
                   this.parent.CryptoLookup = this.parent.generatedCryptoNodes.ToDictionary(x => x.TrueNode.Z);

                   this.Phase = PhaseState.Finished;
               });
            }


            public class HandOverToPhase1 : Scribe.AbstractScripture
            {
                [Scribe.ScriptureValue(Scribe.ScriptureValueType.Public)]
                internal NodeToPhase1[] Nodes { get; set; }
                [Scribe.ScriptureValue(Scribe.ScriptureValueType.Public)]
                internal NodeToPhase1 ShadowNode { get; set; }

                [Scribe.ScriptureValue(Scribe.ScriptureValueType.Public)]
                internal Map<TNode, TEdged> Map { get; set; }

                protected override byte[] ToBytes(object o)
                {
                    if (o is Map<TNode, TEdged> m)
                    {
                        return m.id.ToByteArray();
                    }
                    else if (o is NodeToPhase1 p)
                    {
                        using (var mem = new MemoryStream())
                        {
                            var b = p.Value.ToByteArray();
                            mem.Write(b, 0, b.Length);
                            b = BitConverter.GetBytes(p.PrototypeIdNode);
                            mem.Write(b, 0, 4);

                            return mem.ToArray();
                        }

                    }
                    else if (o is NodeToPhase1[] pa)
                    {
                        using (var mem = new MemoryStream())
                        {
                            foreach (var item in pa)
                            {
                                var b = ToBytes(item);
                                mem.Write(b, 0, b.Length);
                            }
                            return mem.ToArray();
                        }
                    }
                    return base.ToBytes(o);
                }



            }
            public class HandOverToPhase2 : Scribe.AbstractScripture
            {
                [Scribe.ScriptureValue(Scribe.ScriptureValueType.Public)]
                internal NodeToPhase2[] Nodes { get; set; }
                [Scribe.ScriptureValue(Scribe.ScriptureValueType.Public)]
                internal NodeToPhase2 ShadowNode { get; set; }

                protected override byte[] ToBytes(object o)
                {
                    if (o is NodeToPhase2 p)
                    {
                        using (var mem = new MemoryStream())
                        {
                            var b = p.OtherExponented.ToByteArray();
                            mem.Write(b, 0, b.Length);
                            b = BitConverter.GetBytes(p.PrototypeIdNode);
                            mem.Write(b, 0, 4);

                            return mem.ToArray();
                        }

                    }
                    else if (o is NodeToPhase2[] pa)
                    {
                        using (var mem = new MemoryStream())
                        {
                            foreach (var item in pa)
                            {
                                var b = ToBytes(item);
                                mem.Write(b, 0, b.Length);
                            }
                            return mem.ToArray();
                        }
                    }
                    return base.ToBytes(o);
                }
            }

            internal class NodeToPhase1
            {

                internal BigInteger Value { get; set; }
                internal int PrototypeIdNode { get; set; }

                public override bool Equals(object obj)
                {
                    if (obj == null || GetType() != obj.GetType())
                        return false;

                    var other = obj as NodeToPhase1;
                    return other.Value == this.Value && other.PrototypeIdNode == this.PrototypeIdNode;
                }

                // override object.GetHashCode
                public override int GetHashCode()
                {
                    return this.Value.GetHashCode() * 31 + this.PrototypeIdNode;
                }

            }

            internal class NodeToPhase2
            {
                internal BigInteger OtherExponented { get; set; }
                internal int PrototypeIdNode { get; set; }

                public override bool Equals(object obj)
                {
                    if (obj == null || GetType() != obj.GetType())
                        return false;

                    var other = obj as NodeToPhase2;
                    return other.OtherExponented == this.OtherExponented && other.PrototypeIdNode == this.PrototypeIdNode;
                }

                // override object.GetHashCode
                public override int GetHashCode()
                {
                    return this.OtherExponented.GetHashCode() * 31 + this.PrototypeIdNode;
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
            private readonly FogMap<TNode, TEdged> parent;
            private Mapping.Node<TNode, TEdged>[] position;
            private Mapping.Node<TNode, TEdged>[] toProbe;

            internal Scanner(FogMap<TNode, TEdged> parent)
            {
                this.parent = parent;
            }

            public Task<PScan> PrepareForPropeAsync(IEnumerable<Mapping.Node<TNode, TEdged>> ownPositions, IEnumerable<Mapping.Node<TNode, TEdged>> positionsToProbe)
            {
                (var blendFactor, var inverseBlendfactor) = Generate.InversableExponent(this.parent.Prime);
                this.position = ownPositions.ToArray();
                this.toProbe = positionsToProbe.ToArray();
                var result = new PScan() { Scanns = new PreparedScan[this.toProbe.Length], BlendFactor = blendFactor };
                if (this.position.Length > this.parent.MaxShips)
                    throw new ArgumentOutOfRangeException("To many Positions");
                return Task.Run(() =>
                {
                    Parallel.For(0, result.Scanns.Length, i =>
                    {
                        var node = this.toProbe[i];
                        var cryptoNode = this.parent.prototypeLookup[node];
                        result.Scanns[i] = new PreparedScan() { Value = cryptoNode.TrueNode.Scan.Prepare(blendFactor, inverseBlendfactor) };

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
                       CryptoNode<TNode, TEdged> cryptoNode;
                       if (i < this.position.Length)
                           cryptoNode = this.parent.prototypeLookup[this.position[i]].TrueNode;
                       else
                           cryptoNode = this.parent.ShadowNode;

                       //for (int j = 0; j < scanns.Scanns.Length; j++)
                       result.Positions[i * scanns.Scanns.Length + j] = new PreparedPosition() { Value = cryptoNode.Scan.Position(scanns.Scanns[j].Value), Index = j };
                   });
                   return result;
               });
            }

            /// <summary>
            /// Returns the nodes where your openent needs to tell you his units.
            /// </summary>
            /// <param name="positions"></param>
            /// <returns></returns>
            public async Task<ICollection<Mapping.Node<TNode, TEdged>>> ExecuteProbeAsync(PPosition positions)
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


            public class PPosition : Scribe.AbstractScripture
            {
                [Scribe.ScriptureValue(Scribe.ScriptureValueType.Public)]
                internal PreparedPosition[] Positions { get; set; }
                protected override byte[] ToBytes(object o)
                {
                    if (o is PreparedPosition[] p)
                    {
                        using (var mem = new MemoryStream())
                        {
                            foreach (var item in p)
                            {
                                var b = item.Value.ToByteArray();
                                mem.Write(b, 0, b.Length);
                                mem.Write(BitConverter.GetBytes(item.Index), 0, 4);
                            }
                            return mem.ToArray();
                        }
                    }
                    return base.ToBytes(o);
                }
            }

            internal class PreparedPosition
            {
                internal BigInteger Value { get; set; }

                internal int Index { get; set; }

                public override bool Equals(object obj)
                {
                    if (obj == null || GetType() != obj.GetType())
                        return false;

                    var other = obj as PreparedPosition;
                    return other.Value == this.Value && other.Index == this.Index;
                }

                // override object.GetHashCode
                public override int GetHashCode()
                {
                    return this.Value.GetHashCode() * 31 + Index;
                }


            }

            public class PScan : Scribe.AbstractScripture
            {
                [Scribe.ScriptureValue(Scribe.ScriptureValueType.Secret)]
                internal BigInteger BlendFactor { get; set; }

                [Scribe.ScriptureValue(Scribe.ScriptureValueType.Public)]
                internal PreparedScan[] Scanns { get; set; }

                protected override byte[] ToBytes(object o)
                {
                    if (o is PreparedScan[] p)
                    {
                        using (var mem = new MemoryStream())
                        {
                            foreach (var item in p)
                            {
                                var b = item.Value.ToByteArray();
                                mem.Write(b, 0, b.Length);
                            }
                            return mem.ToArray();
                        }
                    }
                    return base.ToBytes(o);
                }
            }

            internal class PreparedScan
            {
                internal BigInteger Value { get; set; }

                public override bool Equals(object obj)
                {
                    if (obj == null || GetType() != obj.GetType())
                        return false;

                    var other = obj as PreparedScan;
                    return other.Value == this.Value;
                }

                // override object.GetHashCode
                public override int GetHashCode()
                {
                    return this.Value.GetHashCode();
                }

            }

        }



        private class EntangelNode
        {
            public Node<TNode, TEdged> PrototypeNode { get; set; }
            public CryptoNode<TNode, TEdged> TrueNode { get; set; }
        }
    }

    public static class FogMap
    {
        public static FogMap<TNode, TEdged> Create<TNode, TEdged>(Map<TNode, TEdged> prototyp, int maxShips, BigInteger prime)
        {
            return FogMap<TNode, TEdged>.Create(prototyp, maxShips, prime);
        }

    }
}
