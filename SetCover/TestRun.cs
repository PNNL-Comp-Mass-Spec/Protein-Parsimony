using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace SetCover
{
    class TestRun
    {
        [Test]
        public void Test()
        {
            string file = @"C:\Users\aldr699\Documents\Visual Studio 2010\Projects2\ProteinPars\SetCover\T_Row_Metadata.txt";
            Runner run = new Runner();
            run.RunAlgorithm(file);

        }
    }
}
