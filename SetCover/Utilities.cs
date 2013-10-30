//Joshua Aldrich

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace SetCover
{
	static class Utilities
	{

		/// <summary>
		/// Generates a datatable with all entries being a string
		/// </summary>
		/// <param name="filePath">input file name</param>
		/// <param name="addDataSetName">whether to add the datasetname as a column</param>
		/// <returns></returns>
		public static DataTable TextFileToDataTableAssignTypeString(string filePath, bool addDataSetName)
		{
			string line = "";
			string[] fields = null;
			DataTable dt = new DataTable();

			using (var sr = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
			{
				// first line has headers   
				if ((line = sr.ReadLine()) != null)
				{
					fields = line.Split(new char[] { '\t'});
					foreach (string s in fields)
					{
						dt.Columns.Add(s);
						dt.Columns[s].DefaultValue = "";
					}

				}
				else
				{
					// it's empty, that's an error   
					throw new ApplicationException("The data provided is not in a valid format: Header row must be tab delimited");
				}

				// fill the rest of the table; positional   
				while ((line = sr.ReadLine()) != null)
				{
					if (!string.IsNullOrEmpty(line))
					{
						DataRow row = dt.NewRow();

						fields = line.Split(new char[] {'\t'});
						int i = 0;
						foreach (string s in fields)
						{
							row[i] = s;
							i++;
						}
						dt.Rows.Add(row);
					}
					
				}
			}

			//if (!dt.Columns.Contains("DatasetName") && addDataSetName)
			//{
			//    string dataSetName = Regex.Replace(Path.GetFileName(fileName).Split('.')[0],
			//"_fht|_fht_MSGF|_fht_MSGF_full|_full|_ReporterIons|_MSGF|_cut", "");

			//    dt.Columns.Add("DataSetName", typeof(string));
			//    foreach (DataRow row in dt.Rows)
			//    {
			//        row["DataSetName"] = dataSetName;
			//    }
			//}

			return dt;


		}

        public static bool ReadTable(string filePath, out List<RowEntry> rows)
        {
            try
            {
                rows = new List<RowEntry>();
                DataTable dt = TextFileToDataTableAssignTypeString(filePath, false);

	            if (!dt.Columns.Contains("Protein"))
		            throw new Exception("Input file is missing column 'Protein'");

				if (!dt.Columns.Contains("Peptide"))
					throw new Exception("Input file is missing column 'Peptide'");

                foreach (DataRow drow in dt.Rows)
                {
                    var entry = new RowEntry
                    {
                        ProteinEntry = (string)drow["Protein"],
                        PeptideEntry = (string)drow["Peptide"]
                    };
                    rows.Add(entry);
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Problem adding rows to List<RowEntry>: " + ex.Message);
            }

            return true;
        }


		/// <summary>
		/// Writes a datatable to text file
		/// </summary>
		/// <param name="dt"></param>
		/// <param name="filePath"></param>
		public static void WriteDataTableToText(DataTable dt, string filePath)
		{
			using (StreamWriter sw = new StreamWriter(filePath))
			{
				string s = dt.Columns[0].ColumnName;
				for (int i = 1; i < dt.Columns.Count; i++)
				{
					s += "\t" + dt.Columns[i].ColumnName;
				}
				sw.WriteLine(s);

				s = string.Empty;
				foreach (DataRow row in dt.Rows)
				{
					s = "" + row[0];
					for (int i = 1; i < dt.Columns.Count; i++)
					{
						s += "\t" + row[i];
					}
					sw.WriteLine(s);
					s = string.Empty;
				}


			}
		}

        /// <summary>
        /// Writes a simple table with protein(group) for one column and associated
        /// peptides for the other column.
        /// </summary>
        /// <param name="outData"></param>
        /// <param name="filepath"></param>
        public static void WriteTable(List<Node> outData, string filepath)
        {
            using (var sw = new StreamWriter(new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.Read)))
            {
                string header = "GroupID\tProtein_First\tPeptide\tProtein_List";
                sw.WriteLine(header);
                foreach (Node proteinNode in outData)
                {
	                string proteinFirst;
	                string proteinList;

	                if (proteinNode.GetType() == typeof (ProteinGroup))
	                {
						var currentGroup = (ProteinGroup)proteinNode;
		                proteinFirst = currentGroup.NodeNameFirst;
						proteinList = currentGroup.nodeName;
	                }
	                else
	                {
		                proteinFirst = proteinNode.nodeName;
		                proteinList = proteinNode.nodeName;
	                }

	                foreach(Node child in proteinNode.children)
                    {
                        if(child.GetType() == typeof(PeptideGroup))
                        {
	                        var currentPeptides = (Group)child;
							foreach (Node groupedpep in currentPeptides.GetNodeGroup())
                            {
                                sw.WriteLine("{0}\t{1}\t{2}\t{3}",
									proteinNode.Id, proteinFirst, groupedpep.nodeName, proteinList);
                            }
                        }
                        else
                        {
							sw.WriteLine("{0}\t{1}\t{2}\t{3}",
								proteinNode.Id, proteinFirst, child.nodeName, proteinList);
                        }
                    }

                }
            }
        }


        public static void WriteTableToConsole(List<Node> outData)
        {
            string header = "Protein\tPeptide";
            Console.WriteLine(header);
            foreach (Node node in outData)
            {
                foreach (Node child in node.children)
                {
                    if (child.GetType() == typeof(PeptideGroup))
                    {
                        foreach (Node groupedpep in ((Group)child).GetNodeGroup())
                        {
                            Console.WriteLine(string.Format("{0}\t{1}",
                                node.nodeName, groupedpep.nodeName));
                        }
                    }
                    else
                    {
                        Console.WriteLine(string.Format("{0}\t{1}",
                                node.nodeName, child.nodeName));
                    }
                }

            }
        }


	}
}
