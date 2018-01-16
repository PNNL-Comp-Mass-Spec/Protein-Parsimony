using System;
using System.IO;
using SetCover;

namespace ProteinParsimony
{
    class Program
    {
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
                fiSourceFile = new FileInfo(args[0]);
                if (!fiSourceFile.Exists)
                {
                    ShowSyntax("The file: " + args[0] + ", does not exist");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                ShowSyntax("Exception validating the input file path: " + ex.Message);
                return -2;

            }

            string parsimonyResultsFilePath;
            string proteinGroupMembersFilePath;

            try
            {


                if (args.Length < 2)
                {

                    parsimonyResultsFilePath = Path.Combine(fiSourceFile.Directory.FullName,
                                                         Path.GetFileNameWithoutExtension(fiSourceFile.Name) + "_parsimony" +
                                                         fiSourceFile.Extension);

                    proteinGroupMembersFilePath = Path.Combine(fiSourceFile.Directory.FullName,
                                                               Path.GetFileNameWithoutExtension(fiSourceFile.Name) +
                                                               "_parsimony_groups" + fiSourceFile.Extension);

                }
                else
                {
                    parsimonyResultsFilePath = args[1];

                    var fiOutputfile = new FileInfo(parsimonyResultsFilePath);
                    proteinGroupMembersFilePath = Path.Combine(fiOutputfile.Directory.FullName,
                              Path.GetFileNameWithoutExtension(fiOutputfile.Name) + "_groups" + fiOutputfile.Extension);

                }

                var fiOutputFile = new FileInfo(parsimonyResultsFilePath);
                if (!fiOutputFile.Directory.Exists)
                    fiOutputFile.Directory.Create();
            }
            catch (Exception ex)
            {
                ShowSyntax("Exception validating the output file path: " + ex.Message);
                return -2;

            }

            try
            {
                var runner = new Runner
                {
                    ShowProgressAtConsole = true
                };

                runner.ProgressChanged += RunnerProgressHandler;
                var success = runner.RunGUIAlgorithm(fiSourceFile.FullName, parsimonyResultsFilePath, proteinGroupMembersFilePath);

                if (success)
                    Console.WriteLine("Success");
                else
                {
                    Console.WriteLine("Failed");
                    return -3;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error computing protein parsimony: " + ex.Message);
                return -4;
            }

            return 0;
        }

        private static void RunnerProgressHandler(Runner id, ProgressInfo e)
        {
            Console.WriteLine(e.ProgressCurrentJob.ToString("0.0") + "% complete; " + e.Value.ToString("0.0") + "% complete overall");
        }

        private static void ShowSyntax()
        {
            ShowSyntax(string.Empty);
        }

        private static void ShowSyntax(string errorMessage)
        {
            Console.WriteLine();
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Console.WriteLine("Error: " + errorMessage);
                Console.WriteLine();
            }

            Console.WriteLine("Program Syntax:");
            Console.WriteLine(Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location) + " InputFilePath [OutputFilePath]");
            Console.WriteLine();
            Console.WriteLine("If the output file path is not defined, then the output file ");
            Console.WriteLine("will be created in the same location as the input file, ");
            Console.WriteLine("but with '_parsimony' added to the filename.");
        }

    }
}
