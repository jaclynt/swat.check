using SWAT.Check.Helpers;
using SWAT.Check.Models;
using SWAT.Check.Schemas;
using System.Data.SQLite;

namespace SWAT.Check.Readers;

public class ReadOutputRch : OutputFileReader
{
	public ReadOutputRch(SWATOutputConfig configSettings, string fileName = OutputFileNames.OutputRch) : base(configSettings, fileName)
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
					int testRch;
					try
					{
						testRch = OutputRchSchema.RCH.GetInt(lines.ToArray()[9]);
					}
					catch (FormatException)
					{
						adjustSpace = 1;
					}

					OutputRchSchemaInstance outputRchSchema = new OutputRchSchemaInstance(adjustSpace);

					List<string> headerColumns = new List<string>();
					int i = 1;
					int areaColumnIndex = _configSettings.UseCalendarDateFormat ? outputRchSchema.AreaHeaderIndexWithCalendarDate : outputRchSchema.AreaHeaderIndex;
					int headingsAreaColumnIndex = _configSettings.UseCalendarDateFormat ? OutputRchSchema.AreaHeaderIndexWithCalendarDate : OutputRchSchema.AreaHeaderIndex;
					Dictionary<string, string> headingDictionary = new Dictionary<string, string>();

					int currentYear = _configSettings.SimulationStartOn.Year + _configSettings.SkipYears;
					int numYears = _configSettings.SimulationEndOn.Year - currentYear + 1;

					int numSubbasins = _configSettings.NumSubbasins;

					bool insertTotNConc = false;
					bool insertTotPConc = false;
					bool insertDisoxConc = false;

