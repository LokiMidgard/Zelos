using System.Runtime.Serialization;

namespace Zelos.Mapping
{
    [System.Diagnostics.DebuggerDisplay("{id} - {Map.id}", Name = "Edge")]
    [DataContract(IsReference = true)]
    public class Edge<TNode, TEdge>
    {
        [DataMember]
        private readonly Map<TNode, TEdge> map;
        [DataMember]
        private readonly int id;
        [DataMember]
        private readonly TEdge value;
        [DataMember]
        private bool isBidirectional;



        public Node<TNode, TEdge> From { get; internal set; }
        public Node<TNode, TEdge> To { get; internal set; }

        public Map<TNode, TEdge> Map => this.map;

        public Edge(int count, Map<TNode, TEdge> map, Node<TNode, TEdge> from, Node<TNode, TEdge> to, bool isBiDirectional, TEdge value)
        {
            this.id = count;
            this.map = map;
            this.From = from;
            this.To = to;
            this.value = value;
            this.isBidirectional = isBiDirectional;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            var other = obj as Node<TNode, TEdge>;
            return this.id == other.id && other.Map.Equals(this.Map);
        }

        public override int GetHashCode()
        {
            return this.Map.GetHashCode() + this.id * 37;
        }
    }
}