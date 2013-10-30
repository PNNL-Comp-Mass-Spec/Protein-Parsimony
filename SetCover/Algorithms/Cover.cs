using System;
using System.Collections.Generic;


namespace SetCover
{
	class Cover
	{
		public void RunAlgorithm(ref List<Node> inNode, ref List<Node> outNode)
		{
			RunAlgorithm(ref inNode);
		}

		/// <summary>
		/// Finds the best coverage using a greedy algorithm
		/// </summary>
		/// <param name="inNode"></param>
		public void RunAlgorithm(ref List<Node> inNode)
		{
			//log.Info("Running Greedy Set Cover");

			inNode = BulkFinder(inNode);

		}

		public List<Node> BulkFinder(List<Node> inCluster)
		{
			List<Node> FilteredList = new List<Node>();
			foreach (Cluster cs in inCluster)
			{
				FilteredList.AddRange(GetCover(cs));
			}
			return FilteredList;
		}

		private IEnumerable<Node> GetCover(Cluster cs)
		{
			List<Node> ProteinSet = new List<Node>();
			List<Node> proteins = new List<Node>();
			List<Node> peptides = new List<Node>();
			foreach (Node node in cs.children)
			{
				Type t = node.GetType();
				if (t == typeof(ProteinGroup) ||
					t == typeof(Protein))
				{
					proteins.Add(node);
				}
				else
				{
					peptides.Add(node);
				}
			}
			//This loop is going to have to change.
			proteins.Sort();
			while (proteins.Count != 0 && ((ProteinGroup)proteins[proteins.Count - 1]).UntakenPeptide != 0)
			{
				Node temp = proteins[proteins.Count - 1];
				ProteinSet.Add(temp);
				proteins.Remove(temp);
				AdjustUntakenPeptides(temp);
				proteins.Sort();
			}

			return ProteinSet;


		}

		private void AdjustUntakenPeptides(Node temp)
		{
			foreach (Node child in temp.children)
			{
				foreach (ProteinGroup pchild in child.children)
				{
					pchild.UntakenPeptide--;
				}
			}
		}




	}
}
