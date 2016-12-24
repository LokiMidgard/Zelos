using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FogOfWar.Test
{
    [TestClass]
    public class SimpleCryptoUnitTests
    {
        [TestMethod]
        public void TestInitialisation()
        {
            var map = new FogOfWar.Prototype.Map();
            var n1 = map.CreateNode();
            var n2 = map.CreateNode();


            var p = CryptoHelper.GeneratePrime();
            var cMap1 = new Map(map, 1,p);
            var cMap2 = new Map(map, 1,p);

            var phase1_1 = cMap1.Initilize.Phase0().ToArray();
            var phase1_2 = cMap2.Initilize.Phase0().ToArray();

            var phase2_1 = cMap1.Initilize.Phase1(phase1_2);
            var phase2_2 = cMap2.Initilize.Phase1(phase1_1);

            cMap1.Initilize.Phase2(phase2_2);
            cMap2.Initilize.Phase2(phase2_1);

            Assert.IsTrue(cMap1.IsInitilzied);
            Assert.IsTrue(cMap2.IsInitilzied);
        }

    }
}
