using System;
using System.Collections.Generic;
using PRISM;
using SetCover.Objects;

namespace SetCover.Algorithms
{
    /// <summary>
    /// Cluster sets of nodes using a depth first search (DFS) approach
    /// </summary>
    /// <remarks>
    /// For more information, see Step 3 of Figure 1 in manuscript
    /// "Proteomic Parsimony through Bipartite Graph Analysis Improves Accuracy and Transparency"
    /// by Bing Zhang, Matthew C. Chambers, and David L. Tabb
    /// PMID: 17676885
    /// https://www.ncbi.nlm.nih.gov/pmc/articles/PMC2810678
    /// </remarks>
    internal class DFS : EventNotifier
    {
        readonly Dictionary<int, Node> AllNodes = new();

        private int mRecursionDepthLimitCount;
        private int mRecursionDepthLimitReportThreshold = 10;

        /// <summary>
        /// Maximum allowed recursion depth
        /// </summary>
        /// <remarks>Defaults to 3500; increase to 20000 if the calling procedure has allocated more memory for the stack</remarks>
        public int MaxRecursionDepth { get; set; } = Runner.DEFAULT_MAX_DFS_RECURSION_DEPTH;

        public List<Node> RunAlgorithm(List<Node> proteins)
        {
            foreach (var item in proteins)
            {
                if (item is ProteinGroup p)
                {
                    p.UpdateUntakenPeptides();
                }
            }

            var clusteredProteins = ClusterNodes(proteins);
            return clusteredProteins;
        }

        public bool CheckID(Node nextNode, HashSet<Node> searchedNodes)
        {
            return searchedNodes.Contains(nextNode);
        }

        /// <summary>
        /// Using a depth first search to cluster sets of nodes for set cover assignment
        /// </summary>
        /// <param name="input"></param>
        /// <param name="searchNodes"></param>
        /// <param name="recursionDepth"></param>
        public void Search(Node input, HashSet<Node> searchNodes, int recursionDepth)
        {
            if (recursionDepth >= MaxRecursionDepth)
            {
                // C# can handle a recursion depth of ~4000 with the default stack size of 1 MB
                // This limit can be increased by using a larger stack size

                mRecursionDepthLimitCount++;

                if (mRecursionDepthLimitCount > 5 && mRecursionDepthLimitCount < mRecursionDepthLimitReportThreshold)
                {
                    return;
                }

                if (mRecursionDepthLimitCount <= 1)
                {
                    OnWarningEvent(
                        "Recursively called SetCover.Algorithms.DFS.Search to a depth of {0}; " +
                        "aborting processing of remaining children", recursionDepth);
                }
                else
                {
                    OnWarningEvent(
                        "Recursively called SetCover.Algorithms.DFS.Search to the maximum depth {0} times; " +
                        "aborting processing of remaining children", mRecursionDepthLimitCount);
                }

                mRecursionDepthLimitReportThreshold += (int)Math.Floor(mRecursionDepthLimitReportThreshold * 0.25);

                return;
            }

            foreach (var child in input.Children)
            {
                if (!CheckID(child, searchNodes))
                {
                    searchNodes.Add(child);
                    AllNodes.Remove(child.Id);
                    Search(child, searchNodes, recursionDepth + 1);
                }
            }
        }

        public HashSet<Node> Search(Node input)
        {
            var searchNodes = new HashSet<Node> {
                input
            };
            AllNodes.Remove(input.Id);
            Search(input, searchNodes, 0);
            return searchNodes;
        }

        public List<Node> ClusterNodes(List<Node> inNodes)
        {
            var clusters = new List<Node>();
            foreach (var myNode in inNodes)
            {
                AllNodes.Add(myNode.Id, myNode);
            }

            foreach (var myNode in inNodes)
            {
                if (AllNodes.ContainsKey(myNode.Id))
                {
                    var cs = new Cluster
                    {
                        Children = new NodeChildren<Node>(Search(myNode))
                    };

                    //		   cs.nodeName = string.Concat(cs.chi);
                    clusters.Add(cs);
                }
            }
            return clusters;
        }
    }
}
