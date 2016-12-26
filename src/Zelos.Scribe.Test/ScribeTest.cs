using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace Zelos.Scribe.Test
{
    [TestClass]
    public class ScribeTest
    {
        [TestMethod]
        public void SerelizeEmptyScrbe()
        {
            var s = new ScripeEmpty();
            s.FreezeAsync().Wait();
            var xml = s.Serelize();
            var s2 = ScripeEmpty.Deserilize<ScripeEmpty>(xml);
            Assert.IsTrue(s2.IsFrozen);
        }


        [TestMethod]
        public void SerelizePropertysPublic()
        {
            var s = new ScripePropertysOnly() { Secret = "Secret", ShouldBeSerelized = 1, ShouldNotBeSerelized = 2 };
            s.FreezeAsync().Wait();
            var xml = s.Serelize();
            var s2 = ScripeEmpty.Deserilize<ScripePropertysOnly>(xml);

            Assert.AreEqual(s.ShouldBeSerelized, s2.ShouldBeSerelized);
            Assert.AreEqual(default(string), s2.Secret);
            Assert.AreEqual(default(int), s2.ShouldNotBeSerelized);
            Assert.IsTrue(s2.IsFrozen);

            Assert.IsTrue(AbstractScripture.Equals(s, s2, true));
            Assert.IsFalse(AbstractScripture.Equals(s, s2, false));
        }
        [TestMethod]
        public void SerelizePropertysSecret()
        {
            var s = new ScripePropertysOnly() { Secret = "Secret", ShouldBeSerelized = 1, ShouldNotBeSerelized = 2 };
            s.FreezeAsync().Wait();
            var xml = s.Serelize(true);
            var s2 = ScripeEmpty.Deserilize<ScripePropertysOnly>(xml);

            Assert.AreEqual(s.ShouldBeSerelized, s2.ShouldBeSerelized);
            Assert.AreEqual(s.Secret, s2.Secret);
            Assert.AreEqual(default(int), s2.ShouldNotBeSerelized);
            Assert.IsTrue(s2.IsFrozen);

            Assert.IsTrue(AbstractScripture.Equals(s, s2, true));
            Assert.IsTrue(AbstractScripture.Equals(s, s2, false));
        }

        [TestMethod]
        public void SerelizeSingelSub()
        {
            var sub = new ScripePropertysOnly() { Secret = "Secret", ShouldBeSerelized = 1, ShouldNotBeSerelized = 2 };
            var parent = new ScrupeWithSingelSub() { Child = sub };
            parent.FreezeAsync().Wait();
            var xml = parent.Serelize();
            var parent2 = ScripeEmpty.Deserilize<ScrupeWithSingelSub>(xml);

            Assert.AreEqual(sub.ShouldBeSerelized, (parent2.Child as ScripePropertysOnly).ShouldBeSerelized);
            Assert.AreEqual(default(int), (parent2.Child as ScripePropertysOnly).ShouldNotBeSerelized);
            Assert.AreEqual(default(string), (parent2.Child as ScripePropertysOnly).Secret);
            Assert.IsTrue(parent2.IsFrozen);
            Assert.IsTrue(parent2.Child.IsFrozen);
        }


        [TestMethod]
        public void SerelizeMultipleSub()
        {
            var sub1 = new ScripePropertysOnly() { Secret = "Secret", ShouldBeSerelized = 1, ShouldNotBeSerelized = 2 };
            var sub2 = new ScripeEmpty();
            var parent = new ScrupeWithCollectionSub();
            parent.Childs.Add(sub1);
            parent.Childs.Add(sub2);
            parent.FreezeAsync().Wait();
            var xml = parent.Serelize();
            var parent2 = ScripeEmpty.Deserilize<ScrupeWithCollectionSub>(xml);

            Assert.IsTrue(parent2.Childs.Count == 2);
            Assert.IsTrue(parent2.Childs[0] is ScripePropertysOnly);
            Assert.IsTrue(parent2.Childs[1] is ScripeEmpty);
            Assert.AreEqual(sub1.ShouldBeSerelized, (parent2.Childs[0] as ScripePropertysOnly).ShouldBeSerelized);
            Assert.AreEqual(default(int), (parent2.Childs[0] as ScripePropertysOnly).ShouldNotBeSerelized);
            Assert.AreEqual(default(string), (parent2.Childs[0] as ScripePropertysOnly).Secret);
            Assert.IsTrue(parent2.IsFrozen);
            Assert.IsTrue(parent2.Childs.All(x => x.IsFrozen));
        }


    }


    public class ScripeEmpty : AbstractScripture
    {

    }
    public class ScripePropertysOnly : AbstractScripture
    {

        public int ShouldNotBeSerelized { get; set; }

        [ScriptureValue(ScriptureValueType.Public)]
        public BigInteger ShouldBeSerelized { get; set; }

        [ScriptureValue(ScriptureValueType.Secret)]
        public string Secret { get; set; }

    }

    public class ScrupeWithSingelSub : AbstractScripture
    {
        public AbstractScripture Child { get; set; }

    }

    public class ScrupeWithCollectionSub : AbstractScripture
    {
        public List<AbstractScripture> Childs { get; private set; } = new List<AbstractScripture>();

    }

}
