using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Mage;
using PRISM;
using SetCover.Algorithms;
using SetCover.Objects;

namespace SetCover
{
    public class Runner
    {

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
        public event ProgressChangedHandler ProgressChanged;
        public delegate void ProgressChangedHandler(Runner runner, ProgressInfo e);

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
        /// Run the parsimony algorithm against the peptides and proteins in table T_Row_Metadata in the specified file
        /// </summary>
        /// <param name="databaseFolderPath"></param>
        /// <param name="dataBaseFileName"></param>
        /// <returns>True if success; false if an error</returns>
        public bool RunAlgorithm(string databaseFolderPath, string dataBaseFileName)
        {
            const string SOURCE_TABLE = "T_Row_Metadata";

            return RunAlgorithm(databaseFolderPath, dataBaseFileName, SOURCE_TABLE);
        }

        /// <summary>
        /// Run the parsimony algorithm against the peptides and proteins in table sourceTableName in the specified file
        /// </summary>
        /// <param name="databaseFolderPath"></param>
        /// <param name="dataBaseFileName"></param>
        /// <param name="sourceTableName">Table name to process</param>
        /// <returns>True if success; false if an error</returns>
        public bool RunAlgorithm(string databaseFolderPath, string dataBaseFileName, string sourceTableName)
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
                Console.WriteLine("Opening SQLite database " + fiDatabaseFile.FullName);

            var reader = new SQLiteReader
            {
                Database = fiDatabaseFile.FullName
            };

            try
            {
                var success = GetPeptideProteinMap(reader, sourceTableName, out pepToProtMapping);
                if (!success)
                {
                    if (ShowProgressAtConsole)
                    {
                        ConsoleMsgUtils.ShowError(
                            string.Format("Error loading data from table {0}; GetPeptideProteinMap returned false", sourceTableName));
                    }
                    return false;
                }

                if (ShowProgressAtConsole)
                    Console.WriteLine("Loaded {0} rows from table {1}", pepToProtMapping.Count, sourceTableName);
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
                PerformParsimony(pepToProtMapping, out result, out globalIDTracker);
            }
            catch (Exception ex)
            {
                throw new Exception("Error calling PerformParsimony: " + ex.Message, ex);
            }

            if (ShowProgressAtConsole)
                Console.WriteLine("Exporting protein groups to temp text files");

            Utilities.SaveResults(result, parsimonyResultsFilePath, proteinGroupMembersFilePath, globalIDTracker);

            const string PARSIMONY_GROUPING_TABLE = "T_Parsimony_Grouping";
            const string PARSIMONY_GROUP_MEMBERS_TABLE = "T_Parsimony_Group_Members";

