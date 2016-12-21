using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FogOfWar
{
    class Node
    {
        public Map Map { get; }

        public BigInteger Z { get; private set; }
        public BigInteger OriginalZ { get; }

        internal BigInteger PrivateExponent { get; }

        public Node(Map map)
        {
            this.Map = map;
            this.OriginalZ = CryptoHelper.Random(this.Map.Prime);
            this.Z = this.OriginalZ;
        }

        public async Task Init()
        {
            
        }

    }
}
