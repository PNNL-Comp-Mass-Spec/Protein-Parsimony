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


        //Collapses redundant protein and peptides into single groups.
        public void CollapseNodes(
            Dictionary<string, Node> proteins,
            Dictionary<string, Node> peptides,
            GlobalIDContainer globalIDTracker)
        {
            var count = 0;
            var listprotein = proteins.Keys.ToList();//get a list of the keys
            while (count != listprotein.Count)
            {
                if (proteins.ContainsKey(listprotein[count]))//is the key there
                {
                    var protein = proteins[listprotein[count]]; //get the protein
                    if (protein.GetType() == typeof(Protein))//no protein groups allowed
                    {
                        var dups = new NodeChildren<Node>();
                        dups.AddRange(FindDuplicates(protein));  // look for duplicates and add to a duplicate list
                        if (dups.Count > 1)
                        {
                            //Create a proteins group from the duplicates
                            var PG = new ProteinGroup(dups, globalIDTracker);
                            foreach (var pp2 in dups)
                            {
                                //Remove proteins from the library, add the protein group
                                proteins.Remove(pp2.NodeName);
                            }

                            proteins.Add(PG.NodeName, PG);
                        }
                    }
                }
                count++;
            }

            // Same thing as above but with peptides
            count = 0;
            var listpeptides = peptides.Keys.ToList();
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
                            var PG = new PeptideGroup(dups, globalIDTracker);

                            foreach (var pp2 in dups)
                            {
                                peptides.Remove(pp2.NodeName);
                            }

                            peptides.Add(PG.NodeName, PG);
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
            //pulls out candidates with different counts of children than the master
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

            //may want to change the List to a Hashset to be faster.
            //finds identical sets.
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
