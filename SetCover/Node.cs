using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SetCover
{
    public class Node : IComparable
    {

        private static int IDNum = 0;
        public int Id { get; set; }
        public NodeChildren<Node> children { get; set; }
        public String nodeName { get; set; }

        public Node() { }


        public Node(String nodeName)
        {
            this.nodeName = nodeName;
            this.Id = System.Threading.Interlocked.Increment(ref IDNum);
            this.children = new NodeChildren<Node>();

        }


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
