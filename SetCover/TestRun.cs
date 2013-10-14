using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace SetCover
{
    class TestRun
    {
        [Test]
        public void TestFile(string[] args)
        {
            string file = @"C:\Users\aldr699\Documents\Visual Studio 2010\Projects2\ProteinPars\SetCover\TestData\T_Row_Metadata.txt";
            Runner run = new Runner();
            run.RunAlgorithm(file,"");

        }

        [Test]
        public void TestNodeBuilder()
        {
            List<RowEntry> lr = new List<RowEntry>();
            lr.Add(new RowEntry { ProteinEntry = "prot_A", PeptideEntry = "pep_A" });
            lr.Add(new RowEntry { ProteinEntry = "prot_B", PeptideEntry = "pep_A" });
            lr.Add(new RowEntry { ProteinEntry = "prot_B", PeptideEntry = "pep_B" });
            lr.Add(new RowEntry { ProteinEntry = "prot_B", PeptideEntry = "pep_C" });
            lr.Add(new RowEntry { ProteinEntry = "prot_C", PeptideEntry = "pep_A" });
            lr.Add(new RowEntry { ProteinEntry = "prot_C", PeptideEntry = "pep_B" });
            lr.Add(new RowEntry { ProteinEntry = "prot_C", PeptideEntry = "pep_C" });
            lr.Add(new RowEntry { ProteinEntry = "prot_D", PeptideEntry = "pep_D" });
            lr.Add(new RowEntry { ProteinEntry = "prot_D", PeptideEntry = "pep_E" });

            var nodebuilder = new NodeBuilder();
            var nodecollapser = new NodeCollapser();
            var dfs = new DFS();
            var cover = new Cover();

            var Proteins = new Dictionary<string,Node>();
            var Peptides = new Dictionary<string,Node>();

            nodebuilder.RunAlgorithm(lr, out Proteins, out Peptides);
            nodecollapser.RunAlgorithm(ref Proteins, ref Peptides);
            var clProteins = Proteins.Values.ToList();
            dfs.RunAlgorithm(ref clProteins);
            cover.RunAlgorithm(ref clProteins);

            Utilities.WriteTableToConsole(clProteins);

            Console.ReadLine();

            //Expected output:
            //prot_C-prot_B	pep_A
            //prot_C-prot_B	pep_B
            //prot_C-prot_B	pep_C
            //prot_D	pep_E
            //prot_D	pep_D


        }

        [Test]
        public void TestResultDB()
        {
            string datadbfolder = @"C:\Users\aldr699\Documents\Visual Studio 2010\Projects2\ProteinPars\SetCover\TestData";
            string datafile = "Results.db3";

            var runner = new Runner();
            runner.RunAlgorithm(datadbfolder,datafile);


        }
    }
}
