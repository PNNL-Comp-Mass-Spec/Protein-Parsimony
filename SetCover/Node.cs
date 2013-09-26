using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SetCover
{
    public class Node
    {

        private static int IDNum = 0;


        public int Id { get; set; }
        public List<Node> children { get; set; }
        public String nodeName { get; set; }

        public Node() { }


        public Node(String nodeName)
        {
            this.nodeName = nodeName;
            this.Id = System.Threading.Interlocked.Increment(ref IDNum);
            this.children = new List<Node>();
        }
    }



    

}
