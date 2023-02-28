using Dapper;
using SWAT.Check.Models;
using System.Data.SQLite;

namespace SWAT.Check.Views;

public class InstreamProcesses
{
	public List<string> Warnings { get; set; }

	public List<Reach> Reaches { get; set; }
	public double UplandSedimentYield { get; set; }
	public double? InstreamSedimentChange { get; set; }
	public double? ChannelErosion { get; set; }
	public double? ChannelDeposition { get; set; }
	public double? TotalN { get; set; }
	public double? TotalP { get; set; }
	public double? TotalStreamflowLosses { get; set; }
	public double? EvaporationLoss { get; set; }
	public double? SeepageLoss { get; set; }

	public static InstreamProcesses Get(SQLiteConnection conn, SWATOutputConfig configSettings, OutputStd outputStd, PointSources pointSources, int sub = 0)
	{
        int simLength = configSettings.SimulationYears - configSettings.SkipYears;
        double? instreamSedimentChange = null;
		double? channelErosion = null;
		double? channelDeposition = null;
		double? totalNRatio = null;
		double? totalPRatio = null;
		double? totalStreamflowLosses = null;
		double? evaporationLosses = null;
		double? seepageLosses = null;

		bool hasRequiredFields = true;
		List<Reach> reaches = new List<Reach>();
		List<string> warnings = new List<string>();

		string groupQueryWhere = string.Empty;
		if (configSettings.PrintCode != SWATPrintSetting.Daily)
		{
			groupQueryWhere = "WHERE `Month` = 0 AND `Year` > 0 ";
		}

		OutputRch firstRow = conn.QuerySingle<OutputRch>("SELECT * FROM OutputRch LIMIT 1");
		int rchCount = conn.QuerySingle<int>("SELECT COUNT(DISTINCT RCH) FROM OutputRch");

		if (firstRow.SED_IN != null && firstRow.SED_OUT != null)
		{
			List<double> sedOutByRch = conn.GetSum("OutputRch", "RCH", "SED_OUT", groupQueryWhere, simLength);
			List<double> sedInByRch = conn.GetSum("OutputRch", "RCH", "SED_IN", groupQueryWhere, simLength);

			instreamSedimentChange = sedOutByRch.Sum() - sedInByRch.Sum();

			double uplandSed = outputStd.TotalSedimentLoading * outputStd.TotalArea * 100d;

			if (instreamSedimentChange > 0)
			{
				channelErosion = 0;
				if (uplandSed > 0)
				{
					channelErosion = instreamSedimentChange / (uplandSed + instreamSedimentChange) * 100d;
				}

				if (channelErosion > 50)
					warnings.Add("More than 50% of sediment is from instream processes");
			}
			else
			{
				channelDeposition = 0;
				if (uplandSed > 0)
				{
					channelDeposition = -instreamSedimentChange / uplandSed * 100d;
				}

				if (channelDeposition > 95)
					warnings.Add("More than 95% of sediment is deposited instream");
			}

			if (channelErosion == 0 && channelDeposition == 0)
			{
				warnings.Add("No in-stream sediment modification; this is unusual");
			}

			if ((channelErosion > -2 && channelErosion < 2) || (channelDeposition > -2 && channelDeposition < 2))
			{
				warnings.Add("Very little in-stream sediment modification (< +-2%); this is unusual");
			}

			for (int i = 0; i < rchCount; i++)
			{
				Reach rch = new Reach
				{
					ID = i + 1
				};

				if (sedInByRch[i] != 0)
					rch.Sediment = sedInByRch[i] > 0 ? sedOutByRch[i] / sedInByRch[i] * 100d : 0;

				reaches.Add(rch);
			}
		}
		else
		{
			hasRequiredFields = false;
		}

		if ((firstRow.NO3_IN != null) && (firstRow.NO3_OUT != null) &&
			(firstRow.NH4_IN != null) && (firstRow.NH4_OUT != null) &&
			(firstRow.NO2_IN != null) && (firstRow.NO2_OUT != null) &&
			(firstRow.ORGN_IN != null) && (firstRow.ORGN_OUT != null))
		{
			List<double> minNOutByRch = conn.GetSum("OutputRch", "RCH", new List<string> { "NO3_OUT", "NH4_OUT", "NO2_OUT" }, groupQueryWhere, simLength);
			List<double> minNInByRch = conn.GetSum("OutputRch", "RCH", new List<string> { "NO3_IN", "NH4_IN", "NO2_IN" }, groupQueryWhere, simLength);
			double minN = minNOutByRch.Sum() - minNInByRch.Sum();

			List<double> orgNOutByRch = conn.GetSum("OutputRch", "RCH", "ORGN_OUT", groupQueryWhere, simLength);
			List<double> orgNInByRch = conn.GetSum("OutputRch", "RCH", "ORGN_IN", groupQueryWhere, simLength);
			double orgN = orgNOutByRch.Sum() - orgNInByRch.Sum();

			if (pointSources.SubbasinLoad.Nitrogen != 0 || pointSources.PointSourceInletLoad.Nitrogen != 0)
				totalNRatio = (minN + orgN) / (pointSources.SubbasinLoad.Nitrogen + pointSources.PointSourceInletLoad.Nitrogen) * 100d;

			if (totalNRatio < -50)
				warnings.Add("Excessive in-stream N modification (loss)");
			else if (totalNRatio > 10)
				warnings.Add("Excessive in-stream N modification (gain)");

			if (hasRequiredFields)
			{
				for (int i = 0; i < rchCount; i++)
				{
					if (orgNInByRch[i] != 0 || minNInByRch[i] != 0)
						reaches[i].Nitrogen = (orgNOutByRch[i] + minNOutByRch[i]) / (orgNInByRch[i] + minNInByRch[i]) * 100d;
				}
			}
		}
		else
		{
			hasRequiredFields = false;
		}

		if ((firstRow.MINP_IN != null) && (firstRow.MINP_OUT != null) && (firstRow.ORGP_IN != null) && (firstRow.ORGP_OUT != null))
		{
			List<double> minPOutByRch = conn.GetSum("OutputRch", "RCH", "MINP_OUT", groupQueryWhere, simLength);
			List<double> minPInByRch = conn.GetSum("OutputRch", "RCH", "MINP_IN", groupQueryWhere, simLength);
			double minP = minPOutByRch.Sum() - minPInByRch.Sum();

			List<double> orgPOutByRch = conn.GetSum("OutputRch", "RCH", "ORGP_OUT", groupQueryWhere, simLength);
			List<double> orgPInByRch = conn.GetSum("OutputRch", "RCH", "ORGP_IN", groupQueryWhere, simLength);
			double orgP = orgPOutByRch.Sum() - orgPInByRch.Sum();

			totalPRatio = pointSources.SubbasinLoad.Phosphorus > 0 || pointSources.PointSourceInletLoad.Phosphorus > 0 ? (minP + orgP) / (pointSources.SubbasinLoad.Phosphorus + pointSources.PointSourceInletLoad.Phosphorus) * 100 : 0;

			if (totalPRatio < -50)
				warnings.Add("Excessive in-stream P modification (loss)");
			else if (totalPRatio > 10)
				warnings.Add("Excessive in-stream P modification (gain)");

			if (hasRequiredFields)
			{
				for (int i = 0; i < rchCount; i++)
				{
					if (orgPInByRch[i] != 0 || minPInByRch[i] != 0)
						reaches[i].Phosphorus = (orgPOutByRch[i] + minPOutByRch[i]) / (orgPInByRch[i] + minPInByRch[i]) * 100d;
				}
			}
		}
		else
		{
			hasRequiredFields = false;
		}

		if ((firstRow.TLOSS != null) && (firstRow.EVAP != null))
		{
			//List<double> tlossByRch = conn.Query<double>("SELECT TLOSS FROM OutputRchStatistic WHERE Month = 0 AND Year = 0 AND Statistic = 'avg'").AsList();
			//List<double> evapByRch = conn.Query<double>("SELECT EVAP FROM OutputRchStatistic WHERE Month = 0 AND Year = 0 AND Statistic = 'avg'").AsList();
            List<double> tlossByRch = conn.GetAvg("OutputRch", "RCH", "TLOSS", groupQueryWhere);
            List<double> evapByRch = conn.GetAvg("OutputRch", "RCH", "EVAP", groupQueryWhere);
            double totalTloss = tlossByRch.Sum() / (outputStd.TotalArea * 1000000d) * 1000d * 60d * 60d * 24d * 365.25d;
			double totalEvap = evapByRch.Sum() / (outputStd.TotalArea * 1000000d) * 1000d * 60d * 60d * 24d * 365.25d;

			double waterYield = outputStd.SurfaceRunoffQ + outputStd.LateralSoilQ + outputStd.GroundWaterQ;

			if (waterYield != 0)
			{
				totalStreamflowLosses = (totalTloss + totalEvap) / waterYield * 100;
				evaporationLosses = totalEvap / waterYield * 100;
				seepageLosses = totalTloss / waterYield * 100;
			}
		}

		if (!hasRequiredFields)
		{
			warnings.Add("Calculations could not be completed for this section because you did not print all reach output variables in File.CIO.");
		}

		return new InstreamProcesses
		{
			UplandSedimentYield = outputStd.TotalSedimentLoading,
			InstreamSedimentChange = outputStd.TotalArea > 0 ? instreamSedimentChange / (outputStd.TotalArea * 100d) : 0,
			ChannelErosion = channelErosion,
			ChannelDeposition = channelDeposition,
			TotalN = totalNRatio,
			TotalP = totalPRatio,
			TotalStreamflowLosses = totalStreamflowLosses,
			EvaporationLoss = evaporationLosses,
			SeepageLoss = seepageLosses,
			Reaches = hasRequiredFields ? reaches : new List<Reach>(),
			Warnings = warnings
		};
	}
}

public class Reach
{
	public int ID { get; set; }
	public double? Sediment { get; set; }
	public double? Phosphorus { get; set; }
	public double? Nitrogen { get; set; }
}
