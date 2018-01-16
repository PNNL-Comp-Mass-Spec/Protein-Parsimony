using System.Collections.Generic;
using SetCover.Objects;

namespace SetCover.Algorithms
{
	class NodeBuilder
	{

		Dictionary<string, Node> Proteins;
		Dictionary<string, Node> Peptides;

		public void RunAlgorithm(List<RowEntry> toParsimonize, out Dictionary<string, Node> prots, out Dictionary<string, Node> peps)
		{

			Proteins = new Dictionary<string, Node>(toParsimonize.Count);
			Peptides = new Dictionary<string, Node>(toParsimonize.Count);
			BuildNodes(toParsimonize);
			prots = Proteins;
			peps = Peptides;
		}
		/// <summary>
		/// Builds the nodes for a bipartite graph composed of proteins
		/// on one side peptides on the other
		/// </summary>
		/// <param name="toParsimonize">input table must have at peptide and protein column</param>
		private void BuildNodes(List<RowEntry> toParsimonize)
		{
			foreach (var row in toParsimonize)
			{

				var prot = new Protein(row.ProteinEntry);
				var pep = new Peptide(row.PeptideEntry);

				var proteinDefined = Proteins.ContainsKey(prot.NodeName);
				var peptideDefined = Peptides.ContainsKey(pep.NodeName);

				if (proteinDefined && peptideDefined)
				{
					if (!Proteins[prot.NodeName].Children.Contains(Peptides[pep.NodeName]))
					{
						Proteins[prot.NodeName].Children.Add(Peptides[pep.NodeName]);
					}
					if (!Peptides[pep.NodeName].Children.Contains(Proteins[prot.NodeName]))
					{
						Peptides[pep.NodeName].Children.Add(Proteins[prot.NodeName]);
					}
				}
				else if (proteinDefined)
				{
					Proteins[prot.NodeName].Children.Add(pep);
					pep.Children.Add(Proteins[prot.NodeName]);
					Peptides.Add(pep.NodeName, pep);

				}
				else if (peptideDefined)
				{
					Peptides[pep.NodeName].Children.Add(prot);
					prot.Children.Add(Peptides[pep.NodeName]);
					Proteins.Add(prot.NodeName, prot);
				}
				else
				{
					prot.Children.Add(pep);
					pep.Children.Add(prot);
					Proteins.Add(prot.NodeName, prot);
					Peptides.Add(pep.NodeName, pep);
				}
			}
		}

		/// <summary>
		/// Ensures that all proteins in the graph have had the untaken peptide supdates.
		/// </summary>
		public void UpdateUntakenPeptides()
		{
			foreach (var node in Proteins.Values)
			{
			    var p = (Protein)node;
			    p.UntakenPeptide = p.Children.Count;
			}
		}



	}
}
