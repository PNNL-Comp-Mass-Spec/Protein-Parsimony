using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace SetCover
{
    class NodeBuilder
    {
        Dictionary<string, Node> Proteins = new Dictionary<string, Node>();
        Dictionary<string,Node> Peptides = new Dictionary<string, Node>();

        public void RunAlgorithm(DataTable dt, ref List<Node> prots, ref List<Node> peps)
        {
            BuildNodes(dt);
            prots = Proteins.Values.ToList();
            peps = Peptides.Values.ToList();
        }
        /// <summary>
        /// Builds the nodes for a bipartite graph composed of proteins 
        /// on one side peptides on the other
        /// </summary>
        /// <param name="dt">input table must have at peptide and protein column</param>
        private void BuildNodes(DataTable dt)
        {
            foreach (DataRow drow in dt.Rows)
            {
                Protein prot = new Protein((string)drow["Protein"]);
                Peptide pep = new Peptide((string)drow["Peptide"]);
                if (Proteins.ContainsKey(prot.nodeName) && Peptides.ContainsKey(pep.nodeName))
                {
                    Proteins[prot.nodeName].children.Add(Peptides[pep.nodeName]);
                    Peptides[pep.nodeName].children.Add(Proteins[prot.nodeName]);
                }
                else if (Proteins.ContainsKey(prot.nodeName))
                {
                    Proteins[prot.nodeName].children.Add(pep);
                    pep.children.Add(Proteins[prot.nodeName]);
                    Peptides.Add(pep.nodeName, pep);

                }
                else if (Peptides.ContainsKey(pep.nodeName))
                {
                    Peptides[pep.nodeName].children.Add(prot);
                    prot.children.Add(Peptides[pep.nodeName]);
                    Proteins.Add(prot.nodeName, prot);
                }
                else
                {
                    prot.children.Add(pep);
                    pep.children.Add(prot);
                    Proteins.Add(prot.nodeName, prot);
                    Peptides.Add(pep.nodeName, pep);
                }
            }
        }

        /// <summary>
        /// Ensures that all proteins in the graph have had the untaken peptide supdates.
        /// </summary>
        public void UpdateUntakenPeptides()
        {
            foreach (Protein p in Proteins.Values)
            {
                p.UntakenPeptide = p.children.Count;
            }
        }



    }
}
