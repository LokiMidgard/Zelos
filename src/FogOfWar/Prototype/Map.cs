﻿using System;
using System.Collections.Generic;
using System.Text;

namespace FogOfWar.Prototype
{
    public class Map
    {
        public IReadOnlyCollection<Node> Nodes { get; }

        private readonly List<Node> nodes = new List<Node>();

        private Guid id = Guid.NewGuid();

        public Map()
        {
            this.Nodes = this.nodes.AsReadOnly();
        }

        public Node CreateNode()
        {
            var newNode = new Node(this.nodes.Count, this);
            this.nodes.Add(newNode);
            return newNode;
        }

        public void ConnectNodes(Node n1, Node n2)
        {
            n1.edges.Add(n2);
            n2.edges.Add(n1);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = obj as Map;
            return other.id.Equals(this.id);
        }

        public override int GetHashCode()
        {
            return this.id.GetHashCode();
        }

    }
}
