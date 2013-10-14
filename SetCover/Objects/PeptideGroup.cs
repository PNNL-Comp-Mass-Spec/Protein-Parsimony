
namespace SetCover
{
    /// <summary>
    /// Groups peptides which have the same proteins as children.
    /// </summary>
    class PeptideGroup : Group
    {
        public PeptideGroup(string nodeName) : base(nodeName) { }

        public PeptideGroup(NodeChildren<Node> groupedNodes) : base(groupedNodes) { }
    }
}
