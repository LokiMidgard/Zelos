using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Zelos.Scribe
{

    public abstract class AbstractScripture

    {
        [ScriptureValue(ScriptureValueType.Secret)]
        internal BigInteger Secret { get; set; }

        public bool IsFrozen { get; private set; }

        private byte[] hash;
        internal byte[] Hash
        {
            get
            {
                ThrowNotFrozen();
                return this.hash;
            }
            private set { this.hash = value; }
        }

        protected void ThrowNotFrozen([CallerMemberName] string caller = null)
        {
            if (!this.IsFrozen)
                throw new InvalidOperationException($"Object must be frozen before {caller} can be called.");
        }

        public IReadOnlyList<AbstractScripture> SubScripture { get; }
        public bool HasSecrets { get; private set; } = true;

        private List<AbstractScripture> subScriptures = new List<AbstractScripture>();
        public AbstractScripture()
        {
            this.SubScripture = this.subScriptures.AsReadOnly();
            this.Secret = Common.Crypto.Generate.Random(int.MaxValue);
        }

        public String Serelize(bool includePrivate = false)
        {
            ThrowNotFrozen();
            var s = new Serelizer();
            return s.Serelize(this, includePrivate);
        }

        public static T Deserilize<T>(string data) where T : AbstractScripture, new()
        {
            var s = new Serelizer();
            return s.Deserelize<T>(data);
        }

        public async Task FreezeAsync()
        {
            if (this.IsFrozen)
                return;

            // Initilize SubScriptures
            InitilizeSubScriptures();

            // Call Freeze on Subscriptures and this
            OnBeforeFreeze();
            await Task.WhenAll(this.SubScripture.Select(x => x.FreezeAsync()));
            OnFreeze();

            // Calculate the Hash
            await Task.Run(() =>
            {
                using (var hash = Common.Crypto.CalculateHash.Create())
                {
                    var propertys = this.GetType().GetRuntimeProperties().Where(x => x.GetCustomAttribute<ScriptureValueAttribute>() != null).OrderBy(x => x.Name);
                    foreach (var p in propertys)
                    {
                        var value = ToBytes(p.GetValue(this));
                        hash.AddData(value);
                    }
                    foreach (var s in this.SubScripture)
                        hash.AddData(s.Hash);

                    this.Hash = hash.GetResult();
                }
            });
            this.IsFrozen = true;
        }

        internal void AfterDeserelize(byte[] hash, bool hasSecrets)
        {
            InitilizeSubScriptures();
            // Do not Calculate the Hash. We may have not all Informations.
            this.Hash = hash;
            this.HasSecrets = hasSecrets;
            this.IsFrozen = true;
        }


        private void InitilizeSubScriptures()
        {
            var singelSubScriptures = this.GetType().GetRuntimeProperties().Where(x => typeof(AbstractScripture).GetTypeInfo().IsAssignableFrom(x.PropertyType.GetTypeInfo())).OrderBy(x => x.Name);
            var multiSubScriptures = this.GetType().GetRuntimeProperties()
                .Where(x => typeof(IEnumerable<AbstractScripture>).GetTypeInfo().IsAssignableFrom(x.PropertyType.GetTypeInfo()))
                .Where(x => x.Name != nameof(SubScripture))
                .OrderBy(x => x.Name);

            this.subScriptures.Clear(); // Should be empty, but if somewhere somehow there was an exception executeing FreezeAsync IsFrozen is not true and FreezeAsync cann executeded again.
            this.subScriptures.AddRange(singelSubScriptures.Select(x => (AbstractScripture)x.GetValue(this)));
            this.subScriptures.AddRange(multiSubScriptures.SelectMany(x => (IEnumerable<AbstractScripture>)x.GetValue(this)));
        }

        protected virtual byte[] ToBytes(object o)
        {
            switch (o)
            {
                case null:
                    return new byte[] { 0 };
                case BigInteger b:
                    return b.ToByteArray();
                case byte[] ba:
                    return ba;
                case string s:
                    return System.Text.Encoding.Unicode.GetBytes(s);
                case int i:
                    return BitConverter.GetBytes(i);
                case Guid g:
                    return g.ToByteArray();
                default:
                    throw new ArgumentException($"The Type {o.GetType()} can not be transformed to bytes");
            }
        }

        protected virtual void OnBeforeFreeze() { }
        protected virtual void OnFreeze() { }


        protected virtual bool AreEqual(AbstractScripture o1, AbstractScripture o2, bool ignoreUnknowSecrests)
        {
            var v1 = o1.GetValues(ignoreUnknowSecrests).ToArray();
            var v2 = o2.GetValues(ignoreUnknowSecrests).ToArray();
            if (v1.Length != v2.Length)
                return false;

            for (int i = 0; i < v1.Length; i++)
                if (!object.Equals(v1[i], v2[i]))
                    return false;

            return true;
        }

        public static bool Equals(AbstractScripture o1, AbstractScripture o2, bool ignoreUnknowSecrests)
        {
            if (object.ReferenceEquals(o1, o2))
                return true;

            if (o1 == null || o2 == null)
                return false;

            if (o1.GetType() != o2.GetType())
                return false;

            if (!ignoreUnknowSecrests && (o1.HasSecrets != o2.HasSecrets))
                return false;

            if (!ignoreUnknowSecrests && (o1.Secret != o2.Secret))
                return false;

            if (!o1.AreEqual(o1, o2, ignoreUnknowSecrests))
                return false;

            if (o1.SubScripture.Count != o2.SubScripture.Count)
                return false;

            for (int i = 0; i < o1.SubScripture.Count; i++)
                if (!Equals(o1.SubScripture[i], o2.SubScripture[i], ignoreUnknowSecrests))
                    return false;

            return true;
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            var other = obj as AbstractScripture;

            return Equals(this, other, false);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            var prime = 31;
            var result = 1;

            result = result * prime + this.Secret.GetHashCode();
            foreach (var s in this.SubScripture)
                result = result * prime + s?.GetHashCode() ?? 0;

            foreach (var s in this.GetValues(false))
                result = result * prime + s?.GetHashCode() ?? 0;

            return result;
        }

        private IEnumerable<object> GetValues(bool ignoreUnknowSecrests)
        {
            var propertysToSerelize = this.GetType().GetRuntimeProperties().Select(x => new { Property = x, ScriptureValue = x.GetCustomAttribute<ScriptureValueAttribute>()?.ValueType })
                            .Where(x => x.ScriptureValue.HasValue);

            if (ignoreUnknowSecrests)
                propertysToSerelize = propertysToSerelize.Where(x => x.ScriptureValue == ScriptureValueType.Public);
            var enumerable = propertysToSerelize.Select(x => x.Property).OrderBy(x => x.Name);
            return enumerable.Select(x => x.GetValue(this));

        }


    }

    [System.AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ScriptureValueAttribute : Attribute
    {
        public ScriptureValueAttribute(ScriptureValueType vType)
        {
            this.ValueType = vType;
        }

        public ScriptureValueType ValueType { get; }
    }

    public enum ScriptureValueType
    {
        Public,
        Secret
    }
}
