using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zelos.Common.Crypto;
using FogMap = Zelos.Mapping.FogOfWar.FogMap<object, object>;
using Node = Zelos.Mapping.Node<object, object>;

namespace Zelos.Mapping.Test
{
    [TestClass]
    public class CryptoUnitTests
    {
        public static FogMap Map1 { get; private set; }
        public static FogMap Map2 { get; private set; }
        public static Node N0;
        public static Node N1 ;
        public static Node N2 ;
        public static Node N3 ;
        public static Node N4 ;
        public static Node N5 ;
        public static Node N6 ;
        public static Node N7 ;
        public static Node N8 ;
        public static Node N9 ;

        [ClassInitialize]
        public static void ClassInitilize(TestContext context)
        {
            var map = Map.Create()
                .AddNode(out N0)
                .AddNode(out N1)
                .AddNode(out N2)
                .AddNode(out N3)
                .AddNode(out N4)
                .AddNode(out N5)
                .AddNode(out N6)
                .AddNode(out N7)
                .AddNode(out N8)
                .AddNode(out N9)
                .GetResult();


            // We dont need to Connect Nodes, this has no influence on the crypto part
            var p = Generate.Prime();
            Map1 = FogMap.Create(map, 10, p);
            Map2 = FogMap.Create(map, 10, p);


            var s1 = new Scribe.ScruptureCollection();
            var s2 = new Scribe.ScruptureCollection();


            var t1_1 = Map1.Initilize.Phase0Async();
            var t1_2 = Map2.Initilize.Phase0Async();
            Task.WaitAll(t1_1, t1_2);
            var phase1_1 = TestSerelisation(t1_1.Result, s1, s2);
            var phase1_2 = TestSerelisation(t1_2.Result, s1, s2);

            var t2_1 = Map1.Initilize.Phase1Async(phase1_2);
            var t2_2 = Map2.Initilize.Phase1Async(phase1_1);
            Task.WaitAll(t2_1, t2_2);
            var phase2_1 = TestSerelisation(t2_1.Result, s1, s2);
            var phase2_2 = TestSerelisation(t2_2.Result, s1, s2);

            Task.WaitAll(Map1.Initilize.Phase2Async(phase2_2),
                Map2.Initilize.Phase2Async(phase2_1));


            s2.AddAsync(s1.FinishAsync().Result).Wait();
            s1.AddAsync(s2.FinishAsync().Result).Wait();

            Assert.IsTrue(s1.IsFinised);
            Assert.IsTrue(s2.IsFinised);

            Assert.IsTrue(s1.IsOtherValid.HasValue);
            Assert.IsTrue(s2.IsOtherValid.HasValue);

            Assert.IsTrue(s1.IsOtherValid.Value);
            Assert.IsTrue(s2.IsOtherValid.Value);

            Assert.IsTrue(Map1.IsInitilzied);
            Assert.IsTrue(Map2.IsInitilzied);

        }
        private static Node[] N(params Node[] data) => data;

        [TestMethod]
        public void FindTest1()
        {

            var s1 = new Scribe.ScruptureCollection();
            var s2 = new Scribe.ScruptureCollection();


            var t1_0 = Map1.Scan.PrepareForPropeAsync(N(), N(N1));
            var t2_0 = Map2.Scan.PrepareForPropeAsync(N(N1), N());
            Task.WaitAll(t1_0, t2_0);
            var s1_1 = TestSerelisation(t1_0.Result, s1, s2);
            var s1_2 = TestSerelisation(t2_0.Result, s1, s2);

            var t1_1 = Map1.Scan.PreparePositionsAsync(s1_2);
            var t2_1 = Map2.Scan.PreparePositionsAsync(s1_1);
            Task.WaitAll(t2_1, t1_1);
            var s2_1 = TestSerelisation(t1_1.Result, s1, s2);
            var s2_2 = TestSerelisation(t2_1.Result, s1, s2);

            var t1 = Map1.Scan.ExecuteProbeAsync(s2_2);
            var t2 = Map2.Scan.ExecuteProbeAsync(s2_1);
            Task.WaitAll(t1, t2);
            var s3_1 = t1.Result.ToArray();
            var s3_2 = t2.Result.ToArray();


            s2.AddAsync(s1.FinishAsync().Result).Wait();
            s1.AddAsync(s2.FinishAsync().Result).Wait();

            Assert.IsTrue(s1.IsFinised);
            Assert.IsTrue(s2.IsFinised);

            Assert.IsTrue(s1.IsOtherValid.HasValue);
            Assert.IsTrue(s2.IsOtherValid.HasValue);

            Assert.IsTrue(s1.IsOtherValid.Value);
            Assert.IsTrue(s2.IsOtherValid.Value);


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
        public void FindTest2(int pos1, int pos2, int s1_, int s2_)
        {
            var selection = new[] { pos1, pos2, s1_, s2_ };
            var data = new[] { N0, N1, N2, N3, N4, N5, N6, N7, N8, N9 };
            var position1 = new List<Node>();
            var position2 = new List<Node>();
            var search1 = new List<Node>();
            var search2 = new List<Node>();

            var lists = new[] { position1, position2, search1, search2 };

            for (int i = 0; i < data.Length; i++)
                for (int j = 0; j < lists.Length; j++)
                {
                    if (((1 << i) & selection[j]) == (1 << i))
                        lists[j].Add(data[i]);
                }

            var s1 = new Scribe.ScruptureCollection();
            var s2 = new Scribe.ScruptureCollection();


            var t1_0 = Map1.Scan.PrepareForPropeAsync(position1, search1);
            var t2_0 = Map2.Scan.PrepareForPropeAsync(position2, search2);
            Task.WaitAll(t1_0, t2_0);
            var s1_1 = TestSerelisation(t1_0.Result, s1, s2);
            var s1_2 = TestSerelisation(t2_0.Result, s1, s2);

            var t1_1 = Map1.Scan.PreparePositionsAsync(s1_2);
            var t2_1 = Map2.Scan.PreparePositionsAsync(s1_1);
            Task.WaitAll(t2_1, t1_1);
            var s2_1 = TestSerelisation(t1_1.Result, s1, s2);
            var s2_2 = TestSerelisation(t2_1.Result, s1, s2);

            var t1 = Map1.Scan.ExecuteProbeAsync(s2_2);
            var t2 = Map2.Scan.ExecuteProbeAsync(s2_1);
            Task.WaitAll(t1, t2);
            var s3_1 = t1.Result.ToArray();
            var s3_2 = t2.Result.ToArray();

            s2.AddAsync(s1.FinishAsync().Result).Wait();
            s1.AddAsync(s2.FinishAsync().Result).Wait();

            Assert.IsTrue(s1.IsFinised);
            Assert.IsTrue(s2.IsFinised);

            Assert.IsTrue(s1.IsOtherValid.HasValue);
            Assert.IsTrue(s2.IsOtherValid.HasValue);

            Assert.IsTrue(s1.IsOtherValid.Value);
            Assert.IsTrue(s2.IsOtherValid.Value);


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

        private static T TestSerelisation<T>(T result, Scribe.ScruptureCollection c1, Scribe.ScruptureCollection c2) where T : Scribe.AbstractScripture, new()
        {
            var data = c1.AddAsync(result).Result;
            return (T)c2.AddAsync(data).Result;

        }
    }
}
