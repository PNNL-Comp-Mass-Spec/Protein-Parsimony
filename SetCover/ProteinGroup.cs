using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SetCover
{
    class ProteinGroup : Group, IComparable
    {
        public int UntakenPeptide { get; set; }

        public ProteinGroup(NodeChildren<Node> groupedNodes) : base(groupedNodes)
        {
            UntakenPeptide = this.ChildCount;

        }

        public ProteinGroup(string nodeName):base(nodeName){
        }

        public int compareTo(object obj)
        {
            if (obj == null) return 1;

            ProteinGroup otherNode = obj as ProteinGroup;
            if (otherNode != null)
            {
                return this.UntakenPeptide.CompareTo(otherNode.UntakenPeptide);
            }
            else
            {
                throw new ArgumentException("Object is not a Node!");
            }
        }

    }
}
