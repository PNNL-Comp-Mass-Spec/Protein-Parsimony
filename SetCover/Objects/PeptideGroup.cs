
namespace SetCover.Objects
{
    /// <summary>
    /// Groups peptides which have the same proteins as children.
    /// </summary>
    class PeptideGroup : Group
    {
        public PeptideGroup(string nodeName)
            : base(NodeTypeName.Peptide, nodeName)
        { }

        public PeptideGroup(NodeChildren<Node> groupedNodes, GlobalIDContainer globalIDTracker)
            : base(NodeTypeName.PeptideGroup, groupedNodes, globalIDTracker)
        { }
    }
}
