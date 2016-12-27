using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Zelos.FogOfWar.Prototype
{
    [DataContract(IsReference = true)]
    public class Map
    {
        public IReadOnlyCollection<Node> Nodes => this.nodes.AsReadOnly();

        [DataMember]
        internal readonly List<Node> nodes = new List<Node>();

        [DataMember]
        internal readonly Guid id = Guid.NewGuid();

        public Node CreateNode()
        {
            var newNode = new Node(this.nodes.Count, this);
            this.nodes.Add(newNode);
            return newNode;
        }

        public bool IsConnected(Node n1, Node n2)
        {
            return n1.edges.Contains(n2) && n2.edges.Contains(n1);
        }

        public void ConnectNodes(Node n1, Node n2)
        {
            if (n1.edges.Contains(n2) || n2.edges.Contains(n1))
                throw new ArgumentException("Connection already established");
            n1.edges.Add(n2);
            n2.edges.Add(n1);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = obj as Map;
            return other.id.Equals(this.id);
        }

        public override int GetHashCode()
        {
            return this.id.GetHashCode();
        }

        internal bool DeepEquals(Map prototypeMap)
        {
            if (!this.Equals(prototypeMap))
                return false;

            if (this.Nodes.Count != prototypeMap.Nodes.Count)
                return false;

            foreach (var node in Nodes)
            {
                var index = prototypeMap.nodes.IndexOf(node);
                if (index == -1)
                    return false;
                var otherNode = prototypeMap.nodes[index];

                if (node.Edgees.Count != otherNode.edges.Count)
                    return false;

                foreach (var edge in node.Edgees)
                {
                    index = otherNode.edges.IndexOf(edge);
                    if (index == -1)
                        return false;
                    var otherEdge = otherNode.edges[index];

                    if (!edge.Equals(otherEdge))
                        return false;

                }
            }

            return true;
        }
    }
}
