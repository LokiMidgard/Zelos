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

        public BigInteger Prime { get; }

        public Map(Prototype.Map prototyp, int maxShips)
        {

            this.generatedCryptoNodes = prototyp.Nodes.Select(x => new CryptoNode { PrototypeNode = x, TrueNode = new Node(this), ShadowNode = new Node(this) });


        }

        private class CryptoNode
        {
            public Prototype.Node PrototypeNode { get; set; }
            public Node TrueNode { get; set; }
            public Node ShadowNode { get; set; }
        }
    }
}
