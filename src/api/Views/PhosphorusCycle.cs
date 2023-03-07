using SWAT.Check.Models;
using System.Data.SQLite;

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

    public List<dynamic> AvgAnnualByLandUse { get; set; }
    public Dictionary<string, string> AvgAnnualDefinitions { get; set; }

    private static List<string> OutputHruFields = new() { "P_APP", "PAUTO", "PGRZ", "PCFRT", "F_MP", "AO_LP", "L_AP", "A_SP", "PUP", "ORGP", "SEDP", "SOLP", "P_GW" };
    private static Dictionary<string, string> OutHruDefinitions = new()
    {
        { "LULC", "Cover/plant character code" },
        { "P_APP", "Phosphorus fertilizer applied (kg P/ha)" },
		{ "PAUTO", "Phosphorus fertilizer auto-applied (kg P/ha)" },
        { "PGRZ", "Phosphorus applied during grazing operation (kg P/ha)" },
        { "PCFRT", "Phosphorus applied during continuous fertilizer operation (kg P/ha)" },
        { "F_MP", "Fresh organic to mineral P (kg P/ha)" },
        { "AO_LP", "Organic to labile mineral P (kg P/ha)" },
        { "L_AP", "Labile to active mineral P (kg P/ha)" },
        { "A_SP", "Active to stable P (kg P/ha)" },
        { "PUP", "Plant uptake of phosphorus (kg P/ha)" },
        { "ORGP", "Organic P yield (kg P/ha)" },
        { "SEDP", "Sediment P yield (kg P/ha)" },
        { "SOLP", "Soluble P yield (kg P/ha)" },
        { "P_GW", "Soluble phosphorus transported by groundwater flow into main channel during the time step (kg P/ha)" }
    };

    public static PhosphorusCycle Get(OutputStd outputStd, SQLiteConnection conn, SWATOutputConfig configSettings)
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
			ResidueMineralization = outputStd.MinFromFreshOrgP,
            AvgAnnualDefinitions = OutHruDefinitions
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

        phosphorusCycle.AvgAnnualByLandUse = conn.GetOutputHruAvgAnnual(OutputHruFields, configSettings.PrintCode);

        return phosphorusCycle;
	}
}
