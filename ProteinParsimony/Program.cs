using System;
using System.Collections.Generic;
using System.IO;
using PRISM;
using PRISM.FileProcessor;
using SetCover;

namespace ProteinParsimony
{
    class Program
    {
        private const string PROGRAM_DATE = "January 17, 2018";

        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                ShowSyntax();
                return 0;
            }

            FileInfo fiSourceFile;

            try
            {
                var inputFilePath = args[0];

                fiSourceFile = new FileInfo(args[0]);
                if (!fiSourceFile.Exists)
                {
                    ShowSyntax("Input file not found: " + inputFilePath);
                    clsProgRunner.SleepMilliseconds(1500);
                    return -1;
                }
            }
            catch (Exception ex)
            {
                ShowSyntax("Exception validating the input file path: " + ex.Message);
                clsProgRunner.SleepMilliseconds(1500);
                return -2;

            }

            var sqliteExtensions = new SortedSet<string>(StringComparer.OrdinalIgnoreCase) {
                ".db",
                ".db3",
                ".sqlite",
                ".sqlite3"
            };

            if (sqliteExtensions.Contains(fiSourceFile.Extension))
            {
                var result = ProcessSQLiteDB(fiSourceFile);
                return result;
            }
            else
            {
                var result = ProcessTextfile(args, fiSourceFile);
                return result;
            }

        }

        private static int ProcessSQLiteDB(FileInfo fiSourceFile)
        {

            try
            {
                var runner = new Runner
                {
                    ShowProgressAtConsole = true
                };

                runner.ProgressChanged += RunnerProgressHandler;
                var success = runner.ProcessSQLite(fiSourceFile.DirectoryName, fiSourceFile.Name, sourceTableName);

                if (success)
                {
                    Console.WriteLine("Success");
                    clsProgRunner.SleepMilliseconds(750);
                    return 0;
                }

                ConsoleMsgUtils.ShowError("Error computing protein parsimony: RunAlgorithm reports false");
                clsProgRunner.SleepMilliseconds(1500);
                return -3;
            }
            catch (Exception ex)
            {
                ConsoleMsgUtils.ShowError("Error computing protein parsimony: " + ex.Message, ex);

                clsProgRunner.SleepMilliseconds(1500);
                return -2;

            }
        }

        private static int ProcessTextfile(IReadOnlyList<string> args, FileInfo fiSourceFile)
        {

            string parsimonyResultsFilePath;
            string proteinGroupMembersFilePath;

            try
            {
                if (args.Count < 2)
                {
                    Runner.GetDefaultOutputFileNames(fiSourceFile, out parsimonyResultsFilePath, out proteinGroupMembersFilePath);
                }
                else
                {
                    Runner.GetDefaultOutputFileNames(fiSourceFile, out _, out proteinGroupMembersFilePath);
                    parsimonyResultsFilePath = args[1];
                }

                var fiOutputFile = new FileInfo(parsimonyResultsFilePath);
                if (fiOutputFile.Directory != null && !fiOutputFile.Directory.Exists)
                    fiOutputFile.Directory.Create();
            }
            catch (Exception ex)
            {
                ShowSyntax("Exception validating the output file path: " + ex.Message);
                clsProgRunner.SleepMilliseconds(1500);
                return -2;
            }

            try
            {
                var runner = new Runner
                {
                    ShowProgressAtConsole = true
                };

                runner.ProgressChanged += RunnerProgressHandler;
                var success = runner.ProcessTextFile(fiSourceFile, parsimonyResultsFilePath, proteinGroupMembersFilePath);

                if (success)
                {
                    Console.WriteLine("Success");
                    clsProgRunner.SleepMilliseconds(750);
                    return 0;
                }

                ConsoleMsgUtils.ShowError("Error computing protein parsimony: RunGUIAlgorithm reports false");
                clsProgRunner.SleepMilliseconds(1500);
                return -3;
            }
            catch (Exception ex)
            {
                ConsoleMsgUtils.ShowError("Error computing protein parsimony: " + ex.Message, ex);
                clsProgRunner.SleepMilliseconds(1500);
                return -4;
            }
        }

        private static void RunnerProgressHandler(Runner id, ProgressInfo e)
        {
            Console.WriteLine(e.ProgressCurrentJob.ToString("0.0") + "% complete; " + e.Value.ToString("0.0") + "% complete overall");
        }

        private static void ShowSyntax(string errorMessage = "")
        {
            Console.WriteLine();
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                ConsoleMsgUtils.ShowError("Error: " + errorMessage, false);
                Console.WriteLine();
            }

            var exeName = Path.GetFileName(ProcessFilesOrFoldersBase.GetAppPath());

            Console.WriteLine("This program implements a protein parsimony algorithm");
            Console.WriteLine("for grouping proteins with similar peptides.");
            Console.WriteLine();
            Console.WriteLine("Program Syntax:");
            Console.WriteLine(exeName + " InputFilePath.txt [OutputFilePath]");
            Console.WriteLine(" or ");
            Console.WriteLine(exeName + " SQLiteDatabase.db3");
            Console.WriteLine();
            Console.WriteLine("The input file is a tab delimited text file with columns Protein and Peptide");
            Console.WriteLine("(column order does not matter; extra columns are ignored)");
            Console.WriteLine();
            Console.WriteLine("If the output file path is not defined, it will be created in the same location");
            Console.WriteLine("as the input file, but with '_parsimony' added to the filename");
            Console.WriteLine();
            Console.WriteLine("Alternatively, the input file can be a SQLite database file (extension .db, .db3, .sqlite, or .sqlite3)");
            Console.WriteLine("Proteins and peptides will be read from table T_Row_Metadata and results will be");
            Console.WriteLine("written to tables T_Row_Metadata_parsimony and T_Row_Metadata_parsimony_groups");
            Console.WriteLine();
            Console.WriteLine("Program written by Josh Aldrich for the Department of Energy (PNNL, Richland, WA) in 2013");
            Console.WriteLine("Version: " + ProcessFilesOrFoldersBase.GetAppVersion(PROGRAM_DATE));
            Console.WriteLine();
            Console.WriteLine("E-mail: proteomics@pnnl.gov");
            Console.WriteLine("Website: https://omics.pnl.gov/ or https://panomics.pnnl.gov/ or https://github.com/PNNL-Comp-Mass-Spec");
        }

    }
}
