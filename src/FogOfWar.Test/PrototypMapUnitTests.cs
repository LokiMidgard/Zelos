using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FogOfWar.Test
{
    [TestClass]
    public class PrototypMapUnitTests
    {
        [TestMethod]
        public void TestNodesCreatetAreDifferent()
        {
            var map = new FogOfWar.Prototype.Map();
            var n1 = map.CreateNode();
            var n2 = map.CreateNode();

            Assert.AreEqual(n1, n1);
            Assert.AreEqual(n2, n2);
            Assert.AreNotEqual(n1, n2);
        }

        [TestMethod]
        public void TestNodesIsContaintInCollection()
        {
            var map = new FogOfWar.Prototype.Map();
            var n1 = map.CreateNode();

            CollectionAssert.Contains(map.Nodes.ToArray(), n1);
        }

        [TestMethod]
        public void TestNodesConnect()
        {
            var map = new FogOfWar.Prototype.Map();
            var n1 = map.CreateNode();
            var n2 = map.CreateNode();

            map.ConnectNodes(n1, n2);

            CollectionAssert.Contains(n1.Edgees.ToArray(), n2);
            CollectionAssert.Contains(n2.Edgees.ToArray(), n1);
        }

        [TestMethod]
        public void TestIsConected()
        {
            var map = new FogOfWar.Prototype.Map();
            var n1 = map.CreateNode();
            var n2 = map.CreateNode();
            var n3 = map.CreateNode();

            map.ConnectNodes(n1, n2);
            Assert.IsTrue(map.IsConnected(n1, n2));
            Assert.IsTrue(map.IsConnected(n2, n1));
            Assert.IsFalse(map.IsConnected(n3, n1));
        }

        [TestMethod]
        public void NodesOfDiferentMapsAreNotEqual()
        {
            var map1 = new FogOfWar.Prototype.Map();
            var map2 = new FogOfWar.Prototype.Map();
            var n1 = map1.CreateNode();
            var n2 = map2.CreateNode();

            Assert.AreNotEqual(n1, n2);
        }

    }
}
