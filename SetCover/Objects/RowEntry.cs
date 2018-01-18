namespace SetCover.Objects
{
    public struct RowEntry
    {
        public string ProteinEntry;
        public string PeptideEntry;

        public override string ToString()
        {
            return string.Format("{0} -> {1}", ProteinEntry, PeptideEntry);
        }
    }
}
