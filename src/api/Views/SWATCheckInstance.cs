namespace SWAT.Check.Views;

public class SWATCheckInstance
{
	public Setup Setup { get; set; }
	public Hydrology Hydrology { get; set; }
	public Sediment Sediment { get; set; }
	public NitrogenCycle NitrogenCycle { get; set; }
	public PhosphorusCycle PhosphorusCycle { get; set; }
	public PlantGrowth PlantGrowth { get; set; }
	public LandscapeNutrientLosses LandscapeNutrientLosses { get; set; }
	public LandUseSummary LandUseSummary { get; set; }
	public InstreamProcesses InstreamProcesses { get; set; }
	public PointSources PointSources { get; set; }
	public Reservoirs Reservoirs { get; set; }
}
