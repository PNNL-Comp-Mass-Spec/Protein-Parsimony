using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SetCover.Algorithms;
using SetCover.Objects;

namespace SetCover
{
    class TestRun
    {
        [Test]
        public void TestFile(string[] args)
        {
            var file = @"C:\Users\aldr699\Documents\Visual Studio 2010\Projects2\ProteinPars\SetCover\TestData\T_Row_Metadata.txt";
            var run = new Runner();
            run.RunAlgorithm(file,"");

        }

        [Test]
        public void TestNodeBuilder()
        {
            var lr = new List<RowEntry>
            {
                new RowEntry {ProteinEntry = "prot_A", PeptideEntry = "pep_A"},
                new RowEntry {ProteinEntry = "prot_B", PeptideEntry = "pep_A"},
                new RowEntry {ProteinEntry = "prot_B", PeptideEntry = "pep_B"},
                new RowEntry {ProteinEntry = "prot_B", PeptideEntry = "pep_C"},
                new RowEntry {ProteinEntry = "prot_C", PeptideEntry = "pep_A"},
                new RowEntry {ProteinEntry = "prot_C", PeptideEntry = "pep_B"},
                new RowEntry {ProteinEntry = "prot_C", PeptideEntry = "pep_C"},
                new RowEntry {ProteinEntry = "prot_D", PeptideEntry = "pep_D"},
                new RowEntry {ProteinEntry = "prot_D", PeptideEntry = "pep_E"}
            };

            var nodebuilder = new NodeBuilder();
            var nodecollapser = new NodeCollapser();
            var dfs = new DFS();
            var cover = new Cover();

            var globalIDTracker = new GlobalIDContainer();

            nodebuilder.RunAlgorithm(lr, out var Proteins, out var Peptides);

            nodecollapser.RunAlgorithm(Proteins, Peptides, globalIDTracker);

            var clProteins = Proteins.Values.ToList();
            dfs.RunAlgorithm(ref clProteins);
            cover.RunAlgorithm(ref clProteins);

            Utilities.WriteTableToConsole(clProteins, globalIDTracker);

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
            var datadbfolder = @"C:\Users\aldr699\Documents\Visual Studio 2010\Projects2\ProteinPars\SetCover\TestData";
            var datafile = "Results.db3";

            var runner = new Runner();
            runner.RunAlgorithm(datadbfolder,datafile);


        }
    }
}
