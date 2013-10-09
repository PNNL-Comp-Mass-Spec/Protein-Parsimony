using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;


namespace SetCover
{
    public class Runner 
    {
        public bool RunAlgorithm(string filename)
        {
            DataTable dt = Utilities.TextFileToDataTableAssignTypeString(filename, false);
            NodeBuilder nb = new NodeBuilder();
            Dictionary<string, Node> Peptides = new Dictionary<string, Node>();
            Dictionary<string, Node> Proteins = new Dictionary<string, Node>();
            nb.RunAlgorithm(dt, ref Proteins, ref Peptides);
            //TODO: speed collapser remove all peptides with multip 0 and associated proteins because
            //any protein with mp peptide of 0 will have no duplicates.  Collapse the remainder and 
            //put them back together.
            dt = null;
            NodeCollapser nc = new NodeCollapser();
            nc.RunAlgorithm(ref Proteins, ref Peptides);
            foreach (ProteinGroup p in Proteins.Values)
            {
                p.UpdateUntakenPeptides();
            }
            
            DFS dfs = new DFS();
            List<Node> ClProteins = Proteins.Values.ToList();
            dfs.RunAlgorithm(ref ClProteins);
            Cover cv = new Cover();
            //Number of proteins is not being reduced.
            cv.RunAlgorithm(ref ClProteins);

            string mydir = System.IO.Path.GetDirectoryName(filename);
            string outpath = System.IO.Path.Combine(mydir, "testout.txt");
            Utilities.WriteTable(ClProteins, outpath);

            return true;
        }

    }
}
