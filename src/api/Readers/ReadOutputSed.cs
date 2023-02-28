using SWAT.Check.Helpers;
using SWAT.Check.Models;
using SWAT.Check.Schemas;
using System.Data.SQLite;

namespace SWAT.Check.Readers;

public class ReadOutputSed : OutputFileReader
{
	public ReadOutputSed(SWATOutputConfig configSettings, string fileName = OutputFileNames.OutputSed) : base(configSettings, fileName)
	{
	}

	public override void ReadFile(bool abort)
	{
		if (abort) return;

		var context = new SWATOutputDbContext(_configSettings.DatabaseFile);
		using var conn = context.GetConnection();

		conn.Open();
		using var cmd = new SQLiteCommand(conn);
		using var transaction = conn.BeginTransaction();
		IEnumerable<string> lines = File.ReadLines(_filePath);

		cmd.CommandText = "INSERT INTO OutputSed (`RCH`, `Month`, `Day`, `Year`, `YearSpan`, `AREA`, `SED_IN`, `SED_OUT`, `SAND_IN`, `SAND_OUT`, `SILT_IN`, `SILT_OUT`, `CLAY_IN`, `CLAY_OUT`, `SMAG_IN`, `SMAG_OUT`, `LAG_IN`, `LAG_OUT`, `GRA_IN`, `GRA_OUT`, `CH_BNK`, `CH_BED`, `CH_DEP`, `FP_DEP`, `TSS`) VALUES (@RCH, @Month, @Day, @Year, @YearSpan, @AREA, @SED_IN, @SED_OUT, @SAND_IN, @SAND_OUT, @SILT_IN, @SILT_OUT, @CLAY_IN, @CLAY_OUT, @SMAG_IN, @SMAG_OUT, @LAG_IN, @LAG_OUT, @GRA_IN, @GRA_OUT, @CH_BNK, @CH_BED, @CH_DEP, @FP_DEP, @TSS)";

		int currentYear = _configSettings.SimulationStartOn.Year + _configSettings.SkipYears;
		int numYears = _configSettings.SimulationEndOn.Year - currentYear + 1;
		int numSubbasins = _configSettings.NumSubbasins;

		int i = 1;
		int startAdjust = 0;
		var monLine = OutputSedSchema.MON;
		var rchLine = OutputSedSchema.RCH;
		foreach (string line in lines)
		{
			if (i > OutputSedSchema.HeaderLineNumber && !string.IsNullOrWhiteSpace(line))
			{
				cmd.Parameters.Clear();
				if (i == OutputSedSchema.HeaderLineNumber + 1)
				{
					try
					{
						var testValue = rchLine.GetInt(line);
					}
					catch (FormatException)
					{
						startAdjust = 2;
						monLine = OutputSedSchema.MONAdj;
						rchLine = OutputSedSchema.RCHAdj;
					}
				}

				int rch = rchLine.GetInt(line);

				cmd.Parameters.AddWithValue("@RCH", rch);

				int rowMonth = 0;
				int rowDay = 0;
				int rowYear = 0;

				switch (_configSettings.PrintCode)
				{
					case SWATPrintSetting.Daily:
						int julianDay = monLine.GetInt(line);
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
						cmd.Parameters.AddWithValue("@YearSpan", 0);
						break;
					case SWATPrintSetting.Monthly:
						cmd.Parameters.AddWithValue("@Day", 0);

						double mon = monLine.GetDouble(line);

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

						double year = monLine.GetDouble(line);
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

				int currentIndex = OutputSedSchema.ValuesStartIndex + startAdjust;
				foreach (var field in OutputSedSchema.ValueFields)
				{
					cmd.Parameters.AddWithValue($"@{field}", line.ParseDouble(currentIndex, OutputSedSchema.ValuesColumnLength));
					currentIndex += OutputSedSchema.ValuesColumnLength;
				}

				cmd.ExecuteNonQuery();
			}
			i++;
		}

		transaction.Commit();
		conn.Close();
		GC.Collect();
		GC.WaitForPendingFinalizers();
	}
}
