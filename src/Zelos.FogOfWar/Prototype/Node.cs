using System;
using System.Collections.Generic;
using System.Text;

namespace Zelos.FogOfWar.Prototype
{
    public sealed class Node
    {
        internal readonly List<Node> edges = new List<Node>();
        private readonly int id;

        internal Node(int id, Map map)
        {
            this.Map = map;
            this.id = id;
            this.Edgees = this.edges.AsReadOnly();
        }

        public Map Map { get; }

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
