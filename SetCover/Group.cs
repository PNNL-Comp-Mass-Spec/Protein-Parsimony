using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SetCover
{
    class Group : Node
    {
        protected NodeChildren<Node> nodeGroup;
        string[] nodeNames;
        public Group(string nodeName) : base(nodeName) { }
        public Group(NodeChildren<Node> groupedNodes)
        {
            this.nodeGroup = groupedNodes;
            this.children = groupedNodes[0].children;
            this.nodeNames = new string[groupedNodes.Count];
            for (int i = 0; i < nodeNames.Length; i++)
            {
                this.nodeNames[i] = groupedNodes[i].nodeName;
            }
            this.nodeName = String.Join("-", nodeNames);

            foreach (Node child in children)
            {
                for(int i = 0; i < groupedNodes.Count; i++)
                {
                    child.children.Remove(groupedNodes[i]);
                }
            }
            foreach (Node child in children)
            {
                child.children.Add(this);
            }

        }
    }
}
