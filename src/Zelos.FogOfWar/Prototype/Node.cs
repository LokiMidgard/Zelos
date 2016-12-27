using System;
using System.Collections.Generic;
using System.Text;

namespace Zelos.FogOfWar.Prototype
{
    public sealed class Node
    {
        internal List<Node> edges { get; set; } = new List<Node>();
        internal int id { get; set; }

        internal Node(int id, Map map) : this()
        {
            this.Map = map;
            this.id = id;
        }

        private Node()
        {
            this.Edgees = this.edges.AsReadOnly();

        }

        public Map Map { get; internal set; }

        [System.Runtime.Serialization.IgnoreDataMember]
        public IReadOnlyCollection<Node> Edgees { get; }


        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = obj as Node;

            return this.id == other.id && other.Map.Equals(this.Map);
        }

        public override int GetHashCode()
        {
            return this.Map.GetHashCode() + this.id * 37;
        }

    }
}
