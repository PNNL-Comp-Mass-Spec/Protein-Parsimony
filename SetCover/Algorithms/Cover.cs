using System;
using System.Collections.Generic;
using PRISM;
using SetCover.Objects;

namespace SetCover.Algorithms
{
    /// <summary>
    /// Finds the best coverage using a greedy algorithm
    /// </summary>
    /// <remarks>
    /// For more information, see Step 4 of Figure 1 in manuscript
    /// "Proteomic Parsimony through Bipartite Graph Analysis Improves Accuracy and Transparency"
    /// by Bing Zhang, Matthew C. Chambers, and David L. Tabb
    /// PMID: 17676885
    /// https://www.ncbi.nlm.nih.gov/pmc/articles/PMC2810678
    /// </remarks>
    internal class Cover : EventNotifier
    {
        /// <summary>
        /// Finds the best coverage using a greedy algorithm
        /// </summary>
        /// <param name="inNode"></param>
        public List<Node> RunAlgorithm(List<Node> inNode)
        {
            var filteredList = new List<Node>();
            var lastProgressTime = DateTime.UtcNow;
            var currentNode = 0;

            foreach (var node in inNode)
            {
                currentNode++;
                if (DateTime.UtcNow.Subtract(lastProgressTime).TotalSeconds > 15)
                {
                    lastProgressTime = DateTime.UtcNow;
                    OnStatusEvent(string.Format("Computing coverage by node, {0} / {1}", currentNode, inNode.Count));
                }

                var cs = (Cluster)node;
                filteredList.AddRange(GetCover(cs));
            }
            return filteredList;
        }

        private IEnumerable<Node> GetCover(Node cs)
        {
            var proteinSet = new List<Node>();
            var proteins = new List<Node>();

            foreach (var node in cs.Children)
            {
                var t = node.GetType();
                if (t == typeof(ProteinGroup) ||
                    t == typeof(Protein))
                {
                    proteins.Add(node);
                }
                /*
                else
                {
                    peptides.Add(node);
                }
                */
            }

            var continueCheck = true;
            var totalProteins = proteins.Count;
            var startIndex = totalProteins - 1;

            var lastProgressTime = DateTime.UtcNow;

            while (continueCheck)
            {
                // Find the peptide with the highest number of children, and 1 or more untaken peptides
                var currentIndex = Math.Min(proteins.Count - 1, startIndex);
                while (currentIndex >= 0)
                {
                    // Sort the proteins by number of children, then by number of untaken peptides
                    proteins.Sort();

                    if (currentIndex % 100 == 0 && DateTime.UtcNow.Subtract(lastProgressTime).TotalSeconds > 15)
                    {
                        lastProgressTime = DateTime.UtcNow;
                        OnDebugEvent(string.Format("  ... computing coverage, {0} / {1}",
                                                   totalProteins - currentIndex, totalProteins));
                    }

                    if (((ProteinGroup)proteins[currentIndex]).UntakenPeptides <= 0)
                    {
                        // Work backward through the sorted list of proteins
                        currentIndex--;
                        startIndex--;
                        continue;
                    }

                    // Protein found; create a new Protein Group
                    var temp = proteins[currentIndex];
                    proteinSet.Add(temp);

                    // Remove the protein from the candidate proteins list
                    proteins.Remove(temp);

                    // Decrement UntakenPeptides for all proteins associated with the peptides in this new protein group
                    AdjustUntakenPeptides(temp);

                    break;
                }

                if (currentIndex < 0)
                    continueCheck = false;
            }

            return proteinSet;
        }

        private void AdjustUntakenPeptides(Node temp)
        {
            foreach (var pepChild in temp.Children)
            {
                foreach (var protChild in pepChild.Children)
                {
                    if (protChild is ProteinGroup p)
                    {
                        // Yes, this could become a negative number; that's OK
                        p.UntakenPeptides--;
                    }
                    else
                    {
                        Console.WriteLine("Possible programming bug: peptide child is not a Protein or ProteinGroup; it is a " + protChild.GetType());
                    }
                }
            }
        }
    }
}
