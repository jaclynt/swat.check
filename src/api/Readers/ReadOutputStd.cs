using Dapper.Contrib.Extensions;
using SWAT.Check.Helpers;
using SWAT.Check.Models;
using SWAT.Check.Schemas;
using System.Data.SQLite;

namespace SWAT.Check.Readers;

public class ReadOutputStd : OutputFileReader
{
	public ReadOutputStd(SWATOutputConfig configSettings, string fileName = OutputFileNames.OutputStd) : base(configSettings, fileName)
	{
	}

	public override void ReadFile(bool abort)
	{
		if (abort) return;

		List<string> remainingLinesList = new List<string>();
		string swatVersion = string.Empty;
		double totalArea = 0;
		Dictionary<int, string> landuses = new Dictionary<int, string>();

		var context = new SWATOutputDbContext(_configSettings.DatabaseFile);
		using (var conn = context.GetConnection())
		{
			conn.Open();

			using (var cmd = new SQLiteCommand(conn))
			{
				using (var transaction = conn.BeginTransaction())
				{
					IEnumerable<string> lines = File.ReadLines(_filePath);

					//The headings changed in SWAT 2012 rev. 549. Need to search for both.
					//Column values appear to be the same though.
					string avgAnnualHeadingsToRev535 = "   HRU  SUB  SOIL       AREAkm2    CN     AWCmm   USLE_LS  IRRmm  AUTONkh AUTOPkh  MIXEF  PRECmm  SURQmm  GWQmm   ETmm     SEDth   NO3kgh  ORGNkgh BIOMth  YLDth";
					string avgAnnualHeadingsToRev549 = "   HRU  SUB  SOIL       AREAkm2    CN     AWCmm   USLE_LS  IRRmm  AUTONkh AUTOPkh  MIXEF PRECmm SURQGENmm   GWQmm    ETmm   SEDth  NO3kgh  ORGNkgh BIOMth   YLDth  SURQmm";
					string endAvgAnnualStartMonthly = "                AVE MONTHLY BASIN VALUES";
					bool currentlyReadingAvgAnnual = false;

					string startPlantValues = "                                            Average Plant Values (kg/ha)";
					string endPlantValues = "    HRU STATISTICS";
					bool currentlyReadingPlantValues = false;

					string endMonthly = "     AVE ANNUAL BASIN STRESS DAYS";
					bool currentlyReadingMonthlyValues = false;
					int monthlyValuesHeadingLine = 0;

					bool currentlyReadingRemainingLines = false;

					int i = 1;
					foreach (string line in lines)
					{
						if (!String.IsNullOrWhiteSpace(line))
						{
							if (currentlyReadingAvgAnnual)
							{
								if (line.Equals(endAvgAnnualStartMonthly))
								{
									currentlyReadingAvgAnnual = false;
									currentlyReadingMonthlyValues = true;
									cmd.CommandText = "INSERT INTO `OutputStdAvgMonBasin` (`Mon`, `Rain`, `SnowFall`, `SurfQ`, `LatQ`, `WaterYield`, `ET`, `SedYield`, `PET`) VALUES (@Mon, @Rain, @SnowFall, @SurfQ, @LatQ, @WaterYield, @ET, @SedYield, @PET);";
								}
								else
								{
									try
									{
										int hru = OutputStdSchema.HRU.GetInt(line);

										cmd.Parameters.Clear();
										cmd.Parameters.AddWithValue("@HRU", hru);
										cmd.Parameters.AddWithValue("@Sub", OutputStdSchema.Sub.GetInt(line));
										cmd.Parameters.AddWithValue("@Soil", OutputStdSchema.Soil.Get(line));
										cmd.Parameters.AddWithValue("@Area", OutputStdSchema.Area.Get(line).ParseDouble());
										cmd.Parameters.AddWithValue("@CN", OutputStdSchema.CN.GetDouble(line, true));
										cmd.Parameters.AddWithValue("@AWC", OutputStdSchema.AWC.GetDouble(line, true));
										cmd.Parameters.AddWithValue("@USLE_LS", OutputStdSchema.USLE_LS.GetDouble(line, true));
										cmd.Parameters.AddWithValue("@IRR", OutputStdSchema.IRR.GetDouble(line, true));
										cmd.Parameters.AddWithValue("@AUTON", OutputStdSchema.AUTON.GetDouble(line, true));
										cmd.Parameters.AddWithValue("@AUTOP", OutputStdSchema.AUTOP.GetDouble(line, true));
										cmd.Parameters.AddWithValue("@MIXEF", OutputStdSchema.MIXEF.GetDouble(line, true));
										cmd.Parameters.AddWithValue("@PREC", OutputStdSchema.PREC.GetDouble(line, true));
										cmd.Parameters.AddWithValue("@SURQ", OutputStdSchema.SURQ.GetDouble(line, true));
										cmd.Parameters.AddWithValue("@GWQ", OutputStdSchema.GWQ.GetDouble(line, true));
										cmd.Parameters.AddWithValue("@ET", OutputStdSchema.ET.GetDouble(line, true));
										cmd.Parameters.AddWithValue("@SED", OutputStdSchema.SED.GetDouble(line, true));
										cmd.Parameters.AddWithValue("@NO3", OutputStdSchema.NO3.GetDouble(line, true));
										cmd.Parameters.AddWithValue("@ORGN", OutputStdSchema.ORGN.GetDouble(line, true));
										cmd.Parameters.AddWithValue("@BIOM", OutputStdSchema.BIOM.GetDouble(line, true));
										cmd.Parameters.AddWithValue("@YLD", OutputStdSchema.YLD.GetDouble(line, true));

										string landuse = string.Empty;
										if (landuses.ContainsKey(hru))
											landuse = landuses[hru];

										cmd.Parameters.AddWithValue("@LandUse", landuse);

										cmd.ExecuteNonQuery();
									}
									catch (Exception)
									{
										//Schema is different, try space delimitted
                                        try
                                        {
                                            var values = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                            int hru = values[0].ParseInt();

                                            cmd.Parameters.Clear();
                                            cmd.Parameters.AddWithValue("@HRU", hru);
                                            cmd.Parameters.AddWithValue("@Sub", values[1].ParseInt());
                                            cmd.Parameters.AddWithValue("@Soil", values[2].Trim());
                                            cmd.Parameters.AddWithValue("@Area", values[3].ParseDouble());
                                            cmd.Parameters.AddWithValue("@CN", values[4].ParseDouble());
                                            cmd.Parameters.AddWithValue("@AWC", values[5].ParseDouble());
                                            cmd.Parameters.AddWithValue("@USLE_LS", values[6].ParseDouble());
                                            cmd.Parameters.AddWithValue("@IRR", values[7].ParseDouble());
                                            cmd.Parameters.AddWithValue("@AUTON", values[8].ParseDouble());
                                            cmd.Parameters.AddWithValue("@AUTOP", values[9].ParseDouble());
                                            cmd.Parameters.AddWithValue("@MIXEF", values[10].ParseDouble());
                                            cmd.Parameters.AddWithValue("@PREC", values[11].ParseDouble());
                                            cmd.Parameters.AddWithValue("@SURQ", values[12].ParseDouble());
                                            cmd.Parameters.AddWithValue("@GWQ", values[13].ParseDouble());
                                            cmd.Parameters.AddWithValue("@ET", values[14].ParseDouble());
                                            cmd.Parameters.AddWithValue("@SED", values[15].ParseDouble());
                                            cmd.Parameters.AddWithValue("@NO3", values[16].ParseDouble());
                                            cmd.Parameters.AddWithValue("@ORGN", values[17].ParseDouble());
                                            cmd.Parameters.AddWithValue("@BIOM", values[18].ParseDouble());
                                            cmd.Parameters.AddWithValue("@YLD", values[19].ParseDouble());

                                            string landuse = string.Empty;
                                            if (landuses.ContainsKey(hru))
                                                landuse = landuses[hru];

                                            cmd.Parameters.AddWithValue("@LandUse", landuse);

                                            cmd.ExecuteNonQuery();
                                        }
                                        catch (Exception)
                                        {
                                            throw new Exception("Error reading output.std average annual values at line " + i);
                                        }
                                    }
								}
							}
							else if (currentlyReadingMonthlyValues)
							{
								if (line.Equals(endMonthly))
								{
									currentlyReadingMonthlyValues = false;
									currentlyReadingRemainingLines = true;
								}
								else if (monthlyValuesHeadingLine < 3)
								{
									monthlyValuesHeadingLine++;
								}
								else
								{
									try
									{
										cmd.Parameters.Clear();
										cmd.Parameters.AddWithValue("@Mon", OutputStdSchema.AMB_Mon.GetInt(line));
										cmd.Parameters.AddWithValue("@Rain", OutputStdSchema.AMB_Rain.GetDouble(line, true));
										cmd.Parameters.AddWithValue("@SnowFall", OutputStdSchema.AMB_SnowFall.GetDouble(line, true));
										cmd.Parameters.AddWithValue("@SurfQ", OutputStdSchema.AMB_SurfQ.GetDouble(line, true));
										cmd.Parameters.AddWithValue("@LatQ", OutputStdSchema.AMB_LatQ.GetDouble(line, true));
										cmd.Parameters.AddWithValue("@WaterYield", OutputStdSchema.AMB_WaterYield.GetDouble(line, true));
										cmd.Parameters.AddWithValue("@ET", OutputStdSchema.AMB_ET.GetDouble(line, true));
										cmd.Parameters.AddWithValue("@SedYield", OutputStdSchema.AMB_SedYield.GetDouble(line, true));
										cmd.Parameters.AddWithValue("@PET", OutputStdSchema.AMB_PET.GetDouble(line, true));
										cmd.ExecuteNonQuery();
									}
									catch (Exception)
									{
                                        //Schema is different, try space delimitted
                                        try
                                        {
                                            var values = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                            cmd.Parameters.Clear();
                                            cmd.Parameters.AddWithValue("@Mon", values[0].ParseInt());
                                            cmd.Parameters.AddWithValue("@Rain", values[1].ParseDouble());
                                            cmd.Parameters.AddWithValue("@SnowFall", values[2].ParseDouble());
                                            cmd.Parameters.AddWithValue("@SurfQ", values[3].ParseDouble());
                                            cmd.Parameters.AddWithValue("@LatQ", values[4].ParseDouble());
                                            cmd.Parameters.AddWithValue("@WaterYield", values[5].ParseDouble());
                                            cmd.Parameters.AddWithValue("@ET", values[6].ParseDouble());
                                            cmd.Parameters.AddWithValue("@SedYield", values[7].ParseDouble());
                                            cmd.Parameters.AddWithValue("@PET", values[8].ParseDouble());
                                            cmd.ExecuteNonQuery();
                                        }
                                        catch (Exception)
                                        {
                                            throw new Exception("Error reading output.std average monthly basin values at line " + i);
                                        }
                                    }
								}
							}
							else if (currentlyReadingPlantValues)
							{
								if (line.Equals(endPlantValues))
								{
									currentlyReadingPlantValues = false;
								}
								else
								{
									if (line.Length > 5 && line.StartsWith("  HRU"))
									{
										/*cmd.Parameters.Clear();
										cmd.Parameters.AddWithValue("@LandUse", OutputStdSchema.LandUse.Get(line));
										cmd.Parameters.AddWithValue("@HRU", OutputStdSchema.HRUForLandUse.GetInt(line));
										cmd.ExecuteNonQuery();*/
										landuses.Add(OutputStdSchema.HRUForLandUse.GetInt(line), OutputStdSchema.LandUse.Get(line));
									}
								}

							}
							else if (currentlyReadingRemainingLines)
							{
								remainingLinesList.Add(line);
							}
							else if (line.Equals(avgAnnualHeadingsToRev535) || line.Equals(avgAnnualHeadingsToRev549))
							{
								currentlyReadingAvgAnnual = true;
								cmd.CommandText = "INSERT INTO `OutputStdAvgAnnual` (`HRU`, `Sub`, `Soil`, `Area`, `CN`, `AWC`, `USLE_LS`, `IRR`, `AUTON`, `AUTOP`, `MIXEF`, `PREC`, `SURQ`, `GWQ`, `ET`, `SED`, `NO3`, `ORGN`, `BIOM`, `YLD`, `LandUse`) VALUES (@HRU, @Sub, @Soil, @Area, @CN, @AWC, @USLE_LS, @IRR, @AUTON, @AUTOP, @MIXEF, @PREC, @SURQ, @GWQ, @ET, @SED, @NO3, @ORGN, @BIOM, @YLD, @LandUse);";
							}
							else if (line.Equals(startPlantValues))
							{
								currentlyReadingPlantValues = true;
								//cmd.CommandText = "UPDATE `OutputStdAvgAnnual` SET `LandUse` = @LandUse WHERE `HRU` = @HRU;";
							}
							else if (i == OutputStdSchema.SWATVersion.LineNumber)
							{
								swatVersion = OutputStdSchema.SWATVersion.Get(line);
							}
							else if (i == OutputStdSchema.TotalArea.LineNumber)
							{
								totalArea = OutputStdSchema.TotalArea.GetDouble(line);
							}
						}

						i++;
					}

					transaction.Commit();
				}
			}

			conn.Close();
		}

		using (var conn = context.GetConnection())
		{
			OutputStd outputStd = new OutputStd();
			outputStd.SWATVersion = swatVersion;
			outputStd.TotalArea = totalArea;

			string[] remainingLines = remainingLinesList.ToArray();
			outputStd.Precipitation = new OutputStdLine("PRECIP =", 22).Get(remainingLines);
			outputStd.SurfaceRunoffQ = new OutputStdLine("SURFACE RUNOFF Q =", 32).Get(remainingLines);
			outputStd.LateralSoilQ = new OutputStdLine("LATERAL SOIL Q =", 30).Get(remainingLines);
			outputStd.GroundWaterQ = new OutputStdLine("GROUNDWATER (SHAL AQ) Q =", 39).Get(remainingLines);
			outputStd.Revap = new OutputStdLine("REVAP (SHAL AQ => SOIL/PLANTS) =", 46).Get(remainingLines);
			outputStd.DeepAQRecharge = new OutputStdLine("DEEP AQ RECHARGE =", 32).Get(remainingLines);
			outputStd.TotalAQRecharge = new OutputStdLine("TOTAL AQ RECHARGE =", 33).Get(remainingLines);
			outputStd.ET = new OutputStdLine(" ET =", 18).Get(remainingLines);
			outputStd.PET = new OutputStdLine(" PET =", 19).Get(remainingLines);

			outputStd.WaterStressDays = new OutputStdLine(" WATER STRESS DAYS =", 34).Get(remainingLines);
			outputStd.TemperatureStressDays = new OutputStdLine(" TEMPERATURE STRESS DAYS =", 40).Get(remainingLines);
			outputStd.NStressDays = new OutputStdLine(" NITROGEN STRESS DAYS =", 37).Get(remainingLines);
			outputStd.PStressDays = new OutputStdLine(" PHOSPHORUS STRESS DAYS =", 39).Get(remainingLines);

			outputStd.InitNO3 = new OutputStdLine("INITIAL NO3 IN SOIL =", 40).Get(remainingLines);
			outputStd.FinalNO3 = new OutputStdLine("FINAL NO3 IN SOIL =", 38).Get(remainingLines);
			outputStd.InitOrgN = new OutputStdLine("INITIAL ORG N IN SOIL =", 42).Get(remainingLines);
			outputStd.FinalOrgN = new OutputStdLine("FINAL ORG N IN SOIL =", 40).Get(remainingLines);

			outputStd.HumusMinOrgN = new OutputStdLine("HUMUS MIN ON ACTIVE ORG N =", 46).Get(remainingLines);
			outputStd.ActiveToStableOrgN = new OutputStdLine("ACTIVE TO STABLE ORG N =", 43).Get(remainingLines);
			outputStd.MinFromFreshOrgN = new OutputStdLine("MIN FROM FRESH ORG N =", 41).Get(remainingLines);
			outputStd.NO3InFert = new OutputStdLine("NO3 IN FERT =", 32).Get(remainingLines);
			outputStd.AmmoniaInFert = new OutputStdLine("AMMONIA IN FERT =", 36).Get(remainingLines);
			outputStd.OrgNInFert = new OutputStdLine("ORG N IN FERT =", 34).Get(remainingLines);
			outputStd.AmmoniaVolatilization = new OutputStdLine("AMMONIA VOLATILIZATION =", 43).Get(remainingLines);

			outputStd.AmmoniaNitrification = new OutputStdLine("AMMONIA NITRIFICATION =", 42).Get(remainingLines);
			outputStd.Denitrification = new OutputStdLine("DENITRIFICATION =", 36).Get(remainingLines);
			outputStd.NUptake = new OutputStdLine("N UPTAKE =", 29).Get(remainingLines);
			outputStd.PUptake = new OutputStdLine("P UPTAKE =", 29).Get(remainingLines);
			outputStd.ActiveToSolutionPFlow = new OutputStdLine("ACTIVE TO SOLUTION P FLOW =", 46).Get(remainingLines);
			outputStd.ActiveToStablePFlow = new OutputStdLine("ACTIVE TO STABLE P FLOW =", 44).Get(remainingLines);
			outputStd.PFertApplied = new OutputStdLine("P FERTILIZER APPLIED =", 41).Get(remainingLines);
			outputStd.NFertApplied = new OutputStdLine("N FERTILIZER APPLIED =", 41).Get(remainingLines);

			outputStd.HumusMinOrgP = new OutputStdLine("HUMUS MIN ON ACTIVE ORG P =", 46).Get(remainingLines);
			outputStd.MinFromFreshOrgP = new OutputStdLine("MIN FROM FRESH ORG P =", 41).Get(remainingLines);
			outputStd.MineralPInFert = new OutputStdLine("MINERAL P IN FERT =", 38).Get(remainingLines);
			outputStd.OrgPInFert = new OutputStdLine("ORG P IN FERT =", 34).Get(remainingLines);
			outputStd.InitMinP = new OutputStdLine("INITIAL MIN P IN SOIL =", 42).Get(remainingLines);
			outputStd.FinalMinP = new OutputStdLine("FINAL MIN P IN SOIL =", 40).Get(remainingLines);
			outputStd.InitOrgP = new OutputStdLine("INITIAL ORG P IN SOIL =", 42).Get(remainingLines);
			outputStd.FinalOrgP = new OutputStdLine("FINAL ORG P IN SOIL =", 40).Get(remainingLines);
			outputStd.OrgN = new OutputStdLine("ORGANIC N =", 30).Get(remainingLines);
			outputStd.OrgP = new OutputStdLine("ORGANIC P =", 30).Get(remainingLines);
			outputStd.NO3YieldSQ = new OutputStdLine("NO3 YIELD (SQ) =", 35).Get(remainingLines);
			outputStd.NO3YieldLat = new OutputStdLine("NO3 YIELD (LAT) =", 36).Get(remainingLines);
			outputStd.SolPYield = new OutputStdLine("SOL P YIELD =", 32).Get(remainingLines);
			outputStd.NO3Leached = new OutputStdLine("NO3 LEACHED =", 32).Get(remainingLines);
			outputStd.NO3YieldGWQ = new OutputStdLine("NO3 YIELD (GWQ) =", 36).Get(remainingLines);
			outputStd.NRemovedInYield = new OutputStdLine("N REMOVED IN YIELD =", 39).Get(remainingLines);
			outputStd.PRemovedInYield = new OutputStdLine("P REMOVED IN YIELD =", 39).Get(remainingLines);
			outputStd.TotalSedimentLoading = new OutputStdLine("TOTAL SEDIMENT LOADING =", 38).Get(remainingLines);

			conn.Insert(outputStd);
		}
	}

	private class OutputStdLine
	{
		public string SearchString { get; set; }
		public int ValueStartIndex { get; set; }

		public OutputStdLine(string searchString, int valueStartIndex)
		{
			SearchString = searchString;
			ValueStartIndex = valueStartIndex;
		}

		public double Get(string[] lines)
		{
			string line = lines.Find(SearchString);
			if (string.IsNullOrWhiteSpace(line)) return 0;
			string value = line.Substring(ValueStartIndex).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];

			value = value.Replace("MM", "");

			if (value.Equals("NaN") || !double.TryParse(value, out double numValue))
			{
				return 0;
			}

			return numValue;
		}
	}
}
