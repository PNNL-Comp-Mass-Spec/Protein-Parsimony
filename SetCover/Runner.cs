using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mage;


namespace SetCover
{
    public class Runner 
    {

            
   		public Runner()
		{
			ShowProgressAtConsole = true;
			WorkDir = ".";
		}
        
        public bool ShowProgressAtConsole { get; set; }
        public string WorkDir { get; set; }
        public event ProgressChangedHandler ProgressChanged;
        public delegate void ProgressChangedHandler(Runner runner, ProgressInfo e);

        public bool RunAlgorithm(string databaseFolderPath, string dataBaseFileName)
        {
            List<RowEntry> lstTrowMetadata;
            List<Node> result;
            bool success;

            var diDataFolder = new DirectoryInfo(databaseFolderPath);
            if(!diDataFolder.Exists)
                throw new DirectoryNotFoundException("Database folder not found: " + databaseFolderPath);
            
            var fiDatabaseFile = new FileInfo(Path.Combine(diDataFolder.FullName, dataBaseFileName));
            if(!fiDatabaseFile.Exists)
                throw new FileNotFoundException("Database not found: " + fiDatabaseFile);


            var reader = new SQLiteReader
            {
                Database = fiDatabaseFile.FullName
            };

            try
            {
                success = GetPeptideProteinMap(reader, out lstTrowMetadata);
                if (!success)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error calling GetPeptideProteinMap: " + ex.Message, ex);
            }


            System.Diagnostics.Debug.Assert(fiDatabaseFile.DirectoryName != null,
                  "fiDatabaseFile.DirectoryName != null");
            string tempParsFilePath = Path.Combine(fiDatabaseFile.DirectoryName,
                "pars_info_temp.txt");

            if (lstTrowMetadata == null || lstTrowMetadata.Count == 0)
            {
                DeleteFile(tempParsFilePath);
                throw new Exception("Error in PerformParsimony: No rows to operate on");
            }

            try
            {
                PerformParsimony(lstTrowMetadata, out result);
            }
            catch (Exception ex)
            {
                throw new Exception("Error calling PerformParsimony: " + ex.Message, ex);
            }

            Utilities.WriteTable(result, tempParsFilePath);

            try
            {
                var delimreader = new DelimitedFileReader
                {
                    FilePath = tempParsFilePath
                };

                var writer = new SQLiteWriter();
                const string tableName = "T_Parsimony_Grouping";
                writer.DbPath = fiDatabaseFile.FullName;
                writer.TableName = tableName;

                ProcessingPipeline.Assemble("ImportToSQLite", delimreader, writer).RunRoot(null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding table T_Parsimony_Grouping to the SqLite database: " + ex.Message, ex);
            }
            DeleteFile(tempParsFilePath);
            return true;
        }

        public bool RunGUIAlgorithm(string inputFile, string outputFile)
        {
            List<RowEntry> lstTrowMetadata;
            List<Node> result = null;
            bool success;


            var fiInputFile = new FileInfo(inputFile);
            if (!fiInputFile.Exists)
                throw new FileNotFoundException("Input file not found: " + fiInputFile);
            
            try
            {
                success = Utilities.ReadTable(fiInputFile.FullName, out lstTrowMetadata);
                if (!success)
                    return false;
            }
            catch(Exception ex)
            {
                throw new Exception("Error loading the " +fiInputFile.FullName + "file: " + ex.Message);
            }

	        if (lstTrowMetadata.Count == 0)
		        throw new Exception("Input file is empty");

            try
            {
                success = success && PerformParsimony(lstTrowMetadata, out result);
            }
            catch (Exception ex)
            {
                throw new Exception("Error calling PerformParsimony: " + ex.Message, ex);
            }

            if (success && result != null)
            {
                Utilities.WriteTable(result, outputFile);
            }
            else
            {
                return false;
            }

            return true;
        }


        public bool PerformParsimony(List<RowEntry> toParsimonize, out List<Node> ClProteins)
        {
            //prepare objects and algorithms
            Dictionary<string, Node> Peptides;
            Dictionary<string, Node> Proteins;
            var nodebuilder = new NodeBuilder();
            var nodecollapser = new NodeCollapser();
            var dfs = new DFS();
            var cover = new Cover();

 //           var dt = Utilities.TextFileToDataTableAssignTypeString(filename, false);
            
            nodebuilder.RunAlgorithm(toParsimonize, out Proteins, out Peptides);
			if (Proteins == null || Proteins.Count == 0)
            {
				throw new Exception("Error in PerformParsimony: Protein list is empty");
            }

			if (Peptides == null || Peptides.Count == 0)
			{
				throw new Exception("Error in PerformParsimony: Peptide list is empty");
			}		

            nodecollapser.RunAlgorithm(ref Proteins, ref Peptides);

			if (Proteins == null || Proteins.Count == 0)
			{
				throw new Exception("Error in PerformParsimony after NodeCollapser: Protein list is empty");
			}

			if (Peptides == null || Peptides.Count == 0)
			{
				throw new Exception("Error in PerformParsimony after NodeCollapser: Peptide list is empty");
			}		

            ClProteins = Proteins.Values.ToList();

            dfs.RunAlgorithm(ref ClProteins);

            if (ClProteins == null || ClProteins.Count == 0)
            {
                throw new Exception("Error in PerformParsimony: DFS returned an empty protein list");
            }

            cover.RunAlgorithm(ref ClProteins);

            if (ClProteins == null || ClProteins.Count == 0)
            {
                throw new Exception("Error in PerformParsimony: Cover returned an empty protein list");
            }


            if (ShowProgressAtConsole)
                Console.WriteLine("Iteration Complete");

            return true;

        }

        private bool GetPeptideProteinMap(SQLiteReader reader, out List<RowEntry> pepToProtMapping)
        {
            reader.SQLText = "SELECT * FROM T_Row_Metadata";

            //Make a Mage sink Module (row buffer
            var sink = new SimpleSink();

            //construct and run mage pipeline to get the peptide and protein info
            ProcessingPipeline.Assemble("Test_Pipeline", reader, sink).RunRoot(null);

            int proteinIdx = sink.ColumnIndex["Protein"];
            int peptideIdx = sink.ColumnIndex["Peptide"];
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
                throw new Exception("Error in getting T_Row_Metadata rows, no results found using " + reader.SQLText);
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
    }




    public class ProgressInfo : EventArgs
    {
        public float Value { get; set; }
        public float ProgressCurrentJob { get; set; }
    }
}
