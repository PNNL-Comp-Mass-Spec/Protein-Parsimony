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
        public Group(NodeChildren<Node> groupedNodes):base()
        {
            NodeChildren<Node> tempNode = new NodeChildren<Node>(groupedNodes);
            this.nodeGroup = new NodeChildren<Node>(groupedNodes);
            this.children = new NodeChildren<Node>(groupedNodes[0].children);
            this.nodeNames = new string[groupedNodes.Count];
            for (int i = 0; i < nodeNames.Length; i++)
            {
                this.nodeNames[i] = groupedNodes[i].nodeName;
            }
            this.nodeName = String.Join("-", nodeNames);


            int toRemove = tempNode.Count;
            foreach (Node child in children)
            {
                for(int i = 0; i < toRemove; i++)
                    child.children.Remove(tempNode[i]);
                
            }
            foreach (Node child in children)
            {
                child.children.Add(this);
            }

        }

        public NodeChildren<Node> GetNodeGroup()
        {
            return nodeGroup;
        }
    }
}
