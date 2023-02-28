using SWAT.Check.Models;

namespace SWAT.Check.Views;

public class LandscapeNutrientLosses
{
	public List<string> Warnings { get; set; }
	public NitrogenLosses NLosses { get; set; }
	public PhosphorusLosses PLosses { get; set; }

	public static LandscapeNutrientLosses Get(OutputStd outputStd, NitrogenCycle nitrogenCycle)
	{
		LandscapeNutrientLosses landscapeNutrientLosses = new LandscapeNutrientLosses
		{
			NLosses = new NitrogenLosses
			{
				TotalLoss = outputStd.OrgN + outputStd.NO3YieldSQ + outputStd.NO3YieldLat + outputStd.NO3Leached + outputStd.NO3YieldGWQ,
				OrgN = outputStd.OrgN,
				SurfaceRunoff = outputStd.NO3YieldSQ,
				Leached = outputStd.NO3Leached,
				LateralFlow = outputStd.NO3YieldLat,
				GroundwaterYield = outputStd.NO3YieldGWQ,
				SolubilityRatio = 0
			},
			PLosses = new PhosphorusLosses
			{
				TotalLoss = outputStd.OrgP + outputStd.SolPYield,
				OrgP = outputStd.OrgP,
				SurfaceRunoff = outputStd.SolPYield,
				SolubilityRatio = 0
			}
		};

		double totalN = outputStd.OrgN + outputStd.NO3YieldSQ;
		if (totalN != 0)
		{
			landscapeNutrientLosses.NLosses.SolubilityRatio = outputStd.NO3YieldSQ / totalN;
		}

		double totalP = outputStd.OrgP + outputStd.SolPYield;
		if (totalP != 0)
		{
			landscapeNutrientLosses.PLosses.SolubilityRatio = outputStd.SolPYield / totalP;
		}

		//Create warning messages
		List<string> warnings = new List<string>();

		if (landscapeNutrientLosses.NLosses.TotalLoss > 0.4d * nitrogenCycle.TotalFertilizerN)
			warnings.Add("Total nitrogen losses are greater than 40% of applied N");
		else if (landscapeNutrientLosses.NLosses.TotalLoss < 0.1d * nitrogenCycle.TotalFertilizerN)
			warnings.Add("Total nitrogen losses are less than 8% of applied N, may be incorrect in agricultural areas. Likely fine in unmanaged areas or forest dominated watersheds.");

		if (landscapeNutrientLosses.NLosses.SurfaceRunoff > 4.7d)
			warnings.Add("Nitrate losses in surface runoff may be high");
		else if (landscapeNutrientLosses.NLosses.SurfaceRunoff < 0.15d)
			warnings.Add("Nitrate losses in surface runoff may be low");

		if (landscapeNutrientLosses.NLosses.OrgN > 33)
			warnings.Add("Organic/Particulate nitrogen losses in surface runoff may be high");
		else if (landscapeNutrientLosses.NLosses.OrgN < 0.3d)
			warnings.Add("Organic/Particulate nitrogen losses in surface runoff may be low");

		if (landscapeNutrientLosses.PLosses.SurfaceRunoff > 1.2d)
			warnings.Add("Soluble phosphorus losses in surface runoff may be high");
		else if (landscapeNutrientLosses.PLosses.SurfaceRunoff < 0.025d)
			warnings.Add("Soluble phosphorus losses in surface runoff may be low");

		if (landscapeNutrientLosses.PLosses.OrgP > 14)
			warnings.Add("Organic/Particulate phosphorus losses in surface runoff may be high");
		else if (landscapeNutrientLosses.PLosses.OrgP < 0)
			warnings.Add("Organic/Particulate phosphorus losses in surface runoff may be low");

		if (landscapeNutrientLosses.NLosses.SolubilityRatio > 0.85d)
			warnings.Add("Solubility ratio for nitrogen in runoff is high");
		else if (landscapeNutrientLosses.NLosses.SolubilityRatio < 0.1d)
			warnings.Add("Solubility ratio for nitrogen in runoff is low");

		if (landscapeNutrientLosses.PLosses.SolubilityRatio > 0.95d)
			warnings.Add("Solubility ratio for phosphorus in runoff is high, may be ok in uncultivated areas");
		else if (landscapeNutrientLosses.PLosses.SolubilityRatio < 0.13d)
			warnings.Add("Solubility ratio for phosphorus in runoff is low, may indicate a problem");

		if (landscapeNutrientLosses.NLosses.Leached > 50)
			warnings.Add("Nitrate leaching is greater than 50 kg/ha, may indicate a problem.");

		if (nitrogenCycle.TotalFertilizerN != 0)
		{
			double ratio = landscapeNutrientLosses.NLosses.Leached / nitrogenCycle.TotalFertilizerN;

			if (ratio < 0.21d)
				warnings.Add("Nitrate leaching is less than 21% of the applied fertilizer.");
			else if (ratio > 0.38d)
				warnings.Add("Nitrate leaching is greater is more than 38% of the applied fertilizer, may indicate a problem.");
		}

		landscapeNutrientLosses.Warnings = warnings;

		return landscapeNutrientLosses;
	}
}

public class NitrogenLosses
{
	public double TotalLoss { get; set; }
	public double OrgN { get; set; }
	public double SurfaceRunoff { get; set; }
	public double Leached { get; set; }
	public double LateralFlow { get; set; }
	public double GroundwaterYield { get; set; }
	public double SolubilityRatio { get; set; }
}

public class PhosphorusLosses
{
	public double TotalLoss { get; set; }
	public double OrgP { get; set; }
	public double SurfaceRunoff { get; set; }
	public double SolubilityRatio { get; set; }
}
