using System.Collections.Generic;
using System.Linq;

namespace SetCover.Objects
{
    /// <summary>
    /// Base class for groups which are composed of peptides or proteins
    /// </summary>
    class Group : Node
    {
        public const char LIST_SEP_CHAR = '\t';

        protected NodeChildren<Node> nodeGroup;
        private readonly List<string> mNodeNameList;

        public string NodeNameFirst
        {
            get
            {
                if (mNodeNameList == null || mNodeNameList.Count == 0)
                    return string.Empty;

                return mNodeNameList.First();
            }

        }

        public List<string> NodeNameList
        {
            get
            {
                if (mNodeNameList == null)
                    return new List<string>();

                return mNodeNameList;
            }
        }

        // Constructor
        public Group(NodeTypeName nodeType, string nodeName) : base(nodeType, nodeName) { }

        // Constructor
        public Group(NodeTypeName nodeType, NodeChildren<Node> groupedNodes, GlobalIDContainer globalIDTracker)
            : base(nodeType)
        {
            //copies inputted nodes to be grouped into a list
            //copies the grouped nodes' children as the groups children.
            //In this case since they should be identical we just grab the first member
            var tempNode = new NodeChildren<Node>(groupedNodes);
            nodeGroup = new NodeChildren<Node>(groupedNodes);
            Children = new NodeChildren<Node>(groupedNodes[0].Children);

            mNodeNameList = new List<string>();
            var nodeNameGlobalIDs = new List<int>();

            foreach (var item in groupedNodes)
            {
                mNodeNameList.Add(item.NodeName);
                var globalID = globalIDTracker.GetGlobalID(item.NodeName);
                nodeNameGlobalIDs.Add(globalID);
            }
            NodeName = string.Join(LIST_SEP_CHAR.ToString(), nodeNameGlobalIDs);


            var toRemove = tempNode.Count;
            foreach (var child in Children)
            {
                for(var i = 0; i < toRemove; i++)
                    child.Children.Remove(tempNode[i]);

            }

            foreach (var child in Children)
            {
                child.Children.Add(this);
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
