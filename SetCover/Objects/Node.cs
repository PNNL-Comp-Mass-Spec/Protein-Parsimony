using System;

namespace SetCover.Objects
{
	/// <summary>
	/// Main object for generating the bipartite graph
	/// </summary>
	public class Node : IComparable
	{

		private static int IDNum;

		public enum NodeTypeName
		{
			Peptide = 0,
			Protein = 1,
			PeptideGroup = 2,
			ProteinGroup = 3,
			Cluster = 4,
			Other = 5
		}

		public NodeTypeName NodeType { get; set; }
		public int Id { get; set; }
		public NodeChildren<Node> Children { get; set; }
		public string NodeName { get; set; }

		// Constructor
		public Node()
			: this(NodeTypeName.Other)
		{
		}

		// Constructor
		public Node(NodeTypeName nodeType)
		{
			NodeType = nodeType;
			if (NodeType == NodeTypeName.Other)
				Console.WriteLine("Found other");

			Id = System.Threading.Interlocked.Increment(ref IDNum);
		    NodeName = string.Empty;
		}

		// Constructor
		public Node(NodeTypeName nodeType, string nodeName)
			: this(nodeType)
		{
			NodeName = nodeName;
		    Children = new NodeChildren<Node>();

		}

		//Use number of children to sort.
		public int CompareTo(object obj)
		{
			if (obj == null) return 1;

		    if (obj is Node otherNode)
			{
				return Children.Count.CompareTo(otherNode.Children.Count);
			}

		    throw new ArgumentException("Object is not a Node!");

		}


	}


}
