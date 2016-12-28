using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Zelos.Common.Crypto;
using Zelos.Mapping.FogOfWar;

namespace Zelos.Mapping.Test
{
    [TestClass]
    public class SimpleCryptoUnitTests
    {
        [TestMethod]
        public void TestInitialisation()
        {
            var map = Map.Create()
                .AddNode(out var n1)
                .AddNode(out var n2)
                .GetResult();


            var p = Generate.Prime();
            var cMap1 = FogMap.Create(map, 1, p);
            var cMap2 = FogMap.Create(map, 1, p);

            var t1_1 = cMap1.Initilize.Phase0Async();
            var t1_2 = cMap2.Initilize.Phase0Async();
            Task.WaitAll(t1_1, t1_2);
            var phase1_1 = t1_1.Result;
            var phase1_2 = t1_2.Result;

            var t2_1 = cMap1.Initilize.Phase1Async(phase1_2);
            var t2_2 = cMap2.Initilize.Phase1Async(phase1_1);
            Task.WaitAll(t2_1, t2_2);
            var phase2_1 = t2_1.Result;
            var phase2_2 = t2_2.Result;

            Task.WaitAll(
            cMap1.Initilize.Phase2Async(phase2_2),
            cMap2.Initilize.Phase2Async(phase2_1));

            Assert.IsTrue(cMap1.IsInitilzied);
            Assert.IsTrue(cMap2.IsInitilzied);
        }

    }
}
