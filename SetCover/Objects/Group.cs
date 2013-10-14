using System;

namespace SetCover
{
    /// <summary>
    /// Base class for groups which are composed of peptides or proteins
    /// </summary>
    class Group : Node
    {
        protected NodeChildren<Node> nodeGroup;
        private string[] nodeNames;
        public Group(string nodeName) : base(nodeName) { }
        public Group(NodeChildren<Node> groupedNodes):base()
        {
            //copies inputted nodes to be grouped into a list
            //copies the grouped nodes' children as the groups children.
            //In this case since they should be identical we just grab the first member
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

        /// <summary>
        /// Getter for grouped set of nodes.
        /// </summary>
        /// <returns></returns>
        public NodeChildren<Node> GetNodeGroup()
        {
            return nodeGroup;
        }
    }
}
