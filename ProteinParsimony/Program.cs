using System;
using System.Collections.Generic;
using System.IO;
using PRISM;
using PRISM.FileProcessor;
using SetCover;

namespace ProteinParsimony
{
    public static class Program
    {
        private const string PROGRAM_DATE = "April 22, 2022";

        private static DateTime mLastProgressUpdateTime = DateTime.UtcNow;

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
                    ProgRunner.SleepMilliseconds(1500);
                    return -1;
                }
            }
            catch (Exception ex)
            {
                ShowSyntax("Exception validating the input file path: " + ex.Message);
                ProgRunner.SleepMilliseconds(1500);
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
                var result = ProcessTextFile(args, fiSourceFile);
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
                RegisterEvents(runner);

                var success = runner.ProcessSQLite(fiSourceFile.DirectoryName, fiSourceFile.Name, sourceTableName);

                if (success)
                {
                    Console.WriteLine();
                    Console.WriteLine("Processing complete");
                    ProgRunner.SleepMilliseconds(750);
                    return 0;
                }

                ConsoleMsgUtils.ShowWarning("Error computing protein parsimony: RunAlgorithm reports false");
                ProgRunner.SleepMilliseconds(1500);
                return -3;
            }
            catch (Exception ex)
            {
                ConsoleMsgUtils.ShowError("Error computing protein parsimony", ex);
                ProgRunner.SleepMilliseconds(1500);
                return -2;
            }
        }

        private static int ProcessTextFile(IReadOnlyList<string> args, FileInfo fiSourceFile)
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

                var outputFile = new FileInfo(parsimonyResultsFilePath);

                // ReSharper disable once MergeIntoPattern
                if (outputFile.Directory != null && !outputFile.Directory.Exists)
                    outputFile.Directory.Create();
            }
            catch (Exception ex)
            {
                ShowSyntax("Exception validating the output file path: " + ex.Message);
                ProgRunner.SleepMilliseconds(1500);
                return -2;
            }

            try
            {
                var runner = new Runner
                {
                    ShowProgressAtConsole = true
                };
                RegisterEvents(runner);

                var success = runner.ProcessTextFile(fiSourceFile, parsimonyResultsFilePath, proteinGroupMembersFilePath);

                if (success)
                {
                    Console.WriteLine();
                    Console.WriteLine("Processing Complete");
                    ProgRunner.SleepMilliseconds(750);
                    return 0;
                }

                ConsoleMsgUtils.ShowWarning("Error computing protein parsimony: ProcessTextFile reports false");
                ProgRunner.SleepMilliseconds(1500);
                return -3;
            }
            catch (Exception ex)
            {
                ConsoleMsgUtils.ShowError("Error computing protein parsimony", ex);
                ProgRunner.SleepMilliseconds(1500);
                return -4;
            }
        }

        private static void ShowErrorMessage(string message, Exception ex = null)
        {
            ConsoleMsgUtils.ShowError(message, ex);
        }

        private static void ShowSyntax(string errorMessage = "")
        {
            Console.WriteLine();
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                ConsoleMsgUtils.ShowErrorCustom("Error: " + errorMessage, false);
                Console.WriteLine();
            }

            var exeName = Path.GetFileName(ProcessFilesOrDirectoriesBase.GetAppPath());

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
            Console.WriteLine("Program written by Josh Aldrich for the Department of Energy (PNNL, Richland, WA)");
            Console.WriteLine("Version: " + ProcessFilesOrDirectoriesBase.GetAppVersion(PROGRAM_DATE));
            Console.WriteLine();
            Console.WriteLine("E-mail:  proteomics@pnnl.gov");
            Console.WriteLine("Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://panomics.pnnl.gov/ or https://www.pnnl.gov/integrative-omics or ");
            Console.WriteLine("         https://github.com/PNNL-Comp-Mass-Spec");
        }

        private static void ConsoleWriteWrapped(string textToWrap)
        {
            Console.WriteLine(ConsoleMsgUtils.WrapParagraph(textToWrap));
        }

        private static void RegisterEvents(EventNotifier processingClass)
        {
            processingClass.DebugEvent += OnDebugEvent;
            processingClass.StatusEvent += OnStatusEvent;
            processingClass.ErrorEvent += OnErrorEvent;
            processingClass.WarningEvent += OnWarningEvent;
            processingClass.ProgressUpdate += OnProgressUpdate;
        }

        private static void OnDebugEvent(string message)
        {
            ConsoleMsgUtils.ShowDebug(message);
        }

        private static void OnStatusEvent(string message)
        {
            Console.WriteLine(message);
        }

        private static void OnErrorEvent(string message, Exception ex)
        {
            ShowErrorMessage(message, ex);
        }

        private static void OnWarningEvent(string message)
        {
            ConsoleMsgUtils.ShowWarning(message);
        }

        private static void OnProgressUpdate(string progressMessage, float percentComplete)
        {
            if (DateTime.UtcNow.Subtract(mLastProgressUpdateTime).TotalSeconds >= 10)
            {
                Console.WriteLine("{0}% complete: {1} ", percentComplete, progressMessage);
                mLastProgressUpdateTime = DateTime.UtcNow;
            }
        }
    }
}
