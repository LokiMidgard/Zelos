using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Zelos.Scribe
{
    public class ScruptureCollection
    {
        public IReadOnlyList<AbstractScripture> OwnScriptures { get; }
        public IReadOnlyList<AbstractScripture> OtherScriptures { get; }

        public bool IsFinised => this.IsLastOtherAdded && this.IsLastOwnAdded;
        public bool IsLastOwnAdded { get; private set; }
        public bool IsLastOtherAdded { get; private set; }

        public bool? IsOtherValid { get; private set; }

        private BigInteger Secrete { get; }

        private readonly List<AbstractScripture> ownScriptures = new List<AbstractScripture>();
        private readonly List<AbstractScripture> otherScriptures = new List<AbstractScripture>();

        public ScruptureCollection()
        {
            this.OwnScriptures = this.ownScriptures.AsReadOnly();
            this.OtherScriptures = this.otherScriptures.AsReadOnly();
            this.Secrete = Zelos.Common.Crypto.Generate.Random(int.MaxValue);
        }

        public async Task<ScriptureTransmitt> AddAsync(AbstractScripture newScripture)
        {
            if (this.IsLastOwnAdded)
                throw new InvalidOperationException("Own queue already closed.");
            if (newScripture is FinishScripture)
                this.IsLastOwnAdded = true;

            await newScripture.FreezeAsync(this.Secrete);
            this.ownScriptures.Add(newScripture);
            return new ScriptureTransmitt(newScripture.Serelize(newScripture is FinishScripture));
        }
        public async Task<AbstractScripture> AddAsync(ScriptureTransmitt transmitt)
        {
            if (this.IsLastOtherAdded)
                throw new InvalidOperationException("Other queue already closed.");

            var newScripture = AbstractScripture.Deserilize<AbstractScripture>(transmitt.data);

            if (newScripture is FinishScripture)
                this.IsLastOtherAdded = true;


            this.otherScriptures.Add(newScripture);

            if (newScripture is FinishScripture)
                await Task.Run(() => this.IsOtherValid = CheckOtherQueue());
            return newScripture;
        }

        private bool CheckOtherQueue()
        {
            var finishElement = this.OtherScriptures.Last() as FinishScripture;
            if (finishElement == null)
                throw new InvalidOperationException("Last Scripture was not end");

            if (finishElement.Scriptures.Length + 1 != this.OtherScriptures.Count)
                return false;

            for (int i = 0; i < finishElement.Scriptures.Length; i++)
            {
                var originalScripture = this.OtherScriptures[i];
                var withSecrete = finishElement.Scriptures[i];

                if (!AbstractScripture.Equals(originalScripture, withSecrete, true, false))
                    return false;

                if (!withSecrete.CheckHash(finishElement.Secrete))
                    return false;

            }

            return true;

        }

        public async Task<ScriptureTransmitt> FinishAsync()
        {
            var fs = new FinishScripture() { Secrete = this.Secrete, Scriptures = this.ownScriptures.ToArray() };
            return await AddAsync(fs);

        }

        private sealed class FinishScripture : AbstractScripture
        {
            [ScriptureValue(ScriptureValueType.Secret)]
            internal BigInteger Secrete { get; set; }

            internal AbstractScripture[] Scriptures { get; set; }
        }

        [DataContract]
        public class ScriptureTransmitt
        {
            [DataMember]
            internal readonly string data;

            public ScriptureTransmitt(string v)
            {
                this.data = v;
            }
        }
    }
}
