using SWAT.Check.Models;

namespace SWAT.Check.Views;

public class Setup
{
	public int SimulationLength { get; set; }
	public int WarmUp { get; set; }
	public int Hrus { get; set; }
	public int Subbasins { get; set; }
	public string OutputTimestep { get; set; }
	public string PrecipMethod { get; set; }
	public double WatershedArea { get; set; }
	public string SWATVersion { get; set; }

	public static Setup Get(SWATOutputConfig configSettings, OutputStd outputStd, int numHrus)
	{
		return new Setup
		{
            SimulationLength = configSettings.SimulationYears,
            WarmUp = configSettings.SkipYears,
            OutputTimestep = configSettings.PrintCode.ToString(),
            PrecipMethod = configSettings.PrecipMethod.ToString(),
            Hrus = numHrus,
            Subbasins = configSettings.NumSubbasins,
            WatershedArea = outputStd.TotalArea,
            SWATVersion = outputStd.SWATVersion
        };
	}
}