					foreach (string line in lines)
					{
						if (i == OutputRchSchema.HeaderLineNumber)
						{
							int columnIndex = headingsAreaColumnIndex + OutputRchSchema.ValuesColumnLength; //Area is the last required column, so start reading variable headings after this.
							while (columnIndex < line.Length)
							{
								headerColumns.Add(line.Substring(columnIndex, OutputRchSchema.ValuesColumnLength).Trim());
								columnIndex += OutputRchSchema.ValuesColumnLength;
							}

							headingDictionary = LoadColumnNamesToHeadingsDictionary(typeof(OutputRch), headerColumns, headingsAreaColumnIndex + OutputRchSchema.ValuesColumnLength, OutputRchSchema.ValuesColumnLength);

							List<string> paramNames = new List<string>();
							List<string> paramValues = new List<string>();
							foreach (string header in headerColumns)
							{
								if (headingDictionary.ContainsKey(header))
								{
									paramNames.Add(string.Format("`{0}`", headingDictionary[header]));
									paramValues.Add(string.Format("@{0}", headingDictionary[header]));
								}
							}

							if (headerColumns.Contains("FLOW_OUTcms"))
							{
								if (headerColumns.Contains("TOT Nkg"))
								{
									paramNames.Add("`TOT_N_CONC`");
									paramValues.Add("@TOT_N_CONC");
									insertTotNConc = true;
								}
								if (headerColumns.Contains("TOT Pkg"))
								{
									paramNames.Add("`TOT_P_CONC`");
									paramValues.Add("@TOT_P_CONC");
									insertTotPConc = true;
								}
								if (headerColumns.Contains("DISOX_OUTkg"))
								{
									paramNames.Add("`DISOX_CONC`");
									paramValues.Add("@DISOX_CONC");
									insertDisoxConc = true;
								}
							}

							cmd.CommandText = string.Format("INSERT INTO OutputRch (`RCH`, `GIS`, `Month`, `Day`, `Year`, `YearSpan`, `Area`, {0}) VALUES (@RCH, @GIS, @Month, @Day, @Year, @YearSpan, @Area, {1});", string.Join(", ", paramNames), string.Join(", ", paramValues));
						}
						else if (i > outputRchSchema.HeaderLineNumber && !String.IsNullOrWhiteSpace(line))
						{
							cmd.Parameters.Clear();
							int rch = outputRchSchema.RCH.GetInt(line);

							cmd.Parameters.AddWithValue("@RCH", rch);
							cmd.Parameters.AddWithValue("@GIS", outputRchSchema.GIS.GetInt(line));

							int rowMonth = 0;
							int rowDay = 0;
							int rowYear = 0;

							switch (_configSettings.PrintCode)
							{
								case SWATPrintSetting.Daily:
									if (_configSettings.UseCalendarDateFormat)
									{
										rowMonth = outputRchSchema.MO.GetInt(line);
										rowDay = outputRchSchema.DA.GetInt(line);
										rowYear = outputRchSchema.YR.GetInt(line);
										cmd.Parameters.AddWithValue("@Month", rowMonth);
										cmd.Parameters.AddWithValue("@Day", rowDay);
										cmd.Parameters.AddWithValue("@Year", rowYear);
									}
									else
									{
										int julianDay = outputRchSchema.MON.GetInt(line);
										DateTime d = new DateTime(currentYear, 1, 1).AddDays(julianDay - 1);
										cmd.Parameters.AddWithValue("@Month", d.Month);
										cmd.Parameters.AddWithValue("@Day", d.Day);
										cmd.Parameters.AddWithValue("@Year", d.Year);

										rowMonth = d.Month;
										rowDay = d.Day;
										rowYear = d.Year;

										if (rch == numSubbasins && ((DateTime.IsLeapYear(currentYear) && julianDay == 366) || julianDay == 365))
										{
											currentYear++;
										}
									}
									cmd.Parameters.AddWithValue("@YearSpan", 0);
									break;
								case SWATPrintSetting.Monthly:
									cmd.Parameters.AddWithValue("@Day", 0);

									double mon = outputRchSchema.MON.GetDouble(line);

									if (currentYear <= _configSettings.SimulationEndOn.Year)
									{
										if (mon < 13)
										{
											rowMonth = (int)mon;
											cmd.Parameters.AddWithValue("@Month", (int)mon);
											cmd.Parameters.AddWithValue("@Year", currentYear);
										}
										else
										{
											cmd.Parameters.AddWithValue("@Month", 0);
											cmd.Parameters.AddWithValue("@Year", (int)mon);
											rowYear = (int)mon;
											if (rch == numSubbasins)
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

									double year = outputRchSchema.MON.GetDouble(line);
									if (year <= _configSettings.SimulationEndOn.Year && year >= _configSettings.SimulationStartOn.Year)
									{
										rowYear = (int)year;
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
							int columnLength = outputRchSchema.ValuesColumnLength;
							//Possible temporary bug in swat.exe. Values not quite aligned properly in calendar format.
							if (_configSettings.UseCalendarDateFormat)
							{
								columnIndex++;
							}

							cmd.Parameters.AddWithValue("@Area", line.ParseDouble(columnIndex, columnLength));
							columnIndex += columnLength;

							double flowOut = 0;
							double totN = 0;
							double totP = 0;
							double disox = 0;

							foreach (string heading in headerColumns)
							{
								if (headingDictionary.ContainsKey(heading))
								{
									double value = line.ParseDouble(columnIndex, columnLength);

									cmd.Parameters.AddWithValue("@" + headingDictionary[heading], value);
									columnIndex += columnLength;

									if (heading.Equals("FLOW_OUTcms"))
									{
										flowOut = value;
									}
									else if (heading.Equals("TOT Nkg"))
									{
										totN = value;
									}
									else if (heading.Equals("TOT Pkg"))
									{
										totP = value;
									}
									else if (heading.Equals("DISOX_OUTkg"))
									{
										disox = value;
									}
								}
							}

							if (insertTotNConc)
							{
								cmd.Parameters.AddWithValue("@TOT_N_CONC", OutputRch.GetConcentration(totN, flowOut, _configSettings.PrintCode, rowYear, rowMonth));
							}
							if (insertTotPConc)
							{
								cmd.Parameters.AddWithValue("@TOT_P_CONC", OutputRch.GetConcentration(totP, flowOut, _configSettings.PrintCode, rowYear, rowMonth));
							}
							if (insertDisoxConc)
							{
								double additionalTimeFactor = 1d;
								if (_configSettings.PrintCode == SWATPrintSetting.Monthly)
								{
									additionalTimeFactor = 30d;
									if (rowMonth > 0 && rowYear > 0) additionalTimeFactor = DateTime.DaysInMonth(rowYear, rowMonth);
								}
								else if (_configSettings.PrintCode == SWATPrintSetting.Yearly)
								{
									additionalTimeFactor = 365d;
									if (rowYear > 0 && DateTime.IsLeapYear(rowYear)) additionalTimeFactor = 366d;
								}

								double dc = flowOut == 0 ? 0 : (disox * 1000) / (flowOut * 24 * 60 * 60 * additionalTimeFactor);
								cmd.Parameters.AddWithValue("@DISOX_CONC", dc);
							}

							cmd.ExecuteNonQuery();
						}
						i++;
					}

					transaction.Commit();
				}
			}

			conn.Close();
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}
	}
}
