using SWAT.Check.Models;

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

	public static NitrogenCycle Get(OutputStd outputStd)
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
			ResidueMineralization = outputStd.MinFromFreshOrgN
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

		return nitrogenCycle;
	}
}
