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
		    var dt = new DataTable();

			using (var sr = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
			{
				// first line has headers
			    string line;
			    string[] fields;
			    if ((line = sr.ReadLine()) != null)
				{
					fields = line.Split('\t');
					foreach (var s in fields)
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
						var row = dt.NewRow();

						fields = line.Split('\t');
						var i = 0;
						foreach (var s in fields)
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
				var dt = TextFileToDataTableAssignTypeString(filePath, false);

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
			using (var sw = new StreamWriter(filePath))
			{
				var headerLine = dt.Columns[0].ColumnName;
				for (var i = 1; i < dt.Columns.Count; i++)
				{
				    headerLine += "\t" + dt.Columns[i].ColumnName;
				}
				sw.WriteLine(headerLine);

				foreach (DataRow row in dt.Rows)
				{
					var dataLine = row[0];
					for (var i = 1; i < dt.Columns.Count; i++)
					{
					    dataLine += "\t" + row[i];
					}
					sw.WriteLine(dataLine);
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
					var header = "GroupID\tProtein_First\tPeptide\tProtein_List\tProtein_Count\tGroup_Count";
					sw.WriteLine(header);

					header = "GroupID\tProtein";
					swGroupMembers.WriteLine(header);

					// Step through the data to determine the number of groups that each peptide is in

					var peptideToProteinGroupMap = new Dictionary<string, int>();

					foreach (var proteinNode in outData)
					{
						foreach (var child in proteinNode.Children)
						{
							if (child.GetType() == typeof(PeptideGroup))
							{
								var currentPeptides = (Group)child;
								foreach (var groupedpep in currentPeptides.GetNodeGroup())
								{
									UpdatePeptideToProteinGroupMap(peptideToProteinGroupMap, groupedpep.NodeName);
								}
							}
							else
							{
								UpdatePeptideToProteinGroupMap(peptideToProteinGroupMap, child.NodeName);
							}
						}
					}


					// Now write out the results
					var groupID = 0;
					foreach (var proteinNode in outData)
					{
						string proteinFirst;
						string proteinNameOrList;
						int proteinsInGroupCount;

						groupID++;

						// Append one or more lines to T_Parsimony_Group_Members.txt
						if (proteinNode.GetType() == typeof(ProteinGroup))
						{
							var currentGroup = (ProteinGroup)proteinNode;
							proteinFirst = currentGroup.NodeNameFirst;
							if (currentGroup.NodeName.IndexOf(Group.LIST_SEP_CHAR) > 0)
							{
								var proteinList = globalIDTracker.IDListToNameList(currentGroup.NodeName, Group.LIST_SEP_CHAR);
								proteinNameOrList = string.Join("; ", proteinList);
								proteinsInGroupCount = proteinList.Count;

								foreach (var proteinMember in proteinList)
								{
									WriteOutputGroupMemberLine(swGroupMembers, groupID, proteinMember);
								}
							}
							else
							{
								// Note: this code should never be reached
								proteinNameOrList = currentGroup.NodeName;
								proteinsInGroupCount = 1;
								WriteOutputGroupMemberLine(swGroupMembers, groupID, proteinNameOrList);
							}
						}
						else
						{
							proteinFirst = proteinNode.NodeName;
							proteinNameOrList = proteinNode.NodeName;
							proteinsInGroupCount = 1;

							WriteOutputGroupMemberLine(swGroupMembers, groupID, proteinNameOrList);
						}

						// Append one or more lines to T_Parsimony_Grouping.txt
						foreach (var child in proteinNode.Children)
						{
							if (child.GetType() == typeof(PeptideGroup))
							{
								var currentPeptides = (Group)child;
								foreach (var groupedpep in currentPeptides.GetNodeGroup())
								{
									WriteOutputGroupingLine(sw, peptideToProteinGroupMap, groupID, proteinFirst, groupedpep.NodeName, proteinNameOrList, proteinsInGroupCount);
								}
							}
							else
							{
								WriteOutputGroupingLine(sw, peptideToProteinGroupMap, groupID, proteinFirst, child.NodeName, proteinNameOrList, proteinsInGroupCount);
							}
						}

					}

				} // End Using
			} // End Using
		}

		private static void UpdatePeptideToProteinGroupMap(IDictionary<string, int> peptideToProteinGroupMap, string peptide)
		{
		    if (peptideToProteinGroupMap.TryGetValue(peptide, out var groupCount))
			{
				peptideToProteinGroupMap[peptide] = groupCount + 1;
			}
			else
			{
				peptideToProteinGroupMap.Add(peptide, 1);
			}
		}

		private static void WriteOutputGroupMemberLine(TextWriter swGroupMembers, int groupID, string proteinNameOrList)
		{
			swGroupMembers.WriteLine("{0}\t{1}", groupID, proteinNameOrList);
		}

		private static void WriteOutputGroupingLine(TextWriter sw, IReadOnlyDictionary<string, int> peptideToProteinGroupMap,
			int groupID, string proteinFirst, string peptide, string proteinNameOrList, int proteinsInGroupCount)
		{
		    peptideToProteinGroupMap.TryGetValue(peptide, out var peptideGroupCount);

			sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", groupID, proteinFirst, peptide, proteinNameOrList, proteinsInGroupCount, peptideGroupCount);
		}

		public static void WriteTableToConsole(List<Node> outData, GlobalIDContainer globalIDTracker)
		{
			const string header = "Protein\tPeptide";
			Console.WriteLine(header);
			foreach (var node in outData)
			{
				foreach (var child in node.Children)
				{
					if (child.GetType() == typeof(PeptideGroup))
					{
						foreach (var groupedpep in ((Group)child).GetNodeGroup())
						{
							string proteinList;
							if (node.NodeName.IndexOf(Group.LIST_SEP_CHAR) > 0)
							{
								proteinList = globalIDTracker.IDListToNameListString(node.NodeName, Group.LIST_SEP_CHAR);
								proteinList = proteinList.Replace(Group.LIST_SEP_CHAR.ToString(), "; ");
							}
							else
								proteinList = node.NodeName;

							Console.WriteLine("{0}\t{1}", proteinList, groupedpep.NodeName);
						}
					}
					else
					{
						Console.WriteLine("{0}\t{1}", node.NodeName, child.NodeName);
					}
				}

			}
		}


	}
}
