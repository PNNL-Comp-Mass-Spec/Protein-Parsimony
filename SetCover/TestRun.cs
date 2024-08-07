﻿using System;
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
            const string file = @"\\proto-2\UnitTest_Files\ProteinParsimony\T_Row_Metadata.txt";

            Runner.GetDefaultOutputFileNames(file, out var parsimonyResultsFilePath, out var proteinGroupMembersFilePath);

            var run = new Runner();
            run.ProcessTextFile(file, parsimonyResultsFilePath, proteinGroupMembersFilePath);
        }

        [Test]
        public void TestNodeBuilderA()
        {
            var peptideProteinMapList = new List<RowEntry>
            {
                new() {ProteinEntry = "prot_A", PeptideEntry = "pep_A"},
                new() {ProteinEntry = "prot_B", PeptideEntry = "pep_A"},
                new() {ProteinEntry = "prot_B", PeptideEntry = "pep_B"},
                new() {ProteinEntry = "prot_B", PeptideEntry = "pep_C"},
                new() {ProteinEntry = "prot_C", PeptideEntry = "pep_A"},
                new() {ProteinEntry = "prot_C", PeptideEntry = "pep_B"},
                new() {ProteinEntry = "prot_C", PeptideEntry = "pep_C"},
                new() {ProteinEntry = "prot_D", PeptideEntry = "pep_D"},
                new() {ProteinEntry = "prot_D", PeptideEntry = "pep_E"}
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
                new() {ProteinEntry = "prot_A", PeptideEntry = "pep_A"},
                new() {ProteinEntry = "prot_B", PeptideEntry = "pep_A"},
                new() {ProteinEntry = "prot_B", PeptideEntry = "pep_B"},
                new() {ProteinEntry = "prot_B", PeptideEntry = "pep_C"},
                new() {ProteinEntry = "prot_C", PeptideEntry = "pep_A"},
                new() {ProteinEntry = "prot_C", PeptideEntry = "pep_B"},
                new() {ProteinEntry = "prot_C", PeptideEntry = "pep_C"},
                new() {ProteinEntry = "prot_D", PeptideEntry = "pep_D"},
                new() {ProteinEntry = "prot_D", PeptideEntry = "pep_E"},
                new() {ProteinEntry = "prot_E", PeptideEntry = "pep_F"},
                new() {ProteinEntry = "prot_E", PeptideEntry = "pep_G"},
                new() {ProteinEntry = "prot_E", PeptideEntry = "pep_H"},
                new() {ProteinEntry = "prot_E", PeptideEntry = "pep_I"},
                new() {ProteinEntry = "prot_E", PeptideEntry = "pep_K"},
                new() {ProteinEntry = "prot_F", PeptideEntry = "pep_F"},
                new() {ProteinEntry = "prot_G", PeptideEntry = "pep_I"},
                new() {ProteinEntry = "prot_G", PeptideEntry = "pep_G"},
                new() {ProteinEntry = "prot_H", PeptideEntry = "pep_F"},
                new() {ProteinEntry = "prot_H", PeptideEntry = "pep_G"},
                new() {ProteinEntry = "prot_I", PeptideEntry = "pep_H"},
                new() {ProteinEntry = "prot_I", PeptideEntry = "pep_J"},
                new() {ProteinEntry = "prot_J", PeptideEntry = "pep_J"},
                new() {ProteinEntry = "prot_K", PeptideEntry = "pep_L"},
                new() {ProteinEntry = "prot_L", PeptideEntry = "pep_M"},
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
                "prot_I\tpep_H",
                "prot_I\tpep_J",
                "prot_K\tpep_L",
                "prot_L\tpep_M"
            };

            TestNodeBuilder(peptideProteinMapList, expectedOutput);
        }
        private void TestNodeBuilder(List<RowEntry> peptideProteinMapList, IReadOnlyList<string> expectedOutput)
        {
            var nodeBuilder = new NodeBuilder();
            var nodeCollapser = new NodeCollapser();
            var dfs = new DFS();
            var cover = new Cover();

            var globalIDTracker = new GlobalIDContainer();

            nodeBuilder.RunAlgorithm(peptideProteinMapList, out var proteins, out var peptides);

            nodeCollapser.RunAlgorithm(proteins, peptides, globalIDTracker);

            var proteinsWithChildren = proteins.Values.ToList();
            var clusteredProteinSets = dfs.RunAlgorithm(proteinsWithChildren);
            var clusteredProteins = cover.RunAlgorithm(clusteredProteinSets);

            var outLines = Utilities.ConvertNodesToStringList(clusteredProteins, globalIDTracker);

            foreach (var item in outLines)
            {
                Console.WriteLine(item);
            }

            if (expectedOutput == null || expectedOutput.Count == 0)
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
            const string dataDbFolder = @"\\proto-2\UnitTest_Files\ProteinParsimony";
            const string dataFile = "Results.db3";
            const string sourceTableName = Runner.DEFAULT_SQLITE_TABLE;

            var runner = new Runner();
            runner.ProcessSQLite(dataDbFolder, dataFile, sourceTableName);
        }
    }
}
