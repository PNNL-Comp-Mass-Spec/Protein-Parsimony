using System;

namespace SetCover.Objects
{
    /// <summary>
    /// A grouped set of proteins with identical children
    /// </summary>
    class ProteinGroup : Group
    {
        /// <summary>
        /// Number children peptides for this group
        /// </summary>
        public int UntakenPeptides { get; set; }

        /// <summary>
        /// Constructor that takes a set grouped nodes
        /// </summary>
        /// <param name="groupedNodes"></param>
        /// <param name="globalIDTracker"></param>
        public ProteinGroup(NodeChildren<Node> groupedNodes, GlobalIDContainer globalIDTracker)
            : base(NodeTypeName.ProteinGroup, groupedNodes, globalIDTracker)
        {
            UntakenPeptides = Children.Count;

        }

        /// <summary>
        /// Constructor that takes a node name
        /// </summary>
        /// <param name="nodeName"></param>
        public ProteinGroup(string nodeName)
            : base(NodeTypeName.Protein, nodeName)
        {
        }

        /// <summary>
        /// Sorts on the number of untaken nodes.  Used for the set coverage portion of
        /// algorithm because when a protein(group) is removed from the set, proteins which
        /// also have one or more of the same peptides can no longer count those towards being
        /// picked next by the greedy algorithm.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public new int CompareTo(object obj)
        {
            if (obj == null) return 1;

            if (obj is ProteinGroup otherNode)
            {
                var childComparison = Children.Count.CompareTo(otherNode.Children.Count);
                if (childComparison != 0)
                    return childComparison;

                var untakenPepComparison = UntakenPeptides.CompareTo(otherNode.UntakenPeptides);
                if (untakenPepComparison != 0)
                    return untakenPepComparison;

                return Id.CompareTo(otherNode.Id);
            }

            throw new ArgumentException("Object is not a Node!");
        }

        public void UpdateUntakenPeptides()
        {
            UntakenPeptides = Children.Count;
        }

        public override string ToString()
        {
            return base.ToString() + "; " + UntakenPeptides + " UntakenPeptides";
        }

    }
}
