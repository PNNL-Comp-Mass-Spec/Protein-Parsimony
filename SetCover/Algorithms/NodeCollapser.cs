using System;
using System.Collections.Generic;
using System.Linq;
using SetCover.Objects;

namespace SetCover.Algorithms
{
    /// <summary>
    /// Set of functions used for collapsing proteins with identical peptides into a single protein group
    /// </summary>
    /// <remarks>
    /// For more information, see Step 2 of Figure 1 in manuscript
    /// "Proteomic Parsimony through Bipartite Graph Analysis Improves Accuracy and Transparency"
    /// by Bing Zhang, Matthew C. Chambers, and David L. Tabb
    /// PMID: 17676885
    /// https://www.ncbi.nlm.nih.gov/pmc/articles/PMC2810678
    /// </remarks>
    internal class NodeCollapser
    {
        public void RunAlgorithm(Dictionary<string, Node> protein, Dictionary<string, Node> pep, GlobalIDContainer globalIDTracker)
        {
            CollapseNodes(protein, pep, globalIDTracker);
        }

        /// <summary>
        /// Collapses redundant protein and peptides into single groups
        /// </summary>
        /// <param name="proteins"></param>
        /// <param name="peptides"></param>
        /// <param name="globalIDTracker"></param>
        public void CollapseNodes(
            Dictionary<string, Node> proteins,
            Dictionary<string, Node> peptides,
            GlobalIDContainer globalIDTracker)
        {
            var proteinNames = proteins.Keys.ToList();
            GroupProteinsOrPeptides(proteins, proteinNames, typeof(Protein), globalIDTracker);

            var peptideNames = peptides.Keys.ToList();
            GroupProteinsOrPeptides(peptides, peptideNames, typeof(Peptide), globalIDTracker);
        }

        void GroupProteinsOrPeptides(
            IDictionary<string, Node> entities,
            IReadOnlyList<string> entityNames,
            Type entityType,
            GlobalIDContainer globalIDTracker)
        {
            var count = 0;
            while (count != entityNames.Count)
            {
                // Is the key there?
                if (entities.ContainsKey(entityNames[count]))
                {
                    // Get the protein or peptide
                    var entity = entities[entityNames[count]];

                    // Only proceed if the correct type
                    if (entity.GetType() == entityType)
                    {
                        var dups = new NodeChildren<Node>();

                        // Look for duplicates and add to a duplicate list
                        dups.AddRange(FindDuplicates(entity));

                        if (dups.Count > 1)
                        {
                            // Create a protein or peptide group from the duplicates
                            Group newGroup;

                            if (entityType == typeof(Protein))
                                newGroup = new ProteinGroup(dups, globalIDTracker);
                            else if (entityType == typeof(Peptide))
                                newGroup = new PeptideGroup(dups, globalIDTracker);
                            else
                                throw new Exception("Invalid type: must be Protein or Peptide");

                            foreach (var dupItem in dups)
                            {
                                // Remove entities from the library, add the new group
                                entities.Remove(dupItem.NodeName);
                            }

                            entities.Add(newGroup.NodeName, newGroup);
                        }
                    }
                }
                count++;
            }
        }

        private NodeChildren<Node> FindDuplicates(Node node)
        {
            node.Children.Sort();
            var candidates = new NodeChildren<Node>();
            candidates.AddRange(node.Children[0].Children);
            candidates.Remove(node);

            var count = 0;

            // Pulls out candidates with different counts of children than the master
            while (candidates.Count != count)
            {
                if (node.Children.Count != candidates[count].Children.Count)
                {
                    candidates.RemoveAt(count);
                }
                else
                {
                    count++;
                }
            }

            // May want to change the List to a Hashset to be faster.
            // Finds identical sets.
            foreach (var childNode in node.Children)
            {
                count = 0;
                while (candidates.Count != count)
                {
                    if (!childNode.Children.Contains(candidates[count]))
                    {
                        candidates.RemoveAt(count);
                    }
                    else
                    {
                        count++;
                    }
                }
            }
            candidates.Add(node);
            return candidates;
        }
    }
}
