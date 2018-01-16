using System.Collections.Generic;
using SetCover.Objects;

namespace SetCover.Algorithms
{
    class Cover
    {
        public void RunAlgorithm(ref List<Node> inNode, ref List<Node> outNode)
        {
            RunAlgorithm(ref inNode);
        }

        /// <summary>
        /// Finds the best coverage using a greedy algorithm
        /// </summary>
        /// <param name="inNode"></param>
        public void RunAlgorithm(ref List<Node> inNode)
        {
            //log.Info("Running Greedy Set Cover");

            inNode = BulkFinder(inNode);

        }

        public List<Node> BulkFinder(List<Node> inCluster)
        {
            var FilteredList = new List<Node>();
            foreach (var node in inCluster)
            {
                var cs = (Cluster)node;
                FilteredList.AddRange(GetCover(cs));
            }
            return FilteredList;
        }

        private IEnumerable<Node> GetCover(Node cs)
        {
            var ProteinSet = new List<Node>();
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
                ProteinSet.Add(temp);
                proteins.Remove(temp);
                AdjustUntakenPeptides(temp);
                proteins.Sort();
            }

            return ProteinSet;


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
