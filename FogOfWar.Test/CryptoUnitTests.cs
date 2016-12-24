using System;
using System.Linq;
using FogOfWar.Prototype;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FogOfWar.Test
{
    [TestClass]
    public class CryptoUnitTests
    {
        public static Map Map1 { get; private set; }
        public static Map Map2 { get; private set; }
        public static Node N0 { get; private set; }
        public static Node N1 { get; private set; }
        //public static Node N2 { get; private set; }
        //public static Node N3 { get; private set; }
        //public static Node N4 { get; private set; }
        //public static Node N5 { get; private set; }
        //public static Node N6 { get; private set; }
        //public static Node N7 { get; private set; }
        //public static Node N8 { get; private set; }
        //public static Node N9 { get; private set; }

        [ClassInitialize]
        public static void ClassInitilize(TestContext context)
        {
            var map = new FogOfWar.Prototype.Map();
            N0 = map.CreateNode();
            N1 = map.CreateNode();
            //N2 = map.CreateNode();
            //N3 = map.CreateNode();
            //N4 = map.CreateNode();
            //N5 = map.CreateNode();
            //N6 = map.CreateNode();
            //N7 = map.CreateNode();
            //N8 = map.CreateNode();
            //N9 = map.CreateNode();

            // We dont need to Connect Nodes, this has no influence on the crypto part
            var p = CryptoHelper.GeneratePrime();
            Map1 = new Map(map, 5,p);
            Map2 = new Map(map, 5, p);

            var phase1_1 = Map1.Initilize.Phase0().ToArray();
            var phase1_2 = Map2.Initilize.Phase0().ToArray();

            var phase2_1 = Map1.Initilize.Phase1(phase1_2);
            var phase2_2 = Map2.Initilize.Phase1(phase1_1);

            Map1.Initilize.Phase2(phase2_2);
            Map2.Initilize.Phase2(phase2_1);

            Assert.IsTrue(Map1.IsInitilzied);
            Assert.IsTrue(Map2.IsInitilzied);

        }
        private static Node[] N(params Node[] data) => data;

        [TestMethod]
        public void FindTest1()
        {
            var s1_1 = Map1.Scan.PrepareForPrope(N(), N(N1));
            var s1_2 = Map2.Scan.PrepareForPrope(N(N1), N());

            var s2_1 = Map1.Scan.PreparePositions(s1_2);
            var s2_2 = Map2.Scan.PreparePositions(s1_1);

            var s3_1 = Map1.Scan.ExecuteProbe(s2_2).ToArray();
            var s3_2 = Map2.Scan.ExecuteProbe(s2_1).ToArray();

            CollectionAssert.Contains(s3_1, N1);
            Assert.AreEqual(0, s3_2.Length);

        }

    }
}
