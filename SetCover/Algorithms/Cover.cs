using System.Collections.Generic;
using SetCover.Objects;

namespace SetCover.Algorithms
{
    /// <summary>
    /// Finds the best coverage using a greedy algorithm
    /// </summary>
    class Cover
    {

        public void RunAlgorithm(ref List<Node> inNode, out List<Node> outNode)
        {
            outNode = RunAlgorithm(inNode);
        }

        /// <summary>
        /// Finds the best coverage using a greedy algorithm
        /// </summary>
        /// <param name="inNode"></param>
        public List<Node> RunAlgorithm(List<Node> inNode)
        {
            //log.Info("Running Greedy Set Cover");

            var outNode = BulkFinder(inNode);
            return outNode;

        }

        public List<Node> BulkFinder(List<Node> inCluster)
        {
            var filteredList = new List<Node>();
            foreach (var node in inCluster)
            {
                var cs = (Cluster)node;
                filteredList.AddRange(GetCover(cs));
            }
            return filteredList;
        }

        private IEnumerable<Node> GetCover(Node cs)
        {
            var proteinSet = new List<Node>();
            var proteins = new List<Node>();
            // var peptides = new List<Node>();

            foreach (var node in cs.Children)
            {
                var t = node.GetType();
                if (t == typeof(ProteinGroup) ||
                    t == typeof(Protein))
                {
                    proteins.Add(node);
                }
                /*
                else
                {
                    peptides.Add(node);
                }
                */
            }

            //This loop is going to have to change.
            proteins.Sort();
            while (proteins.Count != 0 && ((ProteinGroup)proteins[proteins.Count - 1]).UntakenPeptide != 0)
            {
                var temp = proteins[proteins.Count - 1];
                proteinSet.Add(temp);
                proteins.Remove(temp);
                AdjustUntakenPeptides(temp);
                proteins.Sort();
            }

            return proteinSet;


        }

        private void AdjustUntakenPeptides(Node temp)
        {
            foreach (var child in temp.Children)
            {
                foreach (var node in child.Children)
                {
                    var pchild = (ProteinGroup)node;
                    pchild.UntakenPeptide--;
                }
            }
        }




    }
}
