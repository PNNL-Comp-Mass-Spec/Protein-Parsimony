using System;
using System.Collections.Generic;
using PRISM;
using SetCover.Objects;

namespace SetCover.Algorithms
{
    /// <summary>
    /// Cluster sets of nodes using a depth first search approach
    /// </summary>
    class DFS : EventNotifier
    {
        readonly Dictionary<int, Node> AllNodes = new Dictionary<int, Node>();

        private const int MAX_RECURSION_DEPTH = 4000;
        private int mRecursionDepthLimitCount;
        private int mRecursionDepthLimitReportThreshold = 10;

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

            if (recursionDepth >= MAX_RECURSION_DEPTH)
            {
                // C# can handle a recursion depth of ~5000
                // For safety, we will limit it to 4000
                mRecursionDepthLimitCount++;

                if (mRecursionDepthLimitCount > 5 && mRecursionDepthLimitCount < mRecursionDepthLimitReportThreshold)
                {
                    return;
                }

                if (mRecursionDepthLimitCount <= 1)
                {
                    OnWarningEvent(string.Format(
                                       "Recursively called SetCover.Algorithms.DFS.Search to a depth of {0}; " +
                                       "aborting processing of remaining children", recursionDepth));
                }
                else
                {
                    OnWarningEvent(string.Format(
                                       "Recursively called SetCover.Algorithms.DFS.Search to the maximum depth {0} times; " +
                                       "aborting processing of remaining children", mRecursionDepthLimitCount));
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
