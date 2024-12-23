﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading;
using Mage;
using PRISM;
using SetCover.Algorithms;
using SetCover.Objects;

namespace SetCover
{
    /// <summary>
    /// Utility for forming protein groups using a list of peptide identifications and their associated proteins
    /// </summary>
    /// <remarks>
    /// For more information, see Figure 1 in manuscript
    /// "Proteomic Parsimony through Bipartite Graph Analysis Improves Accuracy and Transparency"
    /// by Bing Zhang, Matthew C. Chambers, and David L. Tabb
    /// PMID: 17676885
    /// https://www.ncbi.nlm.nih.gov/pmc/articles/PMC2810678
    /// </remarks>
    public class Runner : EventNotifier
    {
        public const string DEFAULT_SQLITE_TABLE = "T_Row_Metadata";
        public const string PARSIMONY_GROUPING_TABLE = "T_Parsimony_Grouping";
        public const string PARSIMONY_GROUP_MEMBERS_TABLE = "T_Parsimony_Group_Members";

        public const int DEFAULT_MAX_DFS_RECURSION_DEPTH = 3500;

        private const int MAX_RUNTIME_HOURS = 24;

        private List<RowEntry> mPeptideProteinMapList;
        private List<Node> mClusteredProteins;
        private GlobalIDContainer mGlobalIDTracker;
        private bool mProcessingSucceeded;
        private int mMaxDFSRecursionDepth = DEFAULT_MAX_DFS_RECURSION_DEPTH;

        /// <summary>
        /// Constructor
        /// </summary>
        public Runner()
        {
            ShowProgressAtConsole = true;
            WorkDir = ".";
        }

        public bool ShowProgressAtConsole { get; set; }
        public string WorkDir { get; set; }

        public static string ConstructParsimonyGroupsFilename(FileSystemInfo fiSourceFile, string baseName)
        {
            return baseName + "_parsimony_groups" + fiSourceFile.Extension;
        }

        public static void GetDefaultOutputFileNames(string inputFilePath, out string parsimonyResultsFilePath, out string proteinGroupMembersFilePath)
        {
            var inputFile = new FileInfo(inputFilePath);
            GetDefaultOutputFileNames(inputFile, out parsimonyResultsFilePath, out proteinGroupMembersFilePath);
        }

        public static void GetDefaultOutputFileNames(FileInfo inputFile, out string parsimonyResultsFilePath, out string proteinGroupMembersFilePath)
        {
            var inputDirectory = inputFile.Directory;
            var baseName = Path.GetFileNameWithoutExtension(inputFile.Name);

            var parsimonyFileName = baseName + "_parsimony" + inputFile.Extension;
            var proteinGroupMembersFileName = ConstructParsimonyGroupsFilename(inputFile, baseName);

            if (inputDirectory == null)
            {
                parsimonyResultsFilePath = parsimonyFileName;
                proteinGroupMembersFilePath = proteinGroupMembersFileName;
            }
            else
            {
                parsimonyResultsFilePath = Path.Combine(inputDirectory.FullName, parsimonyFileName);
                proteinGroupMembersFilePath = Path.Combine(inputDirectory.FullName, proteinGroupMembersFileName);
            }
        }

