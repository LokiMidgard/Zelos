using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Zelos.FogOfWar.Prototype
{
    [DataContract(IsReference = true)]
    public sealed class Node
    {
        [DataMember]
        internal readonly List<Node> edges = new List<Node>();
        [DataMember]
        internal readonly int id;
        [DataMember]
        public Map Map { get; internal set; }

        public IReadOnlyCollection<Node> Edgees => this.edges.AsReadOnly();

        internal Node(int id, Map map)
        {
            this.Map = map;
            this.id = id;
        }

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
