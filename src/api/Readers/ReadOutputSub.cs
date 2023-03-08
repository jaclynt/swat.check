using SWAT.Check.Helpers;
using SWAT.Check.Models;
using SWAT.Check.Schemas;
using System.Data.SQLite;

namespace SWAT.Check.Readers;

public class ReadOutputSub : OutputFileReader
{
	public ReadOutputSub(SWATOutputConfig configSettings, string fileName = OutputFileNames.OutputSub) : base(configSettings, fileName)
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

					//New for rev.670. They added a space and shifted everything over. Try and detect that.
					int adjustSpace = 0;
					int testSub;
					bool noSuccess = true;
					while (noSuccess)
					{
						try
						{
							SchemaLine testSchema = new SchemaLine { StartIndex = 6 + adjustSpace, Length = 4 };
							testSub = testSchema.GetInt(lines.ToArray()[9]);
							noSuccess = false;
						}
						catch (FormatException)
						{
							adjustSpace++;
							if (adjustSpace > 15) noSuccess = false;
						}
					}

					OutputSubSchemaInstance outputSubSchema = new OutputSubSchemaInstance(adjustSpace);

					List<string> headerColumns = new List<string>();
					int i = 1;
					int areaColumnIndex = _configSettings.UseCalendarDateFormat ? outputSubSchema.AreaHeaderIndexWithCalendarDate : outputSubSchema.AreaHeaderIndex;
					int headingsAreaColumnIndex = _configSettings.UseCalendarDateFormat ? OutputSubSchema.AreaHeaderIndexWithCalendarDate : OutputSubSchema.AreaHeaderIndex;
					Dictionary<string, string> headingDictionary = new Dictionary<string, string>();

					int currentYear = _configSettings.SimulationStartOn.Year + _configSettings.SkipYears;
					int numYears = _configSettings.SimulationEndOn.Year - currentYear + 1;

					int numSubbasins = _configSettings.NumSubbasins;

					foreach (string line in lines)
					{
						if (i == OutputSubSchema.HeaderLineNumber)
						{
							int columnIndex = headingsAreaColumnIndex + OutputSubSchema.ValuesColumnLength; //Area is the last required column, so start reading variable headings after this.
							while (columnIndex < line.Length)
							{
								headerColumns.Add(line.Substring(columnIndex, OutputSubSchema.ValuesColumnLength).Trim());
								columnIndex += OutputSubSchema.ValuesColumnLength;
							}

							headingDictionary = LoadColumnNamesToHeadingsDictionary(typeof(OutputSub), headerColumns, headingsAreaColumnIndex + OutputSubSchema.ValuesColumnLength, OutputSubSchema.ValuesColumnLength);

							List<string> paramNames = new List<string>();
							List<string> paramValues = new List<string>();
							foreach (string header in headerColumns)
							{
								paramNames.Add(string.Format("`{0}`", headingDictionary[header]));
								paramValues.Add(string.Format("@{0}", headingDictionary[header]));
							}

							cmd.CommandText = string.Format("INSERT INTO OutputSub (`SUB`, `GIS`, `Month`, `Day`, `Year`, `YearSpan`, `Area`, {0}) VALUES (@SUB, @GIS, @Month, @Day, @Year, @YearSpan, @Area, {1});", string.Join(", ", paramNames), string.Join(", ", paramValues));
						}
						else if (i > OutputSubSchema.HeaderLineNumber && !String.IsNullOrWhiteSpace(line))
						{
							cmd.Parameters.Clear();
							int sub = outputSubSchema.SUB.GetInt(line);

							cmd.Parameters.AddWithValue("@SUB", sub);
							cmd.Parameters.AddWithValue("@GIS", outputSubSchema.GIS.GetInt(line));

							switch (_configSettings.PrintCode)
							{
								case SWATPrintSetting.Daily:
									if (_configSettings.UseCalendarDateFormat)
									{
										cmd.Parameters.AddWithValue("@Month", outputSubSchema.MO.GetInt(line));
										cmd.Parameters.AddWithValue("@Day", outputSubSchema.DA.GetInt(line));
										cmd.Parameters.AddWithValue("@Year", outputSubSchema.YR.GetInt(line));
									}
									else
									{
										int julianDay = outputSubSchema.MON.GetInt(line);
										DateTime d = new DateTime(currentYear, 1, 1).AddDays(julianDay - 1);
										cmd.Parameters.AddWithValue("@Month", d.Month);
										cmd.Parameters.AddWithValue("@Day", d.Day);
										cmd.Parameters.AddWithValue("@Year", d.Year);

										if (sub == numSubbasins && ((DateTime.IsLeapYear(currentYear) && julianDay == 366) || julianDay == 365))
										{
											currentYear++;
										}
									}
									cmd.Parameters.AddWithValue("@YearSpan", 0);
									break;
								case SWATPrintSetting.Monthly:
									cmd.Parameters.AddWithValue("@Day", 0);

									double mon = outputSubSchema.MON.GetDouble(line);

									if (currentYear <= _configSettings.SimulationEndOn.Year)
									{
										if (mon < 13)
										{
											cmd.Parameters.AddWithValue("@Month", (int)mon);
											cmd.Parameters.AddWithValue("@Year", currentYear);
										}
										else
										{
											cmd.Parameters.AddWithValue("@Month", 0);
											cmd.Parameters.AddWithValue("@Year", (int)mon);
											if (sub == numSubbasins)
											{
												currentYear++;
											}
										}
										cmd.Parameters.AddWithValue("@YearSpan", 0);
									}
									else
									{
										cmd.Parameters.AddWithValue("@Month", 0);
										cmd.Parameters.AddWithValue("@Year", 0);
										cmd.Parameters.AddWithValue("@YearSpan", mon);
									}
									break;
								case SWATPrintSetting.Yearly:
									cmd.Parameters.AddWithValue("@Day", 0);
									cmd.Parameters.AddWithValue("@Month", 0);

									double year = outputSubSchema.MON.GetDouble(line);
									if (year <= _configSettings.SimulationEndOn.Year && year >= _configSettings.SimulationStartOn.Year)
									{
										cmd.Parameters.AddWithValue("@Year", (int)year);
										cmd.Parameters.AddWithValue("@YearSpan", 0);
									}
									else
									{
										cmd.Parameters.AddWithValue("@Year", 0);
										cmd.Parameters.AddWithValue("@YearSpan", year);
									}

									break;
							}

							int columnIndex = areaColumnIndex;
							int columnLength = outputSubSchema.ValuesColumnLength;
							//Possible temporary bug in swat.exe. Values not quite aligned properly in calendar format.
							if (_configSettings.UseCalendarDateFormat)
							{
								columnIndex++;
							}

							cmd.Parameters.AddWithValue("@Area", line.ParseDouble(columnIndex, columnLength));
							columnIndex += columnLength;

							foreach (string heading in headerColumns)
							{
								int extraSpace = 0;
								if (headingDictionary[heading].Equals("CHOLA"))
								{
									extraSpace = 1;
								}
								cmd.Parameters.AddWithValue("@" + headingDictionary[heading], line.ParseDouble(columnIndex, columnLength + extraSpace));
								columnIndex += columnLength + extraSpace;
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
