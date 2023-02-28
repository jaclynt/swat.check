using Dapper;
using SWAT.Check.Models;
using System.Data.SQLite;

namespace SWAT.Check.Views;

public class Reservoirs
{
	public List<string> Warnings { get; set; }
	public List<Reservoir> ReservoirRows { get; set; }
	public AvgTrappingEfficiency AvgTrappingEfficiencies { get; set; }
	public AvgWaterLoss AvgWaterLosses { get; set; }
	public AvgReservoirTrend AvgReservoirTrends { get; set; }

	public static Reservoirs Get(SQLiteConnection conn, SWATOutputConfig configSettings)
	{
		double? ateSediment = null;
		double? ateNitrogen = null;
		double? atePhosphorus = null;
		double? awlTotalRemoved = null;
		double? awlEvaporation = null;
		double? awlSeepage = null;
		double? artMaxVol = null;
		double? artMinVol = null;
		double? artFractionEmpty = null;

		List<Reservoir> reservoirs = new List<Reservoir>();

		List<string> warnings = new List<string>();

		if (conn.QuerySingle<int>("SELECT COUNT(*) FROM OutputRsv") < 1)
			warnings.Add("No output.rsv data available.");
		else
		{
            int initYear = configSettings.SimulationStartOn.Year + configSettings.SkipYears;
            int finalYear = configSettings.SimulationEndOn.Year;

            string volSql = @"SELECT RES, count(*) as Total,
					avg(VOLUME) as VOLUME
					FROM OutputRsv WHERE Year == @year GROUP BY RES";

			var initVolumes = conn.Query(volSql, new { year = initYear }).AsList();
			var finalVolumes = conn.Query(volSql, new { year = finalYear }).AsList();

			if (!finalVolumes.Any())
			{
				int maxYear = conn.QuerySingle<int>("SELECT MAX(Year) FROM OutputRsv");
				finalVolumes = conn.Query(volSql, new { year = maxYear }).AsList();
			}

			var noVolumes = conn.Query(@"SELECT RES, count(*) as Total
					FROM OutputRsv WHERE VOLUME < 1 GROUP BY RES");

			List<double> ratios = new List<double>();
			List<double> emptyVols = new List<double>();

			var query = conn.Query<OutputRsvStatistic>(@"SELECT RES, count(*) as Total,
					avg(SED_OUT) as SED_OUT,
					avg(SED_IN) as SED_IN,
					(sum(ORGN_OUT) + sum(NO3_OUT) + sum(NO2_OUT) + sum(NH3_OUT)) / count(*) as N_OUT,
					(sum(ORGN_IN) + sum(NO3_IN) + sum(NO2_IN) + sum(NH3_IN)) / count(*) as N_IN,
					(sum(ORGP_OUT) + sum(MINP_OUT)) / count(*) as P_OUT,
					(sum(ORGP_IN) + sum(MINP_IN)) / count(*) as P_IN,
					avg(FLOW_OUT) as FLOW_OUT,
					avg(FLOW_IN) as FLOW_IN,
					avg(VOLUME) as VOLUME,
					avg(SEEPAGE) as SEEPAGE,
					avg(EVAP) as EVAP
					FROM OutputRsv GROUP BY RES").AsList();

			foreach (var row in query)
			{
				var initVolume = initVolumes.Where(r => r.RES == row.RES).Any() ? initVolumes.Where(r => r.RES == row.RES).Select(r => (double)r.VOLUME).First() : 0;
				var finalVolume = finalVolumes.Where(r => r.RES == row.RES).Any() ? finalVolumes.Where(r => r.RES == row.RES).Select(r => (double)r.VOLUME).First() : 0;
				var emptyVol = noVolumes.Where(r => r.RES == row.RES).Any() ? noVolumes.Where(r => r.RES == row.RES).Select(r => (int)r.Total).First() : 0;

				double ratio = initVolume == 0 ? 0 : finalVolume / initVolume;
				double emptyFrac = row.Total == 0 ? 0 : emptyVol / row.Total;

				reservoirs.Add(new Reservoir
				{
					ID = row.RES,
					Sediment = GetInOut(row.SED_IN, row.SED_OUT),
					Nitrogen = GetInOut(row.N_IN, row.N_OUT),
					Phosphorus = GetInOut(row.P_IN, row.P_OUT),
					Seepage = row.VOLUME == 0 ? 0 : row.SEEPAGE / row.VOLUME * 100d,
					EvapLoss = row.VOLUME == 0 ? 0 : row.EVAP / row.VOLUME * 100d,
					VolumeRatio = ratio,
					FractionEmpty = emptyFrac
				});

				ratios.Add(ratio);
				emptyVols.Add(emptyFrac);
			}

			double totalSedIn = query.Select(q => q.SED_IN).Sum();
			if (totalSedIn != 0)
				ateSediment = (totalSedIn - query.Select(q => q.SED_OUT).Sum()) / totalSedIn * 100d;

			double totalNIn = query.Select(q => q.N_IN).Sum();
			if (totalNIn != 0)
				ateNitrogen = (totalNIn - query.Select(q => q.N_OUT).Sum()) / totalNIn * 100d;

			double totalPIn = query.Select(q => q.P_IN).Sum();
			if (totalPIn != 0)
				atePhosphorus = (totalPIn - query.Select(q => q.P_OUT).Sum()) / totalPIn * 100d;

			double flowInByResTotal = query.Select(q => q.FLOW_IN).Sum();
			awlTotalRemoved = 0;
			if (flowInByResTotal > 0)
				awlTotalRemoved = (flowInByResTotal - query.Select(q => q.FLOW_OUT).Sum()) / flowInByResTotal * 100d;

			double flowOutCm = GetConvertedFlows(query.Select(q => q.FLOW_OUT).AsList(), configSettings.PrintCode);
			double flowInCm = GetConvertedFlows(query.Select(q => q.FLOW_IN).AsList(), configSettings.PrintCode);

			double totalVolume = query.Select(q => q.VOLUME).Sum();
			if (flowInCm > 0 && totalVolume != 0)
			{
				awlSeepage = query.Select(q => q.SEEPAGE).Sum() / totalVolume * 100d;
				awlEvaporation = query.Select(q => q.EVAP).Sum() / totalVolume * 100d;
			}

			artMaxVol = ratios.Max();
			artMinVol = ratios.Min();
			artFractionEmpty = emptyVols.Max();

			if (artFractionEmpty > 0)
				warnings.Add("At least one of your reservoirs has become complexly dry during the simulation");
			if (artMaxVol > 5)
				warnings.Add("At least one of your reservoirs ends the simulation with at least 500% more volume that it begins with. Check your release parameters.");
			if (artMinVol > 0.2d)
				warnings.Add("At least one of your reservoirs ends the simulation with less than 20% volume that it begins with. Check your release parameters.");

			if (reservoirs.Where(r => r.Sediment < 40).Count() > 0)
				warnings.Add("Sediment trapping efficiency less than 40% at one or more reservoirs");
			if (reservoirs.Where(r => r.Sediment > 98).Count() > 0)
				warnings.Add("Sediment trapping efficiency greater than 98% at one or more reservoirs");

			if (reservoirs.Where(r => r.Nitrogen < 7).Count() > 0)
				warnings.Add("Nitrogen trapping efficiency less than 7% at one or more reservoirs");
			if (reservoirs.Where(r => r.Nitrogen > 72).Count() > 0)
				warnings.Add("Nitrogen trapping efficiency greater than 72% at one or more reservoirs");

			if (reservoirs.Where(r => r.Phosphorus < 18).Count() > 0)
				warnings.Add("Phosphorus trapping efficiency less than 18% at one or more reservoirs");
			if (reservoirs.Where(r => r.Phosphorus > 82).Count() > 0)
				warnings.Add("Phosphorus trapping efficiency greater than 82% at one or more reservoirs");

			if (reservoirs.Where(r => r.EvapLoss < 5).Count() > 0)
				warnings.Add("Evaporation losses are less than 2% at one or more reservoirs");
			if (reservoirs.Where(r => r.EvapLoss > 50).Count() > 0)
				warnings.Add("Evaporation losses are more than 30% at one or more reservoirs");

			if (reservoirs.Where(r => r.Seepage > 25).Count() > 0)
				warnings.Add("Seepage losses are more than 10% at one or more reservoirs");
		}

		return new Reservoirs
		{
			AvgTrappingEfficiencies = new AvgTrappingEfficiency
			{
				Sediment = ateSediment,
				Nitrogen = ateNitrogen,
				Phosphorus = atePhosphorus
			},
			AvgWaterLosses = new AvgWaterLoss
			{
				TotalRemoved = awlTotalRemoved,
				Evaporation = awlEvaporation,
				Seepage = awlSeepage
			},
			AvgReservoirTrends = new AvgReservoirTrend
			{
				NumberReservoirs = reservoirs.Count(),
				FractionEmpty = artFractionEmpty,
				MaxVolume = artMaxVol,
				MinVolume = artMinVol
			},
			ReservoirRows = reservoirs,
			Warnings = warnings
		};
	}

	private static double GetConvertedFlows(List<double> flows, SWATPrintSetting printCode)
	{
		double multiple = 1;
		switch (printCode)
		{
			case SWATPrintSetting.Daily:
				multiple = 24d * 60d * 60d;
				break;
			case SWATPrintSetting.Monthly:
				multiple = 24d * 60d * 60d * 30d;
				break;
			case SWATPrintSetting.Yearly:
				multiple = 24d * 60d * 60d * 365d;
				break;
		}

		return flows.Sum() * multiple;
	}

	private static double GetInOut(double valueIn, double valueOut)
	{
		return valueIn == 0 ? 0 : (valueIn - valueOut) / valueIn * 100d;
	}
}

public class Reservoir
{
	public int ID { get; set; }
	public double? Sediment { get; set; }
	public double? Phosphorus { get; set; }
	public double? Nitrogen { get; set; }
	public double? VolumeRatio { get; set; }
	public double? FractionEmpty { get; set; }
	public double? Seepage { get; set; }
	public double? EvapLoss { get; set; }
}

public class AvgTrappingEfficiency
{
	public double? Sediment { get; set; }
	public double? Phosphorus { get; set; }
	public double? Nitrogen { get; set; }
}

public class AvgWaterLoss
{
	public double? TotalRemoved { get; set; }
	public double? Evaporation { get; set; }
	public double? Seepage { get; set; }
}

public class AvgReservoirTrend
{
	public int NumberReservoirs { get; set; }
	public double? MaxVolume { get; set; }
	public double? MinVolume { get; set; }
	public double? FractionEmpty { get; set; }
}
