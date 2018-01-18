//Joshua Aldrich

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using SetCover.Objects;

namespace SetCover
{
	internal static class Utilities
	{
		/// <summary>
		/// Generates a datatable with all entries being a string
		/// </summary>
		/// <param name="filePath">Input file path</param>
		/// <param name="columnsToTrack">Column names to store in the data table</param>
		/// <returns></returns>
		private static DataTable TextFileToDataTableAssignTypeString(string filePath, ICollection<string> columnsToTrack)
		{
		    var dt = new DataTable();

			using (var sr = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
			{
				if (sr.EndOfStream)
				{
					throw new Exception("Input file is empty: " + filePath);
				}

				// first line has headers
				var headerLine = sr.ReadLine();

				var colIndicesToLoad = new SortedSet<int>();

				if (string.IsNullOrWhiteSpace(headerLine))
				{
					// Header line is empty, that's an error
					throw new Exception("The data provided is not in a valid format: Header line must be tab delimited");
				}

				var headerFields = headerLine.Split('\t');

				if (columnsToTrack == null || columnsToTrack.Count == 0)
				{
					// Load all of the data
					foreach (var fieldIndex in Enumerable.Range(0, headerFields.Length))
					{
						var fieldName = headerFields[fieldIndex];
						colIndicesToLoad.Add(fieldIndex);
						dt.Columns.Add(fieldName);
						dt.Columns[fieldName].DefaultValue = "";
					}
				}
				else
				{
					// Only load data in the specified columns
					for (var fieldIndex = 0; fieldIndex < headerFields.Length; fieldIndex++)
					{
						var fieldName = headerFields[fieldIndex];
						if (columnsToTrack.Contains(fieldName))
						{
							colIndicesToLoad.Add(fieldIndex);
							dt.Columns.Add(fieldName);
							dt.Columns[fieldName].DefaultValue = "";
						}
					}
				}

				// Fill the rest of the table
				while (!sr.EndOfStream)
				{
					var dataLine = sr.ReadLine();

					if (string.IsNullOrWhiteSpace(dataLine))
						continue;

					var row = dt.NewRow();

					var dataVals = dataLine.Split('\t');

					foreach (var fieldIndex in colIndicesToLoad)
					{
						row[fieldIndex] = dataVals[fieldIndex];
					}

					dt.Rows.Add(row);
				}
			}

			return dt;

		}

		/// <summary>
		/// Load data from a tab-delimited text file with columns Protein and Peptide
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="rows">List of protein/peptide pairs (protein name and peptide sequence)</param>
		/// <returns></returns>
		public static bool ReadProteinPeptideTable(string filePath, out List<RowEntry> rows, bool showProgress = true)
		{
			try
			{
				if (showProgress)
					Console.WriteLine("Loading proteins and peptides from " + filePath);

				rows = new List<RowEntry>();
				var columnsToTrack = new SortedSet<string>(StringComparer.OrdinalIgnoreCase) {
					"Protein", "Peptide"
				};

				var dt = TextFileToDataTableAssignTypeString(filePath, columnsToTrack);

				if (!dt.Columns.Contains("Protein"))
					throw new Exception("Input file is missing column 'Protein'");

				if (!dt.Columns.Contains("Peptide"))
					throw new Exception("Input file is missing column 'Peptide'");

				foreach (DataRow item in dt.Rows)
				{
					var entry = new RowEntry
					{
						ProteinEntry = (string)item["Protein"],
						PeptideEntry = (string)item["Peptide"]
					};
					rows.Add(entry);
				}

				if (showProgress)
					Console.WriteLine("Loaded {0} rows", rows.Count);
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("Problem loading data from {0}: {1}", filePath, ex.Message));
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

		public static List<string> ConvertNodesToStringList(List<Node> outData, GlobalIDContainer globalIDTracker)
		{
			var outLines = new List<string>();

			var headerNames = new List<string> { "Protein", "Peptide"};
			outLines.Add(string.Join("\t", headerNames));

			foreach (var node in outData)
			{
				foreach (var child in node.Children)
				{
					if (child.GetType() == typeof(PeptideGroup))
					{
						var proteinList = GetProteinList(node, globalIDTracker);
						foreach (var groupedpep in ((Group)child).GetNodeGroup())
						{
							outLines.Add(string.Format("{0}\t{1}", proteinList, groupedpep.NodeName));
						}
					}
					else if (node is ProteinGroup thisGroup)
					{
						var proteinList = GetProteinList(node, globalIDTracker);
						outLines.Add(string.Format("{0}\t{1}", proteinList, child.NodeName));
					}
					else
					{
						outLines.Add(string.Format("{0}\t{1}", node.NodeName, child.NodeName));
					}

				}

			}
		}

			return outLines;

		private static string GetProteinList(Node node, GlobalIDContainer globalIDTracker)
		{
			if (node.NodeName.IndexOf(Group.LIST_SEP_CHAR) > 0)
			{
				var proteinList = globalIDTracker.IDListToNameListString(node.NodeName, Group.LIST_SEP_CHAR);
				return proteinList.Replace(Group.LIST_SEP_CHAR.ToString(), "; ");
			}
			else
				return node.NodeName;
		}
	}
}
