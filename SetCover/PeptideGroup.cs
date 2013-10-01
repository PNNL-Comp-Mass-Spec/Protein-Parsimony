using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SetCover
{
    class PeptideGroup : Group
    {
        public PeptideGroup(string nodeName) : base(nodeName) { }

        public PeptideGroup(NodeChildren<Node> groupedNodes) : base(groupedNodes) { }
    }
}
