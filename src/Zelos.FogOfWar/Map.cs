using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Zelos.Mapping
{
    [System.Diagnostics.DebuggerDisplay("{id}", Name = "Map")]
    [DataContract(IsReference = true)]
    public class Map<TNode, TEdge>
    {
        public IReadOnlyList<Node<TNode, TEdge>> Nodes => this.nodes.AsReadOnly();

        public EdgeCollection<TNode, TEdge> Edge => this.edges;

        [DataMember]
        internal readonly List<Node<TNode, TEdge>> nodes = new List<Node<TNode, TEdge>>();

        [DataMember]
        internal readonly Guid id = Guid.NewGuid();

        [DataMember]
        private readonly EdgeCollection<TNode, TEdge> edges = new EdgeCollection<TNode, TEdge>();

        private Map()
        {

        }


        [OnDeserialized]
        private void SetValuesSeSerialized(StreamingContext context)
        {
            Initilize();

            var old = edges.lookup.ToArray();
            edges.lookup.Clear();
            foreach (var o in old) // ReCalculatingHash
                edges.lookup.Add(o.Key, o.Value);
        }

        private void Initilize()
        {
            foreach (var pair in this.edges.lookup)
            {
                pair.Key.from.outgoingEdges.Add(pair.Value);
                pair.Key.to.incommingEdges.Add(pair.Value);
                pair.Value.From = pair.Key.from;
                pair.Value.To = pair.Key.to;
            }
        }

        public static IMapBuilder<TNode, TEdge> Create() => new MapBuilder(new Map<TNode, TEdge>());



        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = obj as Map<TNode, TEdge>;
            return other.id.Equals(this.id);
        }

        public override int GetHashCode()
        {
            return this.id.GetHashCode();
        }

        internal bool DeepEquals(Map<TNode, TEdge> prototypeMap)
        {
            if (!this.Equals(prototypeMap))
                return false;

            if (this.Nodes.Count != prototypeMap.Nodes.Count)
                return false;

            foreach (var node in this.Nodes)
            {
                var index = prototypeMap.nodes.IndexOf(node);
                if (index == -1)
                    return false;
                var otherNode = prototypeMap.nodes[index];

                if (node.OutgoingEdges.Count != otherNode.outgoingEdges.Count)
                    return false;

                foreach (var edge in node.OutgoingEdges)
                {
                    index = otherNode.outgoingEdges.IndexOf(edge);
                    if (index == -1)
                        return false;
                    var otherEdge = otherNode.outgoingEdges[index];

                    if (!edge.Equals(otherEdge))
                        return false;

                }
            }

            return true;
        }


        private class MapBuilder : IMapBuilder<TNode, TEdge>
        {
            private Map<TNode, TEdge> map;

            public MapBuilder(Map<TNode, TEdge> map)
            {
                this.map = map;
            }

            public IMapBuilder<TNode, TEdge> AddBidirectionalEdge(out Edge<TNode, TEdge> edge, Node<TNode, TEdge> from, Node<TNode, TEdge> to, TEdge value)
            {
                edge = new Edge<TNode, TEdge>(this.map.edges.lookup.Count, this.map, from, to, true, value);

                this.map.edges.lookup.Add((from, to), edge);
                this.map.edges.lookup.Add((to, from), edge);

                return this;
            }

            public IMapBuilder<TNode, TEdge> AddNode(out Node<TNode, TEdge> node, TNode value)
            {
                node = new Node<TNode, TEdge>(this.map.nodes.Count, this.map, value);
                this.map.nodes.Add(node);
                return this;
            }

            public IMapBuilder<TNode, TEdge> AddUnidirectionalEdge(out Edge<TNode, TEdge> edge, Node<TNode, TEdge> from, Node<TNode, TEdge> to, TEdge value)
            {
                edge = new Edge<TNode, TEdge>(this.map.edges.lookup.Count, this.map, from, to, false, value);

                this.map.edges.lookup.Add((from, to), edge);
                return this;
            }


            public Map<TNode, TEdge> GetResult()
            {
                var m = this.map;
                this.map = null;
                m.Initilize();
                return m;

            }
        }
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [DataContract]
    public class EdgeCollection<TNode, TEdge> : IReadOnlyDictionary<(Node<TNode, TEdge> from, Node<TNode, TEdge> to), Edge<TNode, TEdge>>
    {
        internal EdgeCollection() { }

        [DataMember]
        internal readonly Dictionary<(Node<TNode, TEdge> from, Node<TNode, TEdge> to), Edge<TNode, TEdge>> lookup = new Dictionary<(Node<TNode, TEdge> from, Node<TNode, TEdge> to), Edge<TNode, TEdge>>();

           public Edge<TNode, TEdge> this[Node<TNode, TEdge> from, Node<TNode, TEdge> to]
        {
            get
            {
                if (this.lookup.ContainsKey((from, to)))
                    return this.lookup[(from, to)];
                return null;
            }
        }

        #region IReadOnlyDictionary<,>
        Edge<TNode, TEdge> IReadOnlyDictionary<(Node<TNode, TEdge> from, Node<TNode, TEdge> to), Edge<TNode, TEdge>>.this[(Node<TNode, TEdge> from, Node<TNode, TEdge> to) key] => this.lookup[key];

        public int Count => this.lookup.Count;

        public IEnumerable<(Node<TNode, TEdge> from, Node<TNode, TEdge> to)> Keys => ((IReadOnlyDictionary<(Node<TNode, TEdge> from, Node<TNode, TEdge> to), Edge<TNode, TEdge>>)this.lookup).Keys;

        public IEnumerable<Edge<TNode, TEdge>> Values => ((IReadOnlyDictionary<(Node<TNode, TEdge> from, Node<TNode, TEdge> to), Edge<TNode, TEdge>>)this.lookup).Values;

        bool IReadOnlyDictionary<(Node<TNode, TEdge> from, Node<TNode, TEdge> to), Edge<TNode, TEdge>>.ContainsKey((Node<TNode, TEdge> from, Node<TNode, TEdge> to) key) => this.lookup.ContainsKey(key);

        public IEnumerator<KeyValuePair<(Node<TNode, TEdge> from, Node<TNode, TEdge> to), Edge<TNode, TEdge>>> GetEnumerator() => ((IReadOnlyDictionary<(Node<TNode, TEdge> from, Node<TNode, TEdge> to), Edge<TNode, TEdge>>)this.lookup).GetEnumerator();

        bool IReadOnlyDictionary<(Node<TNode, TEdge> from, Node<TNode, TEdge> to), Edge<TNode, TEdge>>.TryGetValue((Node<TNode, TEdge> from, Node<TNode, TEdge> to) key, out Edge<TNode, TEdge> value) => this.lookup.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => ((IReadOnlyDictionary<(Node<TNode, TEdge> from, Node<TNode, TEdge> to), Edge<TNode, TEdge>>)this.lookup).GetEnumerator();
        #endregion
    }

    public static class Map
    {
        public static IMapBuilder Create() => new MapBuilder(Map<object, object>.Create());

        private class MapBuilder : IMapBuilder
        {
            private readonly IMapBuilder<object, object> m;

            public MapBuilder(IMapBuilder<object, object> mapBuilder)
            {
                this.m = mapBuilder;
            }

            public IMapBuilder AddBidirectionalEdge(out Edge<object, object> edge, Node<object, object> from, Node<object, object> to)
            {
                this.m.AddBidirectionalEdge(out edge, from, to, null);
                return this;
            }

            public IMapBuilder AddNode(out Node<object, object> node)
            {
                this.m.AddNode(out node, null);
                return this;
            }

            public IMapBuilder AddUnidirectionalEdge(out Edge<object, object> edge, Node<object, object> from, Node<object, object> to)
            {
                this.m.AddUnidirectionalEdge(out edge, from, to, null);
                return this;
            }

            public Map<object, object> GetResult()
            {
                return this.m.GetResult();
            }
        }
    }
}
