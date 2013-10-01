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
                prot.children.Add(pep);
                pep.children.Add(prot);

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
