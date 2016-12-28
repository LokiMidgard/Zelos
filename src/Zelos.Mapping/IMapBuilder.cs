namespace Zelos.Mapping
{
    public interface IMapBuilder<TNode, TEdge>
    {
        IMapBuilder<TNode, TEdge> AddNode(out Node<TNode, TEdge> node, TNode value);
        IMapBuilder<TNode, TEdge> AddUnidirectionalEdge(out Edge<TNode, TEdge> edge, Node<TNode, TEdge> from, Node<TNode, TEdge> to, TEdge value);
        IMapBuilder<TNode, TEdge> AddBidirectionalEdge(out Edge<TNode, TEdge> edge, Node<TNode, TEdge> from, Node<TNode, TEdge> to, TEdge value);
        Map<TNode, TEdge> GetResult();
    }
    public interface IMapBuilder
    {
        IMapBuilder AddNode(out Node<object, object> node);
        IMapBuilder AddUnidirectionalEdge(out Edge<object, object> edge, Node<object, object> from, Node<object, object> to);
        IMapBuilder AddBidirectionalEdge(out Edge<object, object> edge, Node<object, object> from, Node<object, object> to);
        Map<object, object> GetResult();
    }
}