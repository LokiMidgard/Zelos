using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FogOfWar
{
    class GuidedComunication<TIn, TOut> : Comunication<GuidedComunication<TIn, TOut>.Envelop<TIn>, GuidedComunication<TIn, TOut>.Envelop<TOut>>
    {
        public Guid Key { get; }

        public GuidedComunication(IComunication c, Guid key) : base(c)
        {
            this.Key = key;
            c.MessageRecived += this.ConnectionMassageRecived;
        }

        private void ConnectionMassageRecived(byte[] data)
        {

        }

        public async Task<TOut> SendWithResultAsync(TIn input)
        {
            var inEnvelop = new Envelop<TIn>() { Content = input, Key = this.Key };
            var returnedEnvelop = await base.SendWithResultAsync(inEnvelop);
            if ()
        }

        [DataContract]
        public class Envelop<TEnvelop>
        {
            [DataMember]
            public TEnvelop Content { get; set; }

            [DataMember]
            public Guid Key { get; set; }

        }
    }
}
