using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SetCover
{
    /// <summary>
    /// Main object for generating the bipartite graph
    /// </summary>
    public class Node : IComparable
    {

        private static int IDNum = 0;
        public int Id { get; set; }
        public NodeChildren<Node> children { get; set; }
        public String nodeName { get; set; }

        public Node() 
        {
            this.Id = System.Threading.Interlocked.Increment(ref IDNum);
        }


        public Node(String nodeName)
        {
            this.nodeName = nodeName;
            this.Id = System.Threading.Interlocked.Increment(ref IDNum);
            this.children = new NodeChildren<Node>();

        }

        //Use number of children to sort.
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            Node otherNode = obj as Node;
            if (otherNode != null)
            {
                return this.children.Count.CompareTo(otherNode.children.Count);
            }
            else
            {
                throw new ArgumentException("Object is not a Node!");
            }


        }



    }



    

}
