﻿using System.Collections.Generic;

namespace SetCover
{
    class DFS
    {
        Dictionary<int, Node> AllNodes = new Dictionary<int, Node>();

        public void RunAlgorithm(ref List<Node> Proteins)
        {
            foreach (ProteinGroup p in Proteins)
            {
                p.UpdateUntakenPeptides();
            }
            Proteins = ClusterNodes(Proteins);
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
        public void Search(Node input, HashSet<Node> searchNodes)
        {
            foreach (Node child in input.children)
            {
                if (!CheckID(child, searchNodes))
                {
                    searchNodes.Add(child);
                    AllNodes.Remove(child.Id);
                    Search(child, searchNodes);
                }
            }
        }

        public HashSet<Node> Search(Node input)
        {
            HashSet<Node> searchNodes = new HashSet<Node>();
            searchNodes.Add(input);
            AllNodes.Remove(input.Id);
            Search(input, searchNodes);
            return searchNodes;
        }

        public List<Node> ClusterNodes(List<Node> inNodes)
        {
            List<Node> clusters = new List<Node>();
            foreach (Node mynode in inNodes)
            {
                AllNodes.Add(mynode.Id, mynode);
            }
            foreach (Node mynode in inNodes)
            {
                if (AllNodes.ContainsKey(mynode.Id))
                {
                    Cluster cs = new Cluster();
                    cs.children = new NodeChildren<Node>(Search(mynode));
         //           cs.nodeName = String.Concat(cs.chi);
                    clusters.Add(cs);
                }
            }
            return clusters;
        }





    }
}