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
        private const string PROGRAM_DATE = "January 18, 2018";

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
                var sourceTableName = args.Length > 1 ? args[1] : Runner.DEFAULT_SQLITE_TABLE;
                var result = ProcessSQLiteDB(fiSourceFile, sourceTableName);
                return result;
            }
            else
            {
                var result = ProcessTextfile(args, fiSourceFile);
                return result;
            }

        }

        private static int ProcessSQLiteDB(FileInfo fiSourceFile, string sourceTableName)
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
                    Console.WriteLine();
                    Console.WriteLine("Processing complete");
                    clsProgRunner.SleepMilliseconds(750);
                    return 0;
                }

                ConsoleMsgUtils.ShowWarning("Error computing protein parsimony: RunAlgorithm reports false");
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
                    Console.WriteLine();
                    Console.WriteLine("Processing Complete");
                    clsProgRunner.SleepMilliseconds(750);
                    return 0;
                }

                ConsoleMsgUtils.ShowWarning("Error computing protein parsimony: ProcessTextFile reports false");
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

            ConsoleWriteWrapped("This program implements a protein parsimony algorithm for grouping proteins with similar peptides.");
            Console.WriteLine();
            Console.WriteLine("Program syntax #1:");
            Console.WriteLine(exeName + " InputFilePath.txt [OutputFilePath]");
            Console.WriteLine();
            ConsoleWriteWrapped("The input file is a tab delimited text file with columns Protein and Peptide " +
                                "(column order does not matter; extra columns are ignored)");
            Console.WriteLine();
            ConsoleWriteWrapped("If the output file path is not defined, it will be created in the same location " +
                                "as the input file, but with '_parsimony' added to the filename");
            Console.WriteLine();
            Console.WriteLine("Program syntax #2:");
            Console.WriteLine(exeName + " SQLiteDatabase.db3 [TableName]");
            Console.WriteLine();
            ConsoleWriteWrapped("If the input is a SQLite database file (extension .db, .db3, .sqlite, or\a.sqlite3), " +
                                "proteins and peptides will be read from the specified table, or\afrom " +
                                "T_Row_Metadata if TableName is not provided. The table must have columns " +
                                "Protein and Peptide. Results will be written to tables " +
                                Runner.PARSIMONY_GROUPING_TABLE + " and " + Runner.PARSIMONY_GROUP_MEMBERS_TABLE);
            Console.WriteLine();
            Console.WriteLine("Program written by Josh Aldrich for the Department of Energy (PNNL, Richland, WA) in 2013");
            Console.WriteLine("Version: " + ProcessFilesOrFoldersBase.GetAppVersion(PROGRAM_DATE));
            Console.WriteLine();
            Console.WriteLine("E-mail:  proteomics@pnnl.gov");
            Console.WriteLine("Website: https://omics.pnl.gov/ or https://panomics.pnnl.gov/ or ");
            Console.WriteLine("         https://github.com/PNNL-Comp-Mass-Spec");
        }

        private static void ConsoleWriteWrapped(string textToWrap)
        {
            Console.WriteLine(PRISM.CommandLineParser<Program>.WrapParagraph(textToWrap));
        }
    }
}
