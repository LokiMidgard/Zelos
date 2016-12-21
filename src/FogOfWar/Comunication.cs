using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FogOfWar
{
    internal class Comunication<TIn, TOut>
    {
        private readonly IComunication comunication;

        public Comunication(IComunication c)
        {
            this.comunication = c;
        }


        public virtual async Task<TOut> SendWithResultAsync(TIn input)
        {
            var inSerilizer = new DataContractSerializer(typeof(TIn));
            using (var memoryStream = new MemoryStream())
            {
                inSerilizer.WriteObject(memoryStream, input);
                var returnBytes = await this.comunication.SendWithResultAsync(memoryStream.ToArray());
                var outSerilizer = new DataContractSerializer(typeof(TOut));
                using (var resultMemoryStream = new MemoryStream(returnBytes))
                    return (TOut)outSerilizer.ReadObject(resultMemoryStream);
            }
        }
    }
}
