using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Zelos.Mapping
{
    [System.Diagnostics.DebuggerDisplay("{id} - {Map.id}", Name = "Node")]
    [DataContract(IsReference = true)]
    public sealed class Node<TNode, TEdge>
    {
        internal List<Edge<TNode, TEdge>> outgoingEdges = new List<Edge<TNode, TEdge>>();
        internal List<Edge<TNode, TEdge>> incommingEdges = new List<Edge<TNode, TEdge>>();
        

        [DataMember]
        internal readonly int id;
        [DataMember]
        private readonly TNode value;
        [DataMember]
        private readonly Map<TNode, TEdge> map;

        public Map<TNode, TEdge> Map => map;

        public TNode Value => this.value;
        public IReadOnlyList<Edge<TNode, TEdge>> OutgoingEdges => this.outgoingEdges.AsReadOnly();
        public IReadOnlyList<Edge<TNode, TEdge>> IncommingEdges => this.incommingEdges.AsReadOnly();


        [OnDeserializing]
        private void SetValuesSeSerialized(StreamingContext context)
        {
            outgoingEdges = new List<Edge<TNode, TEdge>>();
            incommingEdges = new List<Edge<TNode, TEdge>>();
        }

        internal Node(int id, Map<TNode, TEdge> map, TNode value)
        {
            this.map = map;
            this.id = id;
            this.value = value;
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
