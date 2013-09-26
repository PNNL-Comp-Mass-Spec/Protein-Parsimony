using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SetCover
{
    class Group : Node
    {
        protected List<Node> nodeGroup;

        public Group(List<Node> groupedNodes)
        {
            this.nodeGroup = groupedNodes;
        }
    }
}
