using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace SetCover
{
    class Cover : AlgBase
    {
        public override void RunAlgorithm(ref List<Node> inNode, ref List<Node> outNode)
        {
            log.Info("Running Greedy Set Cover");
            outNode = BulkFinder(inNode);
        }

        public List<Node> BulkFinder(List<Node> inCluster)
        {
            List<Node> FilteredList = new List<Node>();
            foreach (Cluster cs in inCluster)
            {
                FilteredList.AddRange(GetCover(cs));
            }
            return FilteredList;
        }

        private IEnumerable<Node> GetCover(Cluster cs)
        {
            List<Node> ProteinSet = new List<Node>();
            List<Node> proteins = new List<Node>();
            List<Node> peptides = new List<Node>();
            foreach (Node node in cs.children)
            {
                Type t = node.GetType();
                if (t == typeof(ProteinGroup) ||
                    t == typeof(Protein))
                {
                    proteins.Add(node);
                }
                else
                {
                    peptides.Add(node);
                }
            }
            for (int i = proteins.Count - 1; i >= 0; i--)
            {                    
                proteins.Sort();
                Node temp = proteins[i];
                if (((ProteinGroup)temp).UntakenPeptide == 0)
                {
                    break;
                }
                ProteinSet.Add(temp);
                proteins.Remove(temp);
                AdjustUntakenPeptides(temp, proteins);
            }
            
            return ProteinSet;
            

        }

        private void AdjustUntakenPeptides(Node temp, List<Node> proteins)
        {
            foreach (Node child in temp.children)
            {
                foreach (ProteinGroup pchild in child.children)
                {
                    pchild.UntakenPeptide--;
                }
            }
        }

        
    }
}
