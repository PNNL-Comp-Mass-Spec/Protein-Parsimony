using System;

namespace SetCover
{
	/// <summary>
	/// Main object for generating the bipartite graph
	/// </summary>
	public class Node : IComparable
	{

		private static int IDNum = 0;

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
		public NodeChildren<Node> children { get; set; }
		public String nodeName { get; set; }

		// Constructor
		public Node()
			: this(NodeTypeName.Other)
		{
		}

		// Constructor
		public Node(NodeTypeName nodeType)
		{
			this.NodeType = nodeType;
			if (this.NodeType == NodeTypeName.Other)
				Console.WriteLine("Found other");

			this.Id = System.Threading.Interlocked.Increment(ref IDNum);
			this.nodeName = string.Empty;
		}

		// Constructor
		public Node(NodeTypeName nodeType, String nodeName)
			: this(nodeType)
		{
			this.nodeName = nodeName;
			this.children = new NodeChildren<Node>();

		}

		//Use number of children to sort.
		public int CompareTo(object obj)
		{
			if (obj == null) return 1;

			Node otherNode = obj as Node;
			if (otherNode != null)
			{
				return this.children.Count.CompareTo(otherNode.children.Count);
			}
			else
			{
				throw new ArgumentException("Object is not a Node!");
			}


		}



	}





}