        /// <summary>
        /// Run the parsimony algorithm against the peptides and proteins in table sourceTableName in the specified SQLite database
        /// </summary>
        /// <param name="databaseFolderPath"></param>
        /// <param name="dataBaseFileName"></param>
        /// <param name="sourceTableName">Table name to process</param>
        /// <returns>True if success; false if an error</returns>
        public bool ProcessSQLite(string databaseFolderPath, string dataBaseFileName, string sourceTableName = DEFAULT_SQLITE_TABLE)
        {
            List<RowEntry> pepToProtMapping;
            List<Node> result;

            var diDataFolder = new DirectoryInfo(databaseFolderPath);
            if (!diDataFolder.Exists)
                throw new DirectoryNotFoundException("Database folder not found: " + databaseFolderPath);

            var fiDatabaseFile = new FileInfo(Path.Combine(diDataFolder.FullName, dataBaseFileName));
            if (!fiDatabaseFile.Exists)
                throw new FileNotFoundException("Database not found: " + fiDatabaseFile);

            if (ShowProgressAtConsole)
                OnStatusEvent("Opening SQLite database " + fiDatabaseFile.FullName);

            if (!VerifySourceTableExists(fiDatabaseFile, sourceTableName))
                return false;

            var dbReader = new SQLiteReader
            {
                Database = fiDatabaseFile.FullName
            };

            try
            {
                var success = GetPeptideProteinMap(dbReader, sourceTableName, out pepToProtMapping);
                if (!success)
                {
                    if (ShowProgressAtConsole)
                    {
                        OnErrorEvent("Error loading data from table {0}; GetPeptideProteinMap returned false", sourceTableName);
                    }
                    return false;
                }

                if (ShowProgressAtConsole)
                    OnStatusEvent("Loaded {0} rows from table {1}", pepToProtMapping.Count, sourceTableName);
            }
            catch (Exception ex)
            {
                throw new Exception("Error calling GetPeptideProteinMap: " + ex.Message, ex);
            }

            if (fiDatabaseFile.DirectoryName == null)
                throw new Exception("Error determining the parent directory for " + fiDatabaseFile.FullName);

            var parsimonyResultsFilePath = Path.Combine(fiDatabaseFile.DirectoryName, "pars_info_temp.txt");
            var proteinGroupMembersFilePath = Path.Combine(fiDatabaseFile.DirectoryName, "pars_info_temp_groups.txt");

            if (pepToProtMapping == null || pepToProtMapping.Count == 0)
            {
                DeleteFile(parsimonyResultsFilePath);
                DeleteFile(proteinGroupMembersFilePath);
                throw new Exception("Error in RunAlgorithm: No rows to operate on");
            }

            GlobalIDContainer globalIDTracker;

            try
            {
                PerformParsimonyThreaded(pepToProtMapping, out result, out globalIDTracker);
            }
            catch (Exception ex)
            {
                throw new Exception("Error calling PerformParsimonyThreaded: " + ex.Message, ex);
            }

            if (ShowProgressAtConsole)
            {
                Console.WriteLine();
                OnStatusEvent("Exporting protein groups to temp text files");
            }

            Utilities.SaveResults(result, parsimonyResultsFilePath, proteinGroupMembersFilePath, globalIDTracker);

            ClearExistingSQLiteResults(fiDatabaseFile);

            if (ShowProgressAtConsole)
                Console.WriteLine();

            try
            {
                var resultsReader = new DelimitedFileReader
                {
                    FilePath = parsimonyResultsFilePath
                };

                var writer = new SQLiteWriter
                {
                    DbPath = fiDatabaseFile.FullName,
                    TableName = PARSIMONY_GROUPING_TABLE
                };

                writer.ColDefOverride = new List<MageColumnDef>
                {
                    new("GroupID", "integer", "4") // Note that "size" doesn't matter since we're writing to a SqLite database
                };

                if (ShowProgressAtConsole)
                    OnStatusEvent("Importing data into table " + PARSIMONY_GROUPING_TABLE);

                ProcessingPipeline.Assemble("ImportToSQLite", resultsReader, writer).RunRoot(null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding data to table " + PARSIMONY_GROUPING_TABLE + " to the SqLite database: " + ex.Message, ex);
            }

            try
            {
                var proteinGroupsReader = new DelimitedFileReader
                {
                    FilePath = proteinGroupMembersFilePath
                };

                var writer = new SQLiteWriter
                {
                    DbPath = fiDatabaseFile.FullName,
                    TableName = PARSIMONY_GROUP_MEMBERS_TABLE
                };

                writer.ColDefOverride = new List<MageColumnDef>
                {
                    new("GroupID", "integer", "4") // Note that "size" doesn't matter since we're writing to a SqLite database
                };

                if (ShowProgressAtConsole)
                    OnStatusEvent("Importing data into table " + PARSIMONY_GROUP_MEMBERS_TABLE);

                ProcessingPipeline.Assemble("ImportToSQLite", proteinGroupsReader, writer).RunRoot(null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding data to table " + PARSIMONY_GROUP_MEMBERS_TABLE + " to the SqLite database: " + ex.Message, ex);
            }

            DeleteFile(parsimonyResultsFilePath);
            DeleteFile(proteinGroupMembersFilePath);

            return true;
        }

        public bool ProcessTextFile(string inputFilePath, string parsimonyResultsFilePath, string proteinGroupMembersFilePath)
        {
            var inputFile = new FileInfo(inputFilePath);
            return ProcessTextFile(inputFile, parsimonyResultsFilePath, proteinGroupMembersFilePath);
        }

        public bool ProcessTextFile(FileInfo inputFile, string parsimonyResultsFilePath, string proteinGroupMembersFilePath)
        {
            List<RowEntry> peptideProteinMapList;
            List<Node> result;
            bool success;

            if (!inputFile.Exists)
                throw new FileNotFoundException("Input file not found: " + inputFile);

            try
            {
                success = Utilities.ReadProteinPeptideTable(inputFile.FullName, out peptideProteinMapList, ShowProgressAtConsole);
                if (!success)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading the " + inputFile.FullName + "file: " + ex.Message);
            }

            if (peptideProteinMapList.Count == 0)
                throw new Exception("Input file is empty");

            GlobalIDContainer globalIDTracker;

            try
            {
                success = PerformParsimonyThreaded(peptideProteinMapList, out result, out globalIDTracker);
            }
            catch (Exception ex)
            {
                throw new Exception("Error calling PerformParsimonyThreaded: " + ex.Message, ex);
            }

            if (success && result != null)
            {
                if (ShowProgressAtConsole)
                {
                    Console.WriteLine();
                    OnStatusEvent("Writing results to " + parsimonyResultsFilePath);
                }

                Utilities.SaveResults(result, parsimonyResultsFilePath, proteinGroupMembersFilePath, globalIDTracker);
            }
            else
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Group proteins having similar peptides
        /// </summary>
        /// <param name="peptideProteinMapList">List of protein to peptide mappings</param>
        /// <param name="clusteredProteins">Parsimonious list of protein</param>
        /// <param name="globalIDTracker"></param>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Global
        public bool PerformParsimony(
            List<RowEntry> peptideProteinMapList,
            out List<Node> clusteredProteins,
            out GlobalIDContainer globalIDTracker)
        {
            mPeptideProteinMapList = peptideProteinMapList;
            mMaxDFSRecursionDepth = DEFAULT_MAX_DFS_RECURSION_DEPTH;

            PerformParsimonyWork();

            clusteredProteins = mClusteredProteins;
            globalIDTracker = mGlobalIDTracker;

            return mProcessingSucceeded;
        }

        /// <summary>
        /// Group proteins having similar peptides
        /// </summary>
        /// <param name="peptideProteinMapList">List of protein to peptide mappings</param>
        /// <param name="clusteredProteins">Parsimonious list of protein</param>
        /// <param name="globalIDTracker"></param>
        /// <returns></returns>
        /// <remarks>Runs on a separate thread which allows for deeper recursion</remarks>
        // ReSharper disable once UnusedMember.Global
        public bool PerformParsimonyThreaded(
            List<RowEntry> peptideProteinMapList,
            out List<Node> clusteredProteins,
            out GlobalIDContainer globalIDTracker)
        {
            mPeptideProteinMapList = peptideProteinMapList;

            // Increase the stack size from 1 MB to 50 MB
            const int stackSizeInBytes = 50 * 1024 * 1024;

            // Since we're using a larger stack size, we can increase the maximum DFS recursion depth
            mMaxDFSRecursionDepth = 20000;

            var thread = new Thread(PerformParsimonyWork, stackSizeInBytes);
            thread.Start();

            var startTime = DateTime.UtcNow;

            while (true)
            {
                // Wait for 2 seconds
                thread.Join(2000);

                // Check whether the thread is still running
                if (!thread.IsAlive)
                    break;

                // Check whether the thread has been running too long
                if (!(DateTime.UtcNow.Subtract(startTime).TotalHours > MAX_RUNTIME_HOURS))
                    continue;

                OnErrorEvent("Parsimony calculations have run for over " + MAX_RUNTIME_HOURS + " hours; aborting");

                try
                {
                    thread.Abort();
                }
                catch
                {
                    // Ignore errors here;
                }

                break;
            }

            clusteredProteins = mClusteredProteins;
            globalIDTracker = mGlobalIDTracker;

            return mProcessingSucceeded;
        }

        /// <summary>
        /// Group proteins having similar peptides
        /// </summary>
        /// <returns></returns>
        private void PerformParsimonyWork()
        {
            mProcessingSucceeded = false;
            mClusteredProteins = new List<Node>();
            mGlobalIDTracker = new GlobalIDContainer();

            // Prepare objects and algorithms
            var nodeBuilder = new NodeBuilder();
            var nodeCollapser = new NodeCollapser();

            var dfs = new DFS {
                MaxRecursionDepth = mMaxDFSRecursionDepth
            };

            var cover = new Cover();

            RegisterEvents(dfs);
            RegisterEvents(cover);

            if (ShowProgressAtConsole)
            {
                Console.WriteLine();
                OnStatusEvent("Finding parsimonious protein groups");
            }

            nodeBuilder.RunAlgorithm(mPeptideProteinMapList, out var proteins, out var peptides);

            if (proteins == null || proteins.Count == 0)
            {
                throw new Exception("Error in PerformParsimony: Protein list is empty");
            }

            if (peptides == null || peptides.Count == 0)
            {
                throw new Exception("Error in PerformParsimony: Peptide list is empty");
            }

            nodeCollapser.RunAlgorithm(proteins, peptides, mGlobalIDTracker);

            if (proteins == null || proteins.Count == 0)
            {
                throw new Exception("Error in PerformParsimony after nodeCollapser.RunAlgorithm: Protein list is empty");
            }

            if (peptides == null || peptides.Count == 0)
            {
                throw new Exception("Error in PerformParsimony after nodeCollapser.RunAlgorithm: Peptide list is empty");
            }

            var proteinsWithChildren = proteins.Values.ToList();

            var clusteredProteinSets = dfs.RunAlgorithm(proteinsWithChildren);

            if (clusteredProteinSets == null || clusteredProteinSets.Count == 0)
            {
                throw new Exception("Error in PerformParsimony: DFS returned an empty protein list");
            }

            mClusteredProteins = cover.RunAlgorithm(clusteredProteinSets);

            if (mClusteredProteins == null || mClusteredProteins.Count == 0)
            {
                throw new Exception("Error in PerformParsimony: cover.RunAlgorithm returned an empty protein list");
            }

            if (ShowProgressAtConsole)
                OnStatusEvent("Iteration Complete, found {0} protein groups", mClusteredProteins.Count);

            mProcessingSucceeded = true;
        }

        /// <summary>
        /// Make sure the parsimony tables in the SQLite database are empty
        /// </summary>
        /// <param name="fiDatabaseFile"></param>
        private void ClearExistingSQLiteResults(FileSystemInfo fiDatabaseFile)
        {
            try
            {
                var connectionString = "Data Source = " + fiDatabaseFile.FullName + "; Version=3;";

                using var dbConnection = new SQLiteConnection(connectionString, true);

                dbConnection.Open();

                var tablesToTruncate = new List<string> {
                    PARSIMONY_GROUPING_TABLE,
                    PARSIMONY_GROUP_MEMBERS_TABLE
                };

                var emptyLineAdded = false;
                foreach (var tableName in tablesToTruncate)
                {
                    if (!SQLiteTableExists(dbConnection, tableName))
                        continue;

                    using var dbCommand = dbConnection.CreateCommand();

                    if (ShowProgressAtConsole)
                    {
                        if (!emptyLineAdded)
                        {
                            emptyLineAdded = true;
                            Console.WriteLine();
                        }

                        OnStatusEvent("Deleting existing data in table {0}", tableName);
                    }

                    dbCommand.CommandText = string.Format("DELETE FROM {0};", tableName);
                    dbCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                OnWarningEvent("Error deleting existing protein parsimony data from the SQLite database: " + ex.Message);
            }
        }

        private void DeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
                // Ignore errors here
            }
        }

        private bool GetPeptideProteinMap(SQLiteReader reader, string tableName, out List<RowEntry> pepToProtMapping)
        {
            reader.SQLText = "SELECT * FROM [" + tableName + "]";

            // Make a Mage sink Module (row buffer
            var sink = new SimpleSink();

            // Construct and run mage pipeline to get the peptide and protein info
            ProcessingPipeline.Assemble("Test_Pipeline", reader, sink).RunRoot(null);

            var proteinIdx = sink.ColumnIndex["Protein"];
            var peptideIdx = sink.ColumnIndex["Peptide"];
            pepToProtMapping = new List<RowEntry>();
            foreach (object[] row in sink.Rows)
            {
                var entry = new RowEntry
                {
                    ProteinEntry = (string)row[proteinIdx],
                    PeptideEntry = (string)row[peptideIdx]
                };
                pepToProtMapping.Add(entry);
            }
            if (pepToProtMapping.Count == 0)
            {
                throw new Exception("Error reading data from " + tableName + "; no results found using " + reader.SQLText);
            }

            return true;
        }

        private bool SQLiteTableExists(SQLiteConnection dbConnection, string tableName)
        {
            using var cmd = new SQLiteCommand(dbConnection)
            {
                CommandText = "SELECT name " +
                              "FROM sqlite_master " +
                              "WHERE type IN ('table','view') And tbl_name = '" + tableName + "'"
            };

            using var reader = cmd.ExecuteReader();

            return reader.HasRows;
        }

        /// <summary>
        /// Assure that table sourceTableName exists in the SQLite database
        /// </summary>
        /// <param name="fiDatabaseFile"></param>
        /// <param name="sourceTableName"></param>
        /// <returns></returns>
        private bool VerifySourceTableExists(FileSystemInfo fiDatabaseFile, string sourceTableName)
        {
            try
            {
                var connectionString = "Data Source = " + fiDatabaseFile.FullName + "; Version=3;";

                using var dbConnection = new SQLiteConnection(connectionString, true);

                dbConnection.Open();

                if (SQLiteTableExists(dbConnection, sourceTableName))
                    return true;

                OnWarningEvent("Source table {0} not found in SQLite file\n{1}", sourceTableName, fiDatabaseFile.FullName);

                return false;
            }
            catch (Exception ex)
            {
                OnWarningEvent("Error verifying that table {0} exists in the SQLite database: {1}", sourceTableName, ex.Message);
                return false;
            }
        }
    }
}
