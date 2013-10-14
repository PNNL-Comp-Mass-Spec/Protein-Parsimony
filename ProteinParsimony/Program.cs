using System;
using System.IO;
using SetCover;

namespace ProteinParsimony
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                throw new ArgumentException("Must have exactly 2 arguments, input and output file paths");
            }
            if (!new FileInfo(args[0]).Exists)
                throw new FileNotFoundException("The file: " + args[0] + ", does not exist");

            bool args2Ok = false;
            try
            {
                new FileInfo(args[1]);
                args2Ok = true;
            }
            catch (ArgumentException) { }
            catch (PathTooLongException) { }
            catch (NotSupportedException) { }
            if (!args2Ok)
            {
                throw new ArgumentException("The output filename" + args[1] + ", is not a legal path.");
            }
            var runner = new Runner();
            runner.ShowProgressAtConsole = true;
            runner.ProgressChanged += RunnerProgressHandler;
            bool success = runner.RunGUIAlgorithm(args[0], args[1]);

            if (success)
                Console.WriteLine("Success");
            else
                Console.WriteLine("Failed");
            Console.ReadLine();

        }

        private static void RunnerProgressHandler(Runner id, ProgressInfo e)
        {
            Console.WriteLine(e.ProgressCurrentJob.ToString("0.0") + "% complete; " + e.Value.ToString("0.0") + "% complete overall");
        }
    }
}
