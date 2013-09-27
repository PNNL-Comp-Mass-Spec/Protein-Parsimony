using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SetCover
{
    class Group : Node
    {
        protected List<Node> nodeGroup;

        public Group(string nodeName) : base(nodeName) { }
        public Group(List<Node> groupedNodes)
        {
            this.nodeGroup = groupedNodes;
            foreach (Node n in groupedNodes)
            {
                foreach (Node child in n.children)
                {
                    this.children.Add(child);
                }
            }
        }
    }
}
