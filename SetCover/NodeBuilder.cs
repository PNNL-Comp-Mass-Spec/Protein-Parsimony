using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace SetCover
{
    class NodeBuilder
    {
        List<Node> Proteins = new List<Node>();
        List<Node> Peptides = new List<Node>();

        public void RunAlgorithm(DataTable dt, ref List<Node> prots, ref List<Node> peps)
        {
            BuildNodes(dt);
            prots = Proteins;
            peps = Peptides;
        }

        private void BuildNodes(DataTable dt)
        {
            foreach (DataRow drow in dt.Rows)
            {
                Protein prot = new Protein((string)drow["Protein"]);
                Peptide pep = new Peptide((string)drow["Peptide"]);
                if (Proteins.Contains(prot))
                {
                    Proteins[Proteins.IndexOf(prot)].children.Add(pep);
                }
                else
                {
                    prot.children.Add(pep);
                    Proteins.Add(prot);
                  
                }
                if (Peptides.Contains(pep))
                {
                    Peptides[Peptides.IndexOf(pep)].children.Add(prot);
                }
                else
                {
                    pep.children.Add(prot);
                    Peptides.Add(pep);
                }
            }
        }

        public void UpdateUntakenPeptides()
        {
            foreach (Protein p in Proteins)
            {
                p.UntakenPeptide = p.ChildCount;
            }
        }



    }
}
