using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace SetCover
{
    class NodeBuilder
    {

        Dictionary<string, Node> Proteins;
        Dictionary<string, Node> Peptides;

        public void RunAlgorithm(List<RowEntry> toParsimonize, out Dictionary<string, Node> prots, out Dictionary<string, Node> peps)
        {
            
                this.Proteins = new Dictionary<string, Node>(toParsimonize.Count);
                this.Peptides = new Dictionary<string, Node>(toParsimonize.Count);
                BuildNodes(toParsimonize);
                prots = this.Proteins;
                peps = this.Peptides;
        }
        /// <summary>
        /// Builds the nodes for a bipartite graph composed of proteins 
        /// on one side peptides on the other
        /// </summary>
        /// <param name="toParsimonize">input table must have at peptide and protein column</param>
        private void BuildNodes(List<RowEntry> toParsimonize)
        {
            foreach (RowEntry row in toParsimonize)
            {

                Protein prot = new Protein(row.ProteinEntry);
                Peptide pep = new Peptide(row.PeptideEntry);

                if (Proteins.ContainsKey(prot.nodeName) && Peptides.ContainsKey(pep.nodeName))
                {
                    if (!Proteins[prot.nodeName].children.Contains(Peptides[pep.nodeName]))
                    {
                        Proteins[prot.nodeName].children.Add(Peptides[pep.nodeName]);
                    }
                    if (!Peptides[pep.nodeName].children.Contains(Proteins[prot.nodeName]))
                    {
                        Peptides[pep.nodeName].children.Add(Proteins[prot.nodeName]);
                    }
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