            try
            {
                // Make sure the parsimony tables in the SQLite database are empty
                var connectionString = "Data Source = " + fiDatabaseFile.FullName + "; Version=3;";
                using (var dbConnection = new SQLiteConnection(connectionString, true))
                {
                    dbConnection.Open();

                    var tablesToTruncate = new List<string> {
                        PARSIMONY_GROUPING_TABLE,
                        PARSIMONY_GROUP_MEMBERS_TABLE
                    };

                    foreach (var tableName in tablesToTruncate)
                    {
                        if (!SQLiteTableExists(dbConnection, tableName))
                            continue;

                        using (var dbCommand = dbConnection.CreateCommand())
                        {
                            if (ShowProgressAtConsole)
                                Console.WriteLine("Deleting existing data in table {0}", tableName);

                            dbCommand.CommandText = string.Format("DELETE FROM {0};", tableName);
                            dbCommand.ExecuteNonQuery();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                ConsoleMsgUtils.ShowWarning("Error deleting existing protein parsimony data from the SQLite database: " + ex.Message);
            }

            try
            {

                var delimreader = new DelimitedFileReader
                {
                    FilePath = parsimonyResultsFilePath
                };

                var writer = new SQLiteWriter
                {
                    DbPath = fiDatabaseFile.FullName,
                    TableName = PARSIMONY_GROUPING_TABLE
                };

                var colDefs = new List<MageColumnDef>
                {
                    new MageColumnDef("GroupID", "integer", "4")		// Note that "size" doesn't matter since we're writing to a SqLite database
                };
                writer.ColDefOverride = colDefs;

                if (ShowProgressAtConsole)
                    Console.WriteLine("Importing data into table " + PARSIMONY_GROUPING_TABLE);

                ProcessingPipeline.Assemble("ImportToSQLite", delimreader, writer).RunRoot(null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding data to table " + PARSIMONY_GROUPING_TABLE + " to the SqLite database: " + ex.Message, ex);
            }

            try
            {
                var delimreader = new DelimitedFileReader
                {
                    FilePath = proteinGroupMembersFilePath
                };

                var writer = new SQLiteWriter
                {
                    DbPath = fiDatabaseFile.FullName,
                    TableName = PARSIMONY_GROUP_MEMBERS_TABLE
                };

                var colDefs = new List<MageColumnDef>
                {
                    new MageColumnDef("GroupID", "integer", "4")		// Note that "size" doesn't matter since we're writing to a SqLite database
                };
                writer.ColDefOverride = colDefs;

                if (ShowProgressAtConsole)
                    Console.WriteLine("Importing data into table " + PARSIMONY_GROUP_MEMBERS_TABLE);

                ProcessingPipeline.Assemble("ImportToSQLite", delimreader, writer).RunRoot(null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding data to table " + PARSIMONY_GROUP_MEMBERS_TABLE + " to the SqLite database: " + ex.Message, ex);
            }

            DeleteFile(parsimonyResultsFilePath);
            DeleteFile(proteinGroupMembersFilePath);

            return true;
        }

        public bool RunGUIAlgorithm(string inputFilePath, string parsimonyResultsFilePath, string proteinGroupMembersFilePath)
        {
            var inputFile = new FileInfo(inputFilePath);
            return RunGUIAlgorithm(inputFile, parsimonyResultsFilePath, proteinGroupMembersFilePath);
        }

        public bool RunGUIAlgorithm(FileInfo inputFile, string parsimonyResultsFilePath, string proteinGroupMembersFilePath)
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
                success = PerformParsimony(peptideProteinMapList, out result, out globalIDTracker);
            }
            catch (Exception ex)
            {
                throw new Exception("Error calling PerformParsimony: " + ex.Message, ex);
            }

            if (success && result != null)
            {
                if (ShowProgressAtConsole)
                    Console.WriteLine("Writing results to " + parsimonyResultsFilePath);

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
        /// <param name="clusteredProteins">Parimonious list of protein</param>
        /// <param name="globalIDTracker"></param>
        /// <returns></returns>
        public bool PerformParsimony(List<RowEntry> peptideProteinMapList, out List<Node> clusteredProteins, out GlobalIDContainer globalIDTracker)
        {
            // Prepare objects and algorithms
            var nodebuilder = new NodeBuilder();
            var nodecollapser = new NodeCollapser();
            var dfs = new DFS();
            var cover = new Cover();

            if (ShowProgressAtConsole)
                Console.WriteLine("Finding parsimonious protein groups");

            nodebuilder.RunAlgorithm(peptideProteinMapList, out var proteins, out var peptides);

            if (proteins == null || proteins.Count == 0)
            {
                throw new Exception("Error in PerformParsimony: Protein list is empty");
            }

            if (peptides == null || peptides.Count == 0)
            {
                throw new Exception("Error in PerformParsimony: Peptide list is empty");
            }

            globalIDTracker = new GlobalIDContainer();
            nodecollapser.RunAlgorithm(proteins, peptides, globalIDTracker);

            if (proteins == null || proteins.Count == 0)
            {
                throw new Exception("Error in PerformParsimony after NodeCollapser: Protein list is empty");
            }

            if (peptides == null || peptides.Count == 0)
            {
                throw new Exception("Error in PerformParsimony after NodeCollapser: Peptide list is empty");
            }

            var proteinsWithChildren = proteins.Values.ToList();

            var clusteredProteinSets = dfs.RunAlgorithm(proteinsWithChildren);

            if (clusteredProteinSets == null || clusteredProteinSets.Count == 0)
            {
                throw new Exception("Error in PerformParsimony: DFS returned an empty protein list");
            }

            clusteredProteins = cover.RunAlgorithm(clusteredProteinSets);

            if (clusteredProteins == null || clusteredProteins.Count == 0)
            {
                throw new Exception("Error in PerformParsimony: Cover returned an empty protein list");
            }

            if (ShowProgressAtConsole)
                Console.WriteLine("Iteration Complete, found {0} protein groups", clusteredProteins.Count);

            return true;

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

        protected void OnProgressChanged(float progressOverall, float progressCurrentFile)
        {

            if (ProgressChanged != null)
            {
                var e = new ProgressInfo
                {
                    Value = progressOverall,
                    ProgressCurrentJob = progressCurrentFile
                };

                ProgressChanged(this, e);
            }
        }

        private bool SQLiteTableExists(SQLiteConnection dbConnection, string tableName)
        {
            bool hasRows;

            using (var cmd = new SQLiteCommand(dbConnection)
            {
                CommandText = "SELECT name " +
                              "FROM sqlite_master " +
                              "WHERE type IN ('table','view') And tbl_name = '" + tableName + "'"
            })
            {
                using (var reader = cmd.ExecuteReader())
                {
                    hasRows = reader.HasRows;
                }
            }

            return hasRows;
        }


    }

    public class ProgressInfo : EventArgs
    {
        public float Value { get; set; }
        public float ProgressCurrentJob { get; set; }
    }
}
