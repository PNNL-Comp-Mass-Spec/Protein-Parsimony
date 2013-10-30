using System;
using System.Collections.Generic;
using System.Linq;

namespace SetCover
{
    /// <summary>
    /// Base class for groups which are composed of peptides or proteins
    /// </summary>
    class Group : Node
    {
        protected NodeChildren<Node> nodeGroup;
        private List<string> mNodeNameList;

	    public String NodeNameFirst
	    {
		    get
		    {
				if (mNodeNameList == null || mNodeNameList.Count == 0)
					return string.Empty;
				else
					return mNodeNameList.First();
		    }

	    }

		public List<string> NodeNameList
		{
			get
			{
				if (mNodeNameList == null)
					return new List<string>();
				else
					return mNodeNameList;
			}
		}

		// Constructor
        public Group(string nodeName) : base(nodeName) { }

		// Constructor
        public Group(NodeChildren<Node> groupedNodes):base()
        {
            //copies inputted nodes to be grouped into a list
            //copies the grouped nodes' children as the groups children.
            //In this case since they should be identical we just grab the first member
            var tempNode = new NodeChildren<Node>(groupedNodes);
            this.nodeGroup = new NodeChildren<Node>(groupedNodes);
            this.children = new NodeChildren<Node>(groupedNodes[0].children);
            
			mNodeNameList = new List<string>();
            foreach (Node item in groupedNodes)
            {
				this.mNodeNameList.Add(item.nodeName);
            }
	        this.nodeName = String.Join("-", mNodeNameList);


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
