using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SetCover
{
    /// <summary>
    /// A grouped set of proteins with identical children
    /// </summary>
    class ProteinGroup : Group, IComparable
    {
        public int UntakenPeptide { get; set; }

        public ProteinGroup(NodeChildren<Node> groupedNodes) : base(groupedNodes)
        {
            UntakenPeptide = this.children.Count;

        }
        //Dont know in what case this would be used.
        public ProteinGroup(string nodeName):base(nodeName){
        }

        //Sorts onthe number of untaken nodes.  Used for the set coverage portion of
        //algorithm because when a protein(group) is removed from the set, proteins which
        //also have one or more of the same peptides can no longer count those towards being
        //picked next in the greed algorithm.
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

        public void UpdateUntakenPeptides()
        {
            UntakenPeptide = this.children.Count;
        }

    }
}
