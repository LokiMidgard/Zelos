using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Zelos.Mapping.Test
{
    [TestClass]
    public class PrototypMapUnitTests
    {
        [TestMethod]
        public void TestNodesCreatetAreDifferent()
        {
            var map = Map.Create()
                .AddNode(out var n1)
                .AddNode(out var n2)
                .GetResult();

            Assert.AreEqual(n1, n1);
            Assert.AreEqual(n2, n2);
            Assert.AreNotEqual(n1, n2);
        }

        [TestMethod]
        public void TestNodesIsContaintInCollection()
        {
            var map = Map.Create()
                            .AddNode(out var n1)
                            .GetResult();

            CollectionAssert.Contains(map.Nodes.ToArray(), n1);
        }

        [TestMethod]
        public void TestNodesConnect()
        {
            var map = Map.Create()
                            .AddNode(out var n1)
                            .AddNode(out var n2)
                            .AddBidirectionalEdge(out var e, n1, n2)
                            .GetResult();

            CollectionAssert.Contains(n1.OutgoingEdges.ToArray(), e);
            CollectionAssert.Contains(n2.OutgoingEdges.ToArray(), e);
            CollectionAssert.Contains(n1.IncommingEdges.ToArray(), e);
            CollectionAssert.Contains(n2.IncommingEdges.ToArray(), e);

        }

        [TestMethod]
        public void TestIsConected()
        {
            var map = Map.Create()
                            .AddNode(out var n1)
                            .AddNode(out var n2)
                            .AddNode(out var n3)
                            .AddBidirectionalEdge(out var e, n1, n2)
                            .GetResult();

            Assert.IsNotNull(map.Edge[n1, n2]);
            Assert.IsNotNull(map.Edge[n2, n1]);
            Assert.IsNull(map.Edge[n3, n1]);
        }

        [TestMethod]
        public void NodesOfDiferentMapsAreNotEqual()
        {
            var map1 = Map.Create()
                .AddNode(out var n1)
                .GetResult();
            var map2 = Map.Create()
                .AddNode(out var n2)
                .GetResult();

            Assert.AreNotEqual(n1, n2);
        }


        [TestMethod]
        public void SerelizeGraph()
        {
            var map = Map.Create()
                            .AddNode(out var n1)
                            .AddNode(out var n2)
                            .AddNode(out var n3)
                            .AddBidirectionalEdge(out var e1, n1, n2)
                            .AddUnidirectionalEdge(out var e2, n2, n3)
                            .GetResult();
            Map<object, object> map2;
            var s = new System.Runtime.Serialization.DataContractSerializer(map.GetType());
            string text;
            using (var mem = new MemoryStream())
            {
                s.WriteObject(mem, map);
                text = System.Text.Encoding.UTF8.GetString(mem.ToArray());
                mem.Seek(0, SeekOrigin.Begin);
                map2 = (Map<object, object>)s.ReadObject(mem);
            }

            Assert.AreEqual(3, map2.Nodes.Count);
            Assert.AreEqual(3, map2.Edge.Count);

            Assert.IsNotNull(map2.Edge[n1, n2]); // Works because equals and hascode are reimplemented
            Assert.IsNotNull(map2.Edge[n2, n1]);
            Assert.IsNotNull(map2.Edge[n2, n3]);

            Assert.IsNull(map2.Edge[n3, n2]);

        }

    }
}
