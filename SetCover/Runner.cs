using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;


namespace SetCover
{
    class Runner 
    {
        public bool RunAlgorithm(string filename)
        {
            DataTable dt = Utilities.TextFileToDataTableAssignTypeString(filename, false);
            NodeBuilder nb = new NodeBuilder();
            List<Node> Peptides = new List<Node>();
            List<Node> Proteins = new List<Node>();
            nb.RunAlgorithm(dt, ref Proteins, ref Peptides);
            //TODO: speed collapser remove all peptides with multip 0 and associated proteins because
            //any protein with mp peptide of 0 will have no duplicates.  Collapse the remainder and 
            //put them back together.
            dt = null;
            NodeCollapser nc = new NodeCollapser();
            nc.RunAlgorithm(ref Proteins, ref Peptides);
            DFS dfs = new DFS();
            dfs.RunAlgorithm(ref Proteins);
            Cover cv = new Cover();
            cv.RunAlgorithm(ref Proteins);
            return true;
        }

    }
}
