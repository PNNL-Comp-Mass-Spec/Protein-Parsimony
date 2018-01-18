using System.Collections.Generic;
using SetCover.Objects;

namespace SetCover.Algorithms
{
	class DFS
	{
	    readonly Dictionary<int, Node> AllNodes = new Dictionary<int, Node>();

		public List<Node> RunAlgorithm(List<Node> proteins)
		{
			foreach (var node in proteins)
			{
				var p = (ProteinGroup)node;
				p.UpdateUntakenPeptides();
			}

			var clusteredProteins = ClusterNodes(proteins);
			return clusteredProteins;

		}

		public bool CheckID(Node nextNode, HashSet<Node> searchedNodes)
		{
			return searchedNodes.Contains(nextNode);
		}

		/// <summary>
		/// Using a depth first search to cluster sets of nodes for set cover assignment
		/// </summary>
		/// <param name="input"></param>
		/// <param name="searchNodes"></param>
		public void Search(Node input, HashSet<Node> searchNodes)
		{
			foreach (var child in input.Children)
			{
				if (!CheckID(child, searchNodes))
				{
					searchNodes.Add(child);
					AllNodes.Remove(child.Id);
					Search(child, searchNodes);
				}
			}
		}

		public HashSet<Node> Search(Node input)
		{
		    var searchNodes = new HashSet<Node> {
		        input
		    };
		    AllNodes.Remove(input.Id);
			Search(input, searchNodes);
			return searchNodes;
		}

		public List<Node> ClusterNodes(List<Node> inNodes)
		{
			var clusters = new List<Node>();
			foreach (var mynode in inNodes)
			{
				AllNodes.Add(mynode.Id, mynode);
			}
			foreach (var mynode in inNodes)
			{
				if (AllNodes.ContainsKey(mynode.Id))
				{
				    var cs = new Cluster {
				        Children = new NodeChildren<Node>(Search(mynode))
				    };

				    //           cs.nodeName = string.Concat(cs.chi);
					clusters.Add(cs);
				}
			}
			return clusters;
		}





	}
}
