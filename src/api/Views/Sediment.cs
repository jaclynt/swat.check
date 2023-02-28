using Dapper;
using SWAT.Check.Models;
using System.Data.SQLite;

namespace SWAT.Check.Views;

public class Sediment
{
	public List<string> Warnings { get; set; }
	public double SurfaceRunoff { get; set; }
	public double MaxUplandSedimentYield { get; set; }
	public double AvgUplandSedimentYield { get; set; }
	public double InletSediment { get; set; }
	public double? InStreamSedimentChange { get; set; } //Nullable because may not be available if user did not choose sed in or sed out as an output rch parameter in file.cio

	public static Sediment Get(SQLiteConnection conn, OutputStd outputStd, InstreamProcesses instreamProcesses, PointSources pointSources, int sub = 0)
	{
		Sediment sediment = new Sediment
		{
			SurfaceRunoff = outputStd.SurfaceRunoffQ,
			InStreamSedimentChange = instreamProcesses.InstreamSedimentChange,
			AvgUplandSedimentYield = outputStd.TotalSedimentLoading,
			MaxUplandSedimentYield = conn.QuerySingle<double>("SELECT MAX(SED) FROM OutputStdAvgAnnual"),
			InletSediment = pointSources.PointSourceInletLoad.Sediment
		};

		//Create warning messages
		List<string> warnings = new List<string>();

		if (sediment.AvgUplandSedimentYield > 10)
			warnings.Add("Average sediment yield is greater than 10 metric ton per ha. This is very high for a basin average.");
		else if (sediment.AvgUplandSedimentYield < 0.01d)
			warnings.Add("Average sediment yield is less than 0.01 metric ton per ha. This is very low for a basin average.");

		if (sediment.MaxUplandSedimentYield > 50)
		{
			var maxRow = conn.QuerySingle<OutputStdAvgAnnual>("SELECT * FROM OutputStdAvgAnnual ORDER BY SED DESC LIMIT 1");
			warnings.Add(
				string.Format("Max sediment yield is greater than 50 metric ton per ha in at least one HRU. The highest value is from HRU#: {0}, subbasin#: {1}, crop: {2}, soil: {3}",
					maxRow.HRU, maxRow.Sub, maxRow.LandUse, maxRow.Soil));
		}

		if (sediment.InStreamSedimentChange == null)
			warnings.Add("Instream sediment change could not be computed because you did not print all reach output variables in File.CIO.");

		sediment.Warnings = warnings;

		return sediment;
	}
}
