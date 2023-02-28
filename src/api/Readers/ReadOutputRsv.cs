using SWAT.Check.Helpers;
using SWAT.Check.Models;
using SWAT.Check.Schemas;
using System.Data.SQLite;

namespace SWAT.Check.Readers;

public class ReadOutputRsv : OutputFileReader
{
	public ReadOutputRsv(SWATOutputConfig configSettings, string fileName = OutputFileNames.OutputRsv) : base(configSettings, fileName)
	{
	}

	public override void ReadFile(bool abort)
	{
		if (abort) return;

		var context = new SWATOutputDbContext(_configSettings.DatabaseFile);
		using (var conn = context.GetConnection())
		{
			conn.Open();

			using (var cmd = new SQLiteCommand(conn))
			{
				using (var transaction = conn.BeginTransaction())
				{
					IEnumerable<string> lines = File.ReadLines(_filePath);
					List<string> headerColumns = new List<string>();
					int i = 1;
					int valuesColumnIndex = OutputRsvSchema.ValuesStartIndex;
					//var headingDictionary = OutputRsvSchema.Headings;
					Dictionary<string, string> headingDictionary = new Dictionary<string, string>();

					int currentYear = _configSettings.SimulationStartOn.Year + _configSettings.SkipYears;
					int numYears = _configSettings.SimulationEndOn.Year - currentYear + 1;

					foreach (string line in lines)
					{
						if (i == OutputRsvSchema.HeaderLineNumber)
						{
							int columnIndex = valuesColumnIndex; //Start reading variable headings after MON.
							while (columnIndex < line.Length)
							{
								headerColumns.Add(line.Substring(columnIndex, OutputRsvSchema.ValuesColumnLength).Trim());
								columnIndex += OutputRsvSchema.ValuesColumnLength;
							}

							headingDictionary = LoadColumnNamesToHeadingsDictionary(typeof(OutputRsv), headerColumns, valuesColumnIndex, OutputRsvSchema.ValuesColumnLength);

							List<string> paramNames = new List<string>();
							List<string> paramValues = new List<string>();
							foreach (string header in headerColumns)
							{
								paramNames.Add(string.Format("`{0}`", headingDictionary[header]));
								paramValues.Add(string.Format("@{0}", headingDictionary[header]));
							}

							cmd.CommandText = string.Format("INSERT INTO OutputRsv (`RES`, `Month`, `Day`, `Year`, {0}) VALUES (@RES, @Month, @Day, @Year, {1});", string.Join(", ", paramNames), string.Join(", ", paramValues));
						}
						else if (i > OutputRsvSchema.HeaderLineNumber && !String.IsNullOrWhiteSpace(line))
						{
							cmd.Parameters.Clear();
							int res = OutputRsvSchema.RES.GetInt(line);

							cmd.Parameters.AddWithValue("@RES", res);

							switch (_configSettings.PrintCode)
							{
								case SWATPrintSetting.Daily:
									int julianDay = OutputRsvSchema.MON.GetInt(line);
									DateTime d = new DateTime(currentYear, 1, 1).AddDays(julianDay - 1);
									cmd.Parameters.AddWithValue("@Month", d.Month);
									cmd.Parameters.AddWithValue("@Day", d.Day);
									cmd.Parameters.AddWithValue("@Year", d.Year);

									if (res == _configSettings.NumReservoirs && ((DateTime.IsLeapYear(currentYear) && julianDay == 366) || julianDay == 365))
									{
										currentYear++;
									}
									break;
								case SWATPrintSetting.Monthly:
									cmd.Parameters.AddWithValue("@Day", 0);

									double mon = OutputRsvSchema.MON.GetDouble(line);

									if (mon < 13)
									{
										cmd.Parameters.AddWithValue("@Month", (int)mon);
										cmd.Parameters.AddWithValue("@Year", currentYear);
									}
									else
									{
										cmd.Parameters.AddWithValue("@Month", 0);
										cmd.Parameters.AddWithValue("@Year", (int)mon);
										if (res == _configSettings.NumReservoirs)
										{
											currentYear++;
										}
									}
									break;
								case SWATPrintSetting.Yearly:
									cmd.Parameters.AddWithValue("@Day", 0);
									cmd.Parameters.AddWithValue("@Month", 0);

									double year = OutputRsvSchema.MON.GetDouble(line);
									cmd.Parameters.AddWithValue("@Year", (int)year);

									break;
							}

							int columnIndex = valuesColumnIndex;
							int columnLength = OutputRsvSchema.ValuesColumnLength;

							foreach (string heading in headerColumns)
							{
								cmd.Parameters.AddWithValue("@" + headingDictionary[heading], line.ParseDouble(columnIndex, columnLength));
								columnIndex += columnLength;
							}

							cmd.ExecuteNonQuery();
						}
						i++;
					}

					transaction.Commit();
				}
			}

			conn.Close();
		}
	}
}
