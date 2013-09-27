using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SetCover
{
    class DFS : AlgBase
    {
        Dictionary<int, Node> AllNodes = new Dictionary<int, Node>();

        public bool CheckID(Node nextNode, HashSet<Node> searchedNodes)
        {
            return searchedNodes.Contains(nextNode);
        }

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
            Search(input, searchNodes);
            return searchNodes;
        }

        public List<Cluster> ClusterNodes(List<Node> inNodes)
        {
            List<Cluster> clusters = new List<Cluster>();
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
                }
            }
            return clusters;
        }





    }
}
