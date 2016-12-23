using System;
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
    }
}
