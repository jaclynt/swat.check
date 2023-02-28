using Dapper;
using SWAT.Check.Models;
using System.Data.SQLite;

namespace SWAT.Check.Views;

public class PlantGrowth
{
	public List<string> Warnings { get; set; }
	public double TempStressDays { get; set; }
	public double WaterStressDays { get; set; }
	public double NStressDays { get; set; }
	public double PStressDays { get; set; }
	public double AvgBiomass { get; set; }
	public double AvgYield { get; set; }
	public double NRemoved { get; set; }
	public double PRemoved { get; set; }
	public double TotalFertilizerN { get; set; }
	public double TotalFertilizerP { get; set; }
	public double PlantUptakeN { get; set; }
	public double PlantUptakeP { get; set; }

	public static PlantGrowth Get(SQLiteConnection conn, OutputStd outputStd)
	{
		var sums = conn.QuerySingle("SELECT SUM(BIOM * Area) AS SumBiom, SUM(YLD * Area) AS SumYld FROM OutputStdAvgAnnual");

		PlantGrowth plantGrowth = new PlantGrowth
		{
			TempStressDays = outputStd.TemperatureStressDays,
			WaterStressDays = outputStd.WaterStressDays,
			NStressDays = outputStd.NStressDays,
			PStressDays = outputStd.PStressDays,
			AvgBiomass = outputStd.TotalArea > 0 ? (double)sums.SumBiom / outputStd.TotalArea : 0,
			AvgYield = outputStd.TotalArea > 0 ? (double)sums.SumYld / outputStd.TotalArea : 0,
			NRemoved = outputStd.NRemovedInYield,
			PRemoved = outputStd.PRemovedInYield,
			TotalFertilizerN = outputStd.NFertApplied,
			TotalFertilizerP = outputStd.PFertApplied,
			PlantUptakeN = outputStd.NUptake,
			PlantUptakeP = outputStd.PUptake
		};

		//Create warning messages
		List<string> warnings = new List<string>();

		if (plantGrowth.PStressDays > 60)
			warnings.Add("More than 100 days of phosphorus stress");
		if (plantGrowth.NStressDays > 60)
			warnings.Add("More than 100 days of nitrogen stress");
		if (plantGrowth.WaterStressDays > 80)
			warnings.Add("More than 100 days of water stress");

		if (plantGrowth.PStressDays < 1)
			warnings.Add("Unusually low phosphorus stress");
		if (plantGrowth.NStressDays < 1)
			warnings.Add("Unusually low nitrogen stress");

		if (plantGrowth.AvgYield < 0.5d)
			warnings.Add("Yield may be low if there is harvested crop");
		if (plantGrowth.AvgBiomass < 1)
			warnings.Add("Biomass averages less than 1 metric ton per hectare");

		plantGrowth.Warnings = warnings;

		return plantGrowth;
	}
}
