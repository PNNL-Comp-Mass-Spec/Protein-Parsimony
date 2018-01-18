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
        public void TestFile()
        {
            var file = @"\\proto-2\UnitTest_Files\ProteinParsimony\T_Row_Metadata.txt";

            Runner.GetDefaultOutputFileNames(file, out var parsimonyResultsFilePath, out var proteinGroupMembersFilePath);

            var run = new Runner();
            run.RunGUIAlgorithm(file, parsimonyResultsFilePath, proteinGroupMembersFilePath);

        }

        [Test]
        public void TestNodeBuilderA()
        {
            var peptideProteinMapList = new List<RowEntry>
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

            var expectedOutput = new List<string> {
                "Protein\tPeptide",
                "prot_C; prot_B\tpep_A",
                "prot_C; prot_B\tpep_C",
                "prot_C; prot_B\tpep_B",
                "prot_D\tpep_E",
                "prot_D\tpep_D"};

            TestNodeBuilder(peptideProteinMapList, expectedOutput);

        }

        [Test]
        public void TestNodeBuilderB()
        {
            var peptideProteinMapList = new List<RowEntry>
            {
                new RowEntry {ProteinEntry = "prot_A", PeptideEntry = "pep_A"},
                new RowEntry {ProteinEntry = "prot_B", PeptideEntry = "pep_A"},
                new RowEntry {ProteinEntry = "prot_B", PeptideEntry = "pep_B"},
                new RowEntry {ProteinEntry = "prot_B", PeptideEntry = "pep_C"},
                new RowEntry {ProteinEntry = "prot_C", PeptideEntry = "pep_A"},
                new RowEntry {ProteinEntry = "prot_C", PeptideEntry = "pep_B"},
                new RowEntry {ProteinEntry = "prot_C", PeptideEntry = "pep_C"},
                new RowEntry {ProteinEntry = "prot_D", PeptideEntry = "pep_D"},
                new RowEntry {ProteinEntry = "prot_D", PeptideEntry = "pep_E"},
                new RowEntry {ProteinEntry = "prot_E", PeptideEntry = "pep_F"},
                new RowEntry {ProteinEntry = "prot_E", PeptideEntry = "pep_G"},
                new RowEntry {ProteinEntry = "prot_E", PeptideEntry = "pep_H"},
                new RowEntry {ProteinEntry = "prot_E", PeptideEntry = "pep_I"},
                new RowEntry {ProteinEntry = "prot_E", PeptideEntry = "pep_K"},
                new RowEntry {ProteinEntry = "prot_F", PeptideEntry = "pep_F"},
                new RowEntry {ProteinEntry = "prot_G", PeptideEntry = "pep_I"},
                new RowEntry {ProteinEntry = "prot_G", PeptideEntry = "pep_G"},
                new RowEntry {ProteinEntry = "prot_H", PeptideEntry = "pep_F"},
                new RowEntry {ProteinEntry = "prot_H", PeptideEntry = "pep_G"},
                new RowEntry {ProteinEntry = "prot_I", PeptideEntry = "pep_H"},
                new RowEntry {ProteinEntry = "prot_I", PeptideEntry = "pep_J"},
                new RowEntry {ProteinEntry = "prot_J", PeptideEntry = "pep_J"},
                new RowEntry {ProteinEntry = "prot_K", PeptideEntry = "pep_L"},
                new RowEntry {ProteinEntry = "prot_L", PeptideEntry = "pep_M"},
            };

            var expectedOutput = new List<string> {
                "Protein\tPeptide",
                "prot_C; prot_B\tpep_A",
                "prot_C; prot_B\tpep_C",
                "prot_C; prot_B\tpep_B",
                "prot_D\tpep_E",
                "prot_D\tpep_D",
                "prot_E\tpep_K",
                "prot_E\tpep_H",
                "prot_E\tpep_I",
                "prot_E\tpep_F",
                "prot_E\tpep_G",
                "prot_K\tpep_L",
                "prot_L\tpep_M"
            };

            TestNodeBuilder(peptideProteinMapList, expectedOutput);

        }
        private void TestNodeBuilder(List<RowEntry> peptideProteinMapList, IReadOnlyList<string> expectedOutput)
        {

            var nodebuilder = new NodeBuilder();
            var nodecollapser = new NodeCollapser();
            var dfs = new DFS();
            var cover = new Cover();

            var globalIDTracker = new GlobalIDContainer();

            nodebuilder.RunAlgorithm(peptideProteinMapList, out var proteins, out var peptides);

            nodecollapser.RunAlgorithm(proteins, peptides, globalIDTracker);

            var proteinsWithChildren = proteins.Values.ToList();
            var clusteredProteinSets = dfs.RunAlgorithm(proteinsWithChildren);
            var clusteredProteins = cover.RunAlgorithm(clusteredProteinSets);

            var outLines = Utilities.ConvertNodesToStringList(clusteredProteins, globalIDTracker);

            foreach (var item in outLines)
            {
                Console.WriteLine(item);
            }

            if (expectedOutput.Count == 0)
                return;

            for (var i = 0; i < outLines.Count; i++)
            {
                if (i >= expectedOutput.Count)
                {
                    Assert.Fail("Extra, unexpected output line: " + outLines[i]);
                }

                Assert.AreEqual(expectedOutput[i], outLines[i], "Mismatch on line {0}", i + 1);
            }

            if (expectedOutput.Count > outLines.Count)
            {
                Assert.Fail("Output did not include additional, expected lines, starting with " + expectedOutput[outLines.Count]);
            }
        }

        [Test]
        public void TestResultDB()
        {
            var datadbfolder = @"\\proto-2\UnitTest_Files\ProteinParsimony";
            var datafile = "Results.db3";

            var runner = new Runner();
            runner.RunAlgorithm(datadbfolder, datafile, "T_Row_Metadata");


        }
    }
}
