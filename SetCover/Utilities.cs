//Joshua Aldrich

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using SetCover.Objects;

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
					fields = line.Split(new char[] { '\t' });
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

						fields = line.Split(new char[] { '\t' });
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
		/// Creates tab-delimited text file parsimonyResultsFilePath with the protein groups and member peptides
		/// Creates tab-delimited text file proteinGroupMembersFilePath with the members of each protein group
		/// </summary>
		/// <param name="outData"></param>
		/// <param name="parsimonyResultsFilePath"></param>
		/// <param name="proteinGroupMembersFilePath"></param>
		/// <param name="globalIDTracker"></param>
		public static void SaveResults(List<Node> outData, string parsimonyResultsFilePath, string proteinGroupMembersFilePath, GlobalIDContainer globalIDTracker)
		{
			using (var sw = new StreamWriter(new FileStream(parsimonyResultsFilePath, FileMode.Create, FileAccess.Write, FileShare.Read)))
			{
				using (var swGroupMembers = new StreamWriter(new FileStream(proteinGroupMembersFilePath, FileMode.Create, FileAccess.Write, FileShare.Read)))
				{
					string header = "GroupID\tProtein_First\tPeptide\tProtein_List\tProtein_Count\tGroup_Count";
					sw.WriteLine(header);

					header = "GroupID\tProtein";
					swGroupMembers.WriteLine(header);

					// Step through the data to determine the number of groups that each peptide is in

					var peptideToProteinGroupMap = new Dictionary<string, int>();

					foreach (Node proteinNode in outData)
					{
						foreach (Node child in proteinNode.children)
						{
							if (child.GetType() == typeof(PeptideGroup))
							{
								var currentPeptides = (Group)child;
								foreach (Node groupedpep in currentPeptides.GetNodeGroup())
								{
									UpdatePeptideToProteinGroupMap(peptideToProteinGroupMap, groupedpep.nodeName);
								}
							}
							else
							{
								UpdatePeptideToProteinGroupMap(peptideToProteinGroupMap, child.nodeName);
							}
						}
					}


					// Now write out the results
					int groupID = 0;
					foreach (Node proteinNode in outData)
					{
						string proteinFirst;
						string proteinNameOrList;
						int proteinsInGroupCount = 0;

						groupID++;

						// Append one or more lines to T_Parsimony_Group_Members.txt 
						if (proteinNode.GetType() == typeof(ProteinGroup))
						{
							var currentGroup = (ProteinGroup)proteinNode;
							proteinFirst = currentGroup.NodeNameFirst;
							if (currentGroup.nodeName.IndexOf(Group.LIST_SEP_CHAR) > 0)
							{
								List<string> proteinList = globalIDTracker.IDListToNameList(currentGroup.nodeName, Group.LIST_SEP_CHAR);
								proteinNameOrList = String.Join("; ", proteinList);
								proteinsInGroupCount = proteinList.Count;

								foreach (var proteinMember in proteinList)
								{
									WriteOutputGroupMemberLine(swGroupMembers, groupID, proteinMember);
								}
							}
							else
							{
								// Note: this code should never be reached
								proteinNameOrList = currentGroup.nodeName;
								proteinsInGroupCount = 1;
								WriteOutputGroupMemberLine(swGroupMembers, groupID, proteinNameOrList);
							}
						}
						else
						{
							proteinFirst = proteinNode.nodeName;
							proteinNameOrList = proteinNode.nodeName;
							proteinsInGroupCount = 1;

							WriteOutputGroupMemberLine(swGroupMembers, groupID, proteinNameOrList);
						}

						// Append one or more lines to T_Parsimony_Grouping.txt
						foreach (Node child in proteinNode.children)
						{
							if (child.GetType() == typeof(PeptideGroup))
							{
								var currentPeptides = (Group)child;
								foreach (Node groupedpep in currentPeptides.GetNodeGroup())
								{
									WriteOutputGroupingLine(sw, peptideToProteinGroupMap, groupID, proteinFirst, groupedpep.nodeName, proteinNameOrList, proteinsInGroupCount);
								}
							}
							else
							{
								WriteOutputGroupingLine(sw, peptideToProteinGroupMap, groupID, proteinFirst, child.nodeName, proteinNameOrList, proteinsInGroupCount);
							}
						}

					}

				} // End Using
			} // End Using
		}

		private static void UpdatePeptideToProteinGroupMap(Dictionary<string, int> peptideToProteinGroupMap, string peptide)
		{
			int groupCount;
			if (peptideToProteinGroupMap.TryGetValue(peptide, out groupCount))
			{
				peptideToProteinGroupMap[peptide] = groupCount + 1;
			}
			else
			{
				peptideToProteinGroupMap.Add(peptide, 1);
			}
		}

		private static void WriteOutputGroupMemberLine(StreamWriter swGroupMembers, int groupID, string proteinNameOrList)
		{
			swGroupMembers.WriteLine("{0}\t{1}", groupID, proteinNameOrList);
		}

		private static void WriteOutputGroupingLine(StreamWriter sw, Dictionary<string, int> peptideToProteinGroupMap,
			int groupID, string proteinFirst, string peptide, string proteinNameOrList, int proteinsInGroupCount)
		{
			int peptideGroupCount = 0;
			peptideToProteinGroupMap.TryGetValue(peptide, out peptideGroupCount);

			sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", groupID, proteinFirst, peptide, proteinNameOrList, proteinsInGroupCount, peptideGroupCount);
		}

		public static void WriteTableToConsole(List<Node> outData, GlobalIDContainer globalIDTracker)
		{
			const string header = "Protein\tPeptide";
			Console.WriteLine(header);
			foreach (Node node in outData)
			{
				foreach (Node child in node.children)
				{
					if (child.GetType() == typeof(PeptideGroup))
					{
						foreach (Node groupedpep in ((Group)child).GetNodeGroup())
						{
							string proteinList;
							if (node.nodeName.IndexOf(Group.LIST_SEP_CHAR) > 0)
							{
								proteinList = globalIDTracker.IDListToNameListString(node.nodeName, Group.LIST_SEP_CHAR);
								proteinList = proteinList.Replace(Group.LIST_SEP_CHAR.ToString(), "; ");
							}
							else
								proteinList = node.nodeName;

							Console.WriteLine(string.Format("{0}\t{1}",
								proteinList, groupedpep.nodeName));
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
