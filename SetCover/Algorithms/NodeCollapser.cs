using System;
using System.Collections.Generic;
using System.Linq;
using SetCover.Objects;

namespace SetCover.Algorithms
{
    /// <summary>
    /// Set of functions used for collapsing proteins with identical peptides into a single protein group
    /// </summary>
    class NodeCollapser
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


            /*
             * Deprecated
            else

            {

                // Get a list of the keys
                var listproteins = proteins.Keys.ToList();

                GroupProteins(proteins, listproteins, globalIDTracker);

                // Same thing as above, but with peptides

                var listpeptides = peptides.Keys.ToList();

                GroupPeptides(peptides, listpeptides, globalIDTracker);
            }
             */
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
                if (string.Equals(entityNames[count], "VIME_MOUSE"))
                {
                    Console.WriteLine("Check this");
                }

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

        /*
         * Deprecated
        private void GroupProteins(IDictionary<string, Node> proteins, IReadOnlyList<string> listprotein, GlobalIDContainer globalIDTracker)
        {
            var count = 0;
            while (count != listprotein.Count)
            {
                // Is the key there?
                if (proteins.ContainsKey(listprotein[count]))
                {
                    // Get the protein
                    var protein = proteins[listprotein[count]];

                    // No protein groups allowed
                    if (protein.GetType() == typeof(Protein))
                    {
                        var dups = new NodeChildren<Node>();

                        // Look for duplicates and add to a duplicate list
                        dups.AddRange(FindDuplicates(protein));
                        if (dups.Count > 1)
                        {
                            // Create a protein group from the duplicates
                            var protGroup = new ProteinGroup(dups, globalIDTracker);
                            foreach (var dupProtein in dups)
                            {
                                // Remove proteins from the library, add the protein group
                                proteins.Remove(dupProtein.NodeName);
                            }

                            proteins.Add(protGroup.NodeName, protGroup);
                        }
                    }
                }
                count++;
            }
        }

        private void GroupPeptides(IDictionary<string, Node> peptides, IReadOnlyList<string> listpeptides, GlobalIDContainer globalIDTracker)
        {
            var count = 0;
            while (count != listpeptides.Count)
            {
                if (peptides.ContainsKey(listpeptides[count]))
                {
                    var peptide = peptides[listpeptides[count]];
                    if (peptide.GetType() == typeof(Peptide))
                    {
                        var dups = new NodeChildren<Node>();
                        dups.AddRange(FindDuplicates(peptide));
                        if (dups.Count > 1)
                        {
                            var protGroup = new PeptideGroup(dups, globalIDTracker);

                            foreach (var dupPeptide in dups)
                            {
                                peptides.Remove(dupPeptide.NodeName);
                            }

                            peptides.Add(protGroup.NodeName, protGroup);
                        }
                    }
                }
                count++;
            }

        }
         */

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

        //private NodeChildren<Node> FindDuplicates(Node protein)
        //{
        //    protein.Children.Sort();
        //    NodeChildren<Node> candidates = new NodeChildren<Node>();
        //    candidates = protein.Children[0].Children;
        //    candidates.RemoveChild(protein);
        //    int count = 0;
        //    while (candidates.Count != count)
        //    {
        //        if (protein.ChildCount != candidates[count].ChildCount)
        //        {
        //            candidates.RemoveAt(count);
        //        }
        //        count++;
        //    }

        //    for (int i = 1; i < protein.Children.Count; i++)
        //    {

        //        for (int j = 0; j < candidates.Count; j++)
        //        {
        //            Node temp = candidates.Get(j);
        //            if (!protein.Children.Get(i).Children.Contains(candidates.Get(j)))
        //            {
        //                candidates.RemoveChild(temp);
        //                j--;
        //            }
        //        }
        //    }
        //    return candidates;
        //}



    }
}
