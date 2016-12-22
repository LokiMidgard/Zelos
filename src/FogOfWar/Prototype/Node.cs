using System;
using System.Collections.Generic;
using System.Text;

namespace FogOfWar.Prototype
{
    public class Node
    {
        public Map Map { get; }

        public ICollection<Node> Edgees { get; }

    }
}
