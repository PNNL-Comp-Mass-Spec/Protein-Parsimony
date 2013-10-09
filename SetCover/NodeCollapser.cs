using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SetCover
{
    class NodeCollapser : IAlgBase
    {
        public void RunAlgorithm(ref Dictionary<string, Node> protein, ref Dictionary<string, Node> pep)
        {
            CollapseNodes(ref protein, ref pep);
        }

        public void RunAlgorithm(ref List<Node> inNode)
        {
            List<Node> dummy = new List<Node>();
   //         CollapseNodes(ref inNode, ref dummy);
        }


        //Collapses redundant protein and peptides into single groups.
        public void CollapseNodes(ref Dictionary<string, Node> proteins, ref Dictionary<string, Node> peptides)
        {
     //       peptides.Sort();
     //       proteins.Sort();
            List<Node> tempList = new List<Node>();
            int count = 0;
            List<string> listprotein = proteins.Keys.ToList();
            while(count != proteins.Count)
            {
                if(proteins.ContainsKey(listprotein[count]))
                {
                    Node protein = proteins[listprotein[count]];

                    if (protein.GetType() == typeof(Protein))
                    {
                        NodeChildren<Node> dups = new NodeChildren<Node>();
                        dups.AddRange(FindDuplicates(protein));
                        if (dups.Count > 1)
                        {
                            ProteinGroup PG = new ProteinGroup(dups);
                            foreach (Node pp2 in dups)
                            {
                                proteins.Remove(pp2.nodeName);
                            }
                            proteins.Add(PG.nodeName, PG);
                        }
                    }
                }
                count++;
            }
            List<string> listpeptides = peptides.Keys.ToList();
            while (count != peptides.Count)
            {
                if (peptides.ContainsKey(listpeptides[count]))
                {
                    Node peptide = peptides[listpeptides[count]];
                    if (peptide.GetType() == typeof(Peptide))
                    {
                        NodeChildren<Node> dups = new NodeChildren<Node>();
                        dups.AddRange(FindDuplicates(peptide));
                        if (dups.Count > 1)
                        {
                            PeptideGroup PG = new PeptideGroup(dups);

                            foreach (Node pp2 in dups)
                            {
                                peptides.Remove(pp2.nodeName);
                            }
                            peptides.Add(PG.nodeName, PG);
                        }
                    }
                }
                count++;
            }
        }

        private NodeChildren<Node> FindDuplicates(Node node)
        {
            node.children.Sort();
            NodeChildren<Node> candidates = new NodeChildren<Node>();
            candidates.AddRange(node.children[0].children);
            candidates.Remove(node);

            int count = 0;
            //pulls out candidates with different counts of children than the master
            while(candidates.Count != count)
            {
                if (node.children.Count != candidates[count].children.Count)
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
            for (int i = 0; i < node.children.Count; i++)
            {
                count = 0;
                while(candidates.Count != count)
                {
                    if (!node.children[i].children.Contains(candidates[count]))
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
        //    protein.children.Sort();
        //    NodeChildren<Node> candidates = new NodeChildren<Node>();
        //    candidates = protein.children[0].children;
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

        //    for (int i = 1; i < protein.children.Count; i++)
        //    {

        //        for (int j = 0; j < candidates.Count; j++)
        //        {
        //            Node temp = candidates.Get(j);
        //            if (!protein.children.Get(i).children.Contains(candidates.Get(j)))
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
