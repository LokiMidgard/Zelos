using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FogOfWar
{
    public interface IComunication
    {

        Task SendMessageAsync(byte[] data);

        event Action<byte[]> MessageRecived;
    }
}
