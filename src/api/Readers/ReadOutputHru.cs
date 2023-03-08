using SWAT.Check.Helpers;
using SWAT.Check.Models;
using SWAT.Check.Schemas;
using System.Data.SQLite;

namespace SWAT.Check.Readers;

public class ReadOutputHru : OutputFileReader
{
	public ReadOutputHru(SWATOutputConfig configSettings, string fileName = OutputFileNames.OutputHru) : base(configSettings, fileName)
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

					//For HRU, I don't think space got adjusted but length did.
					int adjustLength = 0;
					int testSub;
					bool noSuccess = true;
                    while (noSuccess)
                    {
                        try
                        {
                            SchemaLine testSchema = new SchemaLine { StartIndex = 4, Length = 5 + adjustLength };
                            testSub = testSchema.GetInt(lines.ToArray()[9]);
                            noSuccess = false;
                        }
                        catch (FormatException)
                        {
                            adjustLength++;
                            if (adjustLength > 15) noSuccess = false;
                        }
                    }

                    OutputHruSchemaInstance outputHruSchema = new OutputHruSchemaInstance(0, adjustLength);

					List<string> headerColumns = new List<string>();
					int i = 1;
					int areaColumnIndex = _configSettings.UseCalendarDateFormat ? outputHruSchema.AreaHeaderIndexWithCalendarDate : outputHruSchema.AreaHeaderIndex;
					int headingsAreaColumnIndex = _configSettings.UseCalendarDateFormat ? OutputHruSchema.AreaHeaderIndexWithCalendarDate : OutputHruSchema.AreaHeaderIndex;
					Dictionary<string, string> headingDictionary = new Dictionary<string, string>();

					int currentYear = _configSettings.SimulationStartOn.Year + _configSettings.SkipYears;
					int numYears = _configSettings.SimulationEndOn.Year - currentYear + 1;

					int numHrus = _configSettings.NumHrusPrinted;

					foreach (string line in lines)
					{
						if (i == OutputHruSchema.HeaderLineNumber)
						{
							int columnIndex = headingsAreaColumnIndex + outputHruSchema.AreaColumnLength; //Area is the last required column, so start reading variable headings after this.
							while (columnIndex < line.Length)
							{
								headerColumns.Add(line.Substring(columnIndex, outputHruSchema.ValuesColumnLength).Trim());
								columnIndex += outputHruSchema.ValuesColumnLength;
							}

							headingDictionary = LoadColumnNamesToHeadingsDictionary(typeof(OutputHru), headerColumns, headingsAreaColumnIndex + outputHruSchema.ValuesColumnLength, OutputHruSchema.ValuesColumnLength);
                            //Add some alternates in case old version still have these
                            headingDictionary.Add("WTAB CLIm", "WTAB_CLI");
                            headingDictionary.Add("WTAB SOLm", "WTAB_SOL");

                            List<string> paramNames = new List<string>();
							List<string> paramValues = new List<string>();
							foreach (string header in headerColumns)
							{
								paramNames.Add(string.Format("`{0}`", headingDictionary[header]));
								paramValues.Add(string.Format("@{0}", headingDictionary[header]));
							}

							cmd.CommandText = string.Format("INSERT INTO OutputHru (`LULC`, `HRU`, `GIS`, `SUB`, `MGT`, `Month`, `Day`, `Year`, `YearSpan`, `Area`, {0}) VALUES (@LULC, @HRU, @GIS, @SUB, @MGT, @Month, @Day, @Year, @YearSpan, @Area, {1});", string.Join(", ", paramNames), string.Join(", ", paramValues));
						}
						else if (i > OutputHruSchema.HeaderLineNumber && !String.IsNullOrWhiteSpace(line))
						{
							cmd.Parameters.Clear();
							int hru = outputHruSchema.HRU.GetInt(line);

							cmd.Parameters.AddWithValue("@LULC", outputHruSchema.LULC.Get(line));
							cmd.Parameters.AddWithValue("@HRU", outputHruSchema.HRU.GetInt(line));
							cmd.Parameters.AddWithValue("@GIS", outputHruSchema.GIS.Get(line));
							cmd.Parameters.AddWithValue("@SUB", outputHruSchema.SUB.GetInt(line));
							cmd.Parameters.AddWithValue("@MGT", outputHruSchema.MGT.GetInt(line));

							switch (_configSettings.PrintCode)
							{
								case SWATPrintSetting.Daily:
									if (_configSettings.UseCalendarDateFormat)
									{
										cmd.Parameters.AddWithValue("@Month", outputHruSchema.MO.GetInt(line));
										cmd.Parameters.AddWithValue("@Day", outputHruSchema.DA.GetInt(line));
										cmd.Parameters.AddWithValue("@Year", outputHruSchema.YR.GetInt(line));
									}
									else
									{
										int julianDay = outputHruSchema.MON.GetInt(line);
										DateTime d = new DateTime(currentYear, 1, 1).AddDays(julianDay - 1);
										cmd.Parameters.AddWithValue("@Month", d.Month);
										cmd.Parameters.AddWithValue("@Day", d.Day);
										cmd.Parameters.AddWithValue("@Year", d.Year);

										if (hru == numHrus && ((DateTime.IsLeapYear(currentYear) && julianDay == 366) || julianDay == 365))
										{
											currentYear++;
										}
									}
									cmd.Parameters.AddWithValue("@YearSpan", 0);
									break;
								case SWATPrintSetting.Monthly:
									cmd.Parameters.AddWithValue("@Day", 0);

									double mon = outputHruSchema.MON.GetDouble(line);

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
											if (hru == numHrus)
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

									double year = outputHruSchema.MON.GetDouble(line);
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
							int columnLength = outputHruSchema.ValuesColumnLength;
							//Possible temporary bug in swat.exe. Values not quite aligned properly in calendar format.
							if (_configSettings.UseCalendarDateFormat)
							{
								columnIndex++;
							}

							cmd.Parameters.AddWithValue("@Area", line.ParseDouble(columnIndex, outputHruSchema.AreaColumnLength));
							columnIndex += outputHruSchema.AreaColumnLength;

							foreach (string heading in headerColumns)
							{
								int extraSpace = 0;
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
