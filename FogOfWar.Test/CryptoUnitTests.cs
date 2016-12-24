using System;
using System.Linq;
using FogOfWar.Prototype;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace FogOfWar.Test
{
    [TestClass]
    public class CryptoUnitTests
    {
        public static Map Map1 { get; private set; }
        public static Map Map2 { get; private set; }
        public static Node N0 { get; private set; }
        public static Node N1 { get; private set; }
        public static Node N2 { get; private set; }
        public static Node N3 { get; private set; }
        public static Node N4 { get; private set; }
        public static Node N5 { get; private set; }
        public static Node N6 { get; private set; }
        public static Node N7 { get; private set; }
        public static Node N8 { get; private set; }
        public static Node N9 { get; private set; }

        [ClassInitialize]
        public static void ClassInitilize(TestContext context)
        {
            var map = new FogOfWar.Prototype.Map();
            N0 = map.CreateNode();
            N1 = map.CreateNode();
            N2 = map.CreateNode();
            N3 = map.CreateNode();
            N4 = map.CreateNode();
            N5 = map.CreateNode();
            N6 = map.CreateNode();
            N7 = map.CreateNode();
            N8 = map.CreateNode();
            N9 = map.CreateNode();

            // We dont need to Connect Nodes, this has no influence on the crypto part
            var p = CryptoHelper.GeneratePrime();
            Map1 = new Map(map, 10, p);
            Map2 = new Map(map, 10, p);

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

        [DataTestMethod]
        [DataRow(3, 10, 3, 2)]
        [DataRow(0, 1023, 9, 8)]
        [DataRow(0, 1023, 1023, 1023)]
        [DataRow(1023, 1023, 1023, 1023)]
        [DataRow(0, 0, 1023, 1024)]
        [DataRow(1023, 1023, 0, 0)]
        [DataRow(1023, 1023, 7, 5)]
        public void FindTest2(int pos1, int pos2, int s1, int s2)
        {
            var selection = new[] { pos1, pos2, s1, s2 };
            var data = new[] { N0, N1, N2, N3, N4, N5, N6, N7, N8, N9 };
            var position1 = new List<Prototype.Node>();
            var position2 = new List<Prototype.Node>();
            var search1 = new List<Prototype.Node>();
            var search2 = new List<Prototype.Node>();

            var lists = new[] { position1, position2, search1, search2 };

            for (int i = 0; i < data.Length; i++)
                for (int j = 0; j < lists.Length; j++)
                {
                    if (((1 << i) & selection[j]) == (1 << i))
                        lists[j].Add(data[i]);
                }


            var s1_1 = Map1.Scan.PrepareForPrope(position1, search1);
            var s1_2 = Map2.Scan.PrepareForPrope(position2, search2);

            var s2_1 = Map1.Scan.PreparePositions(s1_2);
            var s2_2 = Map2.Scan.PreparePositions(s1_1);

            var s3_1 = Map1.Scan.ExecuteProbe(s2_2).ToArray();
            var s3_2 = Map2.Scan.ExecuteProbe(s2_1).ToArray();


            for (int i = 0; i < data.Length; i++)
            {
                if (position1.Contains(data[i]) && search2.Contains(data[i]))
                    CollectionAssert.Contains(s3_2, data[i]);
                else
                    CollectionAssert.DoesNotContain(s3_2, data[i]);

                if (position2.Contains(data[i]) && search1.Contains(data[i]))
                    CollectionAssert.Contains(s3_1, data[i]);
                else
                    CollectionAssert.DoesNotContain(s3_1, data[i]);
            }
        }

    }
}
