using SWAT.Check.Models;
using System.Data.SQLite;

namespace SWAT.Check.Views;

public class NitrogenCycle
{
	public List<string> Warnings { get; set; }
	public double InitialNO3 { get; set; }
	public double FinalNO3 { get; set; }
	public double InitialOrgN { get; set; }
	public double FinalOrgN { get; set; }
	public double Volatilization { get; set; }
	public double Denitrification { get; set; }
	public double NH4InOrgNFertilizer { get; set; }
	public double NO3InOrgNFertilizer { get; set; }
	public double PlantUptake { get; set; }
	public double Nitrification { get; set; }
	public double Mineralization { get; set; }
	public double TotalFertilizerN { get; set; }
	public double OrgNFertilizer { get; set; }
	public double ActiveToStableOrgN { get; set; }
	public double ResidueMineralization { get; set; }

	public List<dynamic> AvgAnnualByLandUse { get; set; }
	public Dictionary<string, string> AvgAnnualDefinitions { get; set; }

    private static List<string> OutputHruFields = new() { "N_APP", "NAUTO", "NGRZ", "NCFRT", "NRAIN", "NFIX", "F_MN", "A_MN", "A_SN", "DNIT", "NUP", "ORGN", "NSURQ", "NLATQ", "NO3L", "NO3GW" };
	private static Dictionary<string, string> OutHruDefinitions = new()
	{
        { "LULC", "Cover/plant character code" },
        { "N_APP", "Nitrogen fertilizer applied (kg N/ha)" },
        { "NAUTO", "Nitrogen fertilizer auto-applied (kg N/ha)" },
		{ "NGRZ", "Nitrogen applied during grazing operation (kg N/ha)" },
		{ "NCFRT", "Nitrogen applied during continuous fertilizer operation (kg N/ha)" },
		{ "NRAIN", "Nitrate added to soil profile by rain (kg N/ha)" },
		{ "NFIX", "Nitrogen fixation (kg N/ha)" },
		{ "F_MN", "Fresh organic to mineral N (kg N/ha)" },
		{ "A_MN", "Active organic to mineral N (kg N/ha)" },
		{ "A_SN", "Active organic to stable organic N (kg N/ha)" },
		{ "DNIT", "Denitrification (kg N/ha)" },
		{ "NUP", "Plant uptake of nitrogen (kg N/ha)" },
		{ "ORGN", "Organic N yield (kg N/ha)" },
		{ "NSURQ", "NO3 in surface runoff (kg N/ha)" }, 
		{ "NLATQ", "NO3 in lateral flow (kg N/ha)" },
        { "NO3L", "NO3 leached from the soil profile (kg N/ha)" },
        { "NO3GW", "NO3 transported into main channel in the groundwater loading from the HRU (kg N/ha)" }
	};

	public static NitrogenCycle Get(OutputStd outputStd, SQLiteConnection conn, SWATOutputConfig configSettings)
	{
		NitrogenCycle nitrogenCycle = new NitrogenCycle
		{
			InitialNO3 = outputStd.InitNO3,
			FinalNO3 = outputStd.FinalNO3,
			InitialOrgN = outputStd.InitOrgN,
			FinalOrgN = outputStd.FinalOrgN,
			Volatilization = outputStd.AmmoniaVolatilization,
			Denitrification = outputStd.Denitrification,
			NH4InOrgNFertilizer = outputStd.AmmoniaInFert,
			NO3InOrgNFertilizer = outputStd.NO3InFert,
			PlantUptake = outputStd.NUptake,
			Nitrification = outputStd.AmmoniaNitrification,
			Mineralization = outputStd.HumusMinOrgN,
			TotalFertilizerN = outputStd.NFertApplied,
			OrgNFertilizer = outputStd.OrgNInFert,
			ActiveToStableOrgN = outputStd.ActiveToStableOrgN,
			ResidueMineralization = outputStd.MinFromFreshOrgN,
            AvgAnnualDefinitions = OutHruDefinitions
        };

		//Create warning messages
		List<string> warnings = new List<string>();
		double calc;

		if (nitrogenCycle.Denitrification == 0)
			warnings.Add("Denitrification is zero, consider decreasing SDNCO: (Denitrification threshold water content)");
		else if (nitrogenCycle.TotalFertilizerN != 0)
		{
			calc = nitrogenCycle.Denitrification / nitrogenCycle.TotalFertilizerN;

			if (calc < 0.01d)
				warnings.Add("Denitrification is less than 2% of the applied fertilizer amount");
			else if (calc > 0.4d)
				warnings.Add("Denitrification is greater than 25% of the applied fertilizer amount");
		}

		if (nitrogenCycle.TotalFertilizerN != 0)
		{
			calc = nitrogenCycle.Volatilization / nitrogenCycle.TotalFertilizerN;

			if (calc < 0.001d)
				warnings.Add("Ammonia volatilization is less than 0.1% of the applied fertilizer amount");
			else if (calc > 0.38d)
				warnings.Add("Ammonia volatilization is greater than 38% of the applied fertilizer amount");
		}

		if (nitrogenCycle.InitialNO3 != 0)
		{
			calc = (nitrogenCycle.FinalNO3 - nitrogenCycle.InitialNO3) / nitrogenCycle.InitialNO3;

			if (calc > 0.5d)
				warnings.Add(string.Format("Nitrate is building up in the soil, the simulation ends with {0:0.0}% more", calc * 100));
			else if (calc < -0.5d)
				warnings.Add(string.Format("Nitrate is being removed from the soil profile, the simulation ends with {0:0.0}% less", calc * -100));
		}

		if (nitrogenCycle.InitialOrgN != 0)
		{
			calc = (nitrogenCycle.FinalOrgN - nitrogenCycle.InitialOrgN) / nitrogenCycle.InitialOrgN;

			if (calc > 0.5d)
				warnings.Add(string.Format("Organic N is building up in the soil, the simulation ends with {0:0.0}% more", calc * 100));
			else if (calc < -0.5d)
				warnings.Add(string.Format("Organic N is being removed from the soil profile, the simulation ends with {0:0.0}% less", calc * -100));
		}

		if (nitrogenCycle.TotalFertilizerN != 0)
		{
			calc = nitrogenCycle.PlantUptake / nitrogenCycle.TotalFertilizerN;

			if (calc < 0.5d)
				warnings.Add("Crop is consuming less than half the amount of applied N");
		}

		nitrogenCycle.Warnings = warnings;

		nitrogenCycle.AvgAnnualByLandUse = conn.GetOutputHruAvgAnnual(OutputHruFields, configSettings.PrintCode);

        return nitrogenCycle;
	}
}
