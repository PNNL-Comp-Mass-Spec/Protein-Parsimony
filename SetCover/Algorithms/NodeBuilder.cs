using System.Collections.Generic;
using SetCover.Objects;

namespace SetCover.Algorithms
{
    class NodeBuilder
    /// <summary>
    /// Build a bipartite graph of proteins and peptides
    /// </summary>
    /// <remarks>
    /// For more information, see Step 1 of Figure 1 in manuscript
    /// "Proteomic Parsimony through Bipartite Graph Analysis Improves Accuracy and Transparency"
    /// by Bing Zhang, Matthew C. Chambers, and David L. Tabb
    /// PMID: 17676885
    /// https://www.ncbi.nlm.nih.gov/pmc/articles/PMC2810678
    /// </remarks>
    {

        private Dictionary<string, Node> mProteins;
        private Dictionary<string, Node> mPeptides;

        /// <summary>
        /// Group proteins having similar peptides
        /// </summary>
        /// <param name="peptideProteinMapList">List of protein to peptide mappings</param>
        /// <param name="proteins">Dictionary of proteins and children</param>
        /// <param name="peptides">Dictionary of peptides and children</param>
        public void RunAlgorithm(
            List<RowEntry> peptideProteinMapList,
            out Dictionary<string, Node> proteins,
            out Dictionary<string, Node> peptides)
        {

            mProteins = new Dictionary<string, Node>(peptideProteinMapList.Count);
            mPeptides = new Dictionary<string, Node>(peptideProteinMapList.Count);

            BuildNodes(peptideProteinMapList);

            proteins = mProteins;
            peptides = mPeptides;
        }

        /// <summary>
        /// Builds the nodes for a bipartite graph composed of proteins on one side
        /// and peptides on the other
        /// </summary>
        /// <param name="peptideProteinMapList">List of protein to peptide mappings</param>
        private void BuildNodes(IEnumerable<RowEntry> peptideProteinMapList)
        {
            foreach (var row in peptideProteinMapList)
            {

                var prot = new Protein(row.ProteinEntry);
                var pep = new Peptide(row.PeptideEntry);

                var proteinDefined = mProteins.ContainsKey(prot.NodeName);
                var peptideDefined = mPeptides.ContainsKey(pep.NodeName);

                if (proteinDefined && peptideDefined)
                {
                    if (!mProteins[prot.NodeName].Children.Contains(mPeptides[pep.NodeName]))
                    {
                        mProteins[prot.NodeName].Children.Add(mPeptides[pep.NodeName]);
                    }
                    if (!mPeptides[pep.NodeName].Children.Contains(mProteins[prot.NodeName]))
                    {
                        mPeptides[pep.NodeName].Children.Add(mProteins[prot.NodeName]);
                    }
                }
                else if (proteinDefined)
                {
                    mProteins[prot.NodeName].Children.Add(pep);
                    pep.Children.Add(mProteins[prot.NodeName]);
                    mPeptides.Add(pep.NodeName, pep);

                }
                else if (peptideDefined)
                {
                    mPeptides[pep.NodeName].Children.Add(prot);
                    prot.Children.Add(mPeptides[pep.NodeName]);
                    mProteins.Add(prot.NodeName, prot);
                }
                else
                {
                    prot.Children.Add(pep);
                    pep.Children.Add(prot);
                    mProteins.Add(prot.NodeName, prot);
                    mPeptides.Add(pep.NodeName, pep);
                }
            }
        }

        /// <summary>
        /// Ensures that all proteins in the graph have up-to-date untaken peptide counts
        /// </summary>
        public void UpdateUntakenPeptides()
        {
            foreach (var item in mProteins.Values)
            {
                if (item is ProteinGroup p)
                {
                    p.UpdateUntakenPeptides();
                }
            }
        }

    }
}
