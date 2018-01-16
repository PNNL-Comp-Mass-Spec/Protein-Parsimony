using System;

namespace SetCover.Objects
{
	/// <summary>
	/// A grouped set of proteins with identical children
	/// </summary>
	class ProteinGroup : Group
	{
		public int UntakenPeptide { get; set; }

		public ProteinGroup(NodeChildren<Node> groupedNodes, GlobalIDContainer globalIDTracker)
			: base(NodeTypeName.ProteinGroup, groupedNodes, globalIDTracker)
		{
			UntakenPeptide = Children.Count;

		}

		//Don't know in what case this would be used.
		public ProteinGroup(string nodeName)
			: base(NodeTypeName.Protein, nodeName)
		{
		}

		//Sorts onthe number of untaken nodes.  Used for the set coverage portion of
		//algorithm because when a protein(group) is removed from the set, proteins which
		//also have one or more of the same peptides can no longer count those towards being
		//picked next in the greed algorithm.
		public int compareTo(object obj)
		{
			if (obj == null) return 1;

		    if (obj is ProteinGroup otherNode)
			{
				return UntakenPeptide.CompareTo(otherNode.UntakenPeptide);
			}

		    throw new ArgumentException("Object is not a Node!");
		}

		public void UpdateUntakenPeptides()
		{
			UntakenPeptide = Children.Count;
		}

	}
}
