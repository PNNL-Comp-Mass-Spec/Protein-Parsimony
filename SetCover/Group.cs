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
            children = groupedNodes[0].children;

            foreach (Node child in children)
            {
                foreach (Node grNode in groupedNodes)
                {
                    child.children.Remove(grNode);
                }
            }

            foreach (Node child in children)
            {
                child.children.Add(this);
            }

        }
    }
}
