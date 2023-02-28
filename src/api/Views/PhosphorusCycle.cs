using SWAT.Check.Models;

namespace SWAT.Check.Views;

public class PhosphorusCycle
{
	public List<string> Warnings { get; set; }
	public double InitialMinP { get; set; }
	public double FinalMinP { get; set; }
	public double InitialOrgP { get; set; }
	public double FinalOrgP { get; set; }
	public double TotalFertilizerP { get; set; }
	public double InOrgPFertilizer { get; set; }
	public double PlantUptake { get; set; }
	public double OrgPFertilizer { get; set; }
	public double ResidueMineralization { get; set; }
	public double Mineralization { get; set; }
	public double ActiveSolution { get; set; }
	public double StableActive { get; set; }

	public static PhosphorusCycle Get(OutputStd outputStd)
	{
		PhosphorusCycle phosphorusCycle = new PhosphorusCycle
		{
			InitialMinP = outputStd.InitMinP,
			FinalMinP = outputStd.FinalMinP,
			InitialOrgP = outputStd.InitOrgP,
			FinalOrgP = outputStd.FinalOrgP,
			TotalFertilizerP = outputStd.PFertApplied,
			InOrgPFertilizer = outputStd.MineralPInFert,
			PlantUptake = outputStd.PUptake,
			StableActive = outputStd.ActiveToStablePFlow,
			ActiveSolution = outputStd.ActiveToSolutionPFlow,
			Mineralization = outputStd.HumusMinOrgP,
			OrgPFertilizer = outputStd.OrgPInFert,
			ResidueMineralization = outputStd.MinFromFreshOrgP
		};

		//Create warning messages
		List<string> warnings = new List<string>();
		double calc;

		if (phosphorusCycle.InitialMinP != 0)
		{
			calc = (phosphorusCycle.FinalMinP - phosphorusCycle.InitialMinP) / phosphorusCycle.InitialMinP;

			if (calc > 0.5d)
				warnings.Add(string.Format("Mineral P is building up in the soil, the simulation ends with {0:0.0}% more", calc * 100));
			else if (calc < -0.5d)
				warnings.Add(string.Format("Mineral P is being removed from the soil profile, the simulation ends with {0:0.0}% less", calc * -100));
		}

		if (phosphorusCycle.InitialOrgP != 0)
		{
			calc = (phosphorusCycle.FinalOrgP - phosphorusCycle.InitialOrgP) / phosphorusCycle.InitialOrgP;

			if (calc > 0.5d)
				warnings.Add(string.Format("Organic P is building up in the soil, the simulation ends with {0:0.0}% more", calc * 100));
			else if (calc < -0.5d)
				warnings.Add(string.Format("Organic P is being removed from the soil profile, the simulation ends with {0:0.0}% less", calc * -100));
		}

		if (phosphorusCycle.TotalFertilizerP != 0)
		{
			calc = phosphorusCycle.PlantUptake / phosphorusCycle.TotalFertilizerP;

			if (calc < 0.5d)
				warnings.Add("Crop is consuming less than half the amount of applied P");
		}

		phosphorusCycle.Warnings = warnings;

		return phosphorusCycle;
	}
}
