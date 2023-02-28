using Dapper;
using SWAT.Check.Models;
using System.Data.SQLite;

namespace SWAT.Check.Views;

public class PointSources
{
	public List<string> Warnings { get; set; }
	public PointSourcesLoad SubbasinLoad { get; set; }
	public PointSourcesLoad PointSourceInletLoad { get; set; }
	public PointSourcesLoad FromInletAndPointSource { get; set; }

	public static PointSources Get(SQLiteConnection conn, SWATOutputConfig configSettings, int sub = 0)
	{
		int years = configSettings.SimulationYears;

		var subLines = conn.Query<HydOut>("SELECT * FROM HydOut WHERE ICode = @icode", new { icode = 1 }).AsList();
		var psLines = conn.Query<HydOut>("SELECT * FROM HydOut WHERE ICode IN @icodes", new { icodes = new[] { 6, 7, 8, 10, 11 } }).AsList();

		PointSourcesLoad subLoad = new PointSourcesLoad
		{
			Flow = subLines.Sum(s => years > 0 ? (s.Flow / years / 60d / 60d / 24d / 365.25d) : 0),
			Sediment = subLines.Sum(s => years > 0 ? s.Sed / years : 0),
			Nitrogen = subLines.Sum(s => years > 0 ? s.OrgN / years : 0) + subLines.Sum(s => years > 0 ? s.Nitrate / years : 0),
			Phosphorus = subLines.Sum(s => years > 0 ? s.OrgP / years : 0) + subLines.Sum(s => years > 0 ? s.SolP / years : 0)
		};

		PointSourcesLoad psLoad = new PointSourcesLoad
		{
			Flow = psLines.Sum(s => years > 0 ? s.Flow / years / 60d / 60d / 24d / 365.25d : 0),
			Sediment = psLines.Sum(s => years > 0 ? s.Sed / years : 0),
			Nitrogen = psLines.Sum(s => years > 0 ? s.OrgN / years : 0) + psLines.Sum(s => years > 0 ? s.Nitrate / years : 0),
			Phosphorus = psLines.Sum(s => years > 0 ? s.OrgP / years : 0) + psLines.Sum(s => years > 0 ? s.SolP / years : 0)
		};

		PointSourcesLoad fromLoad = new PointSourcesLoad
		{
			Flow = psLoad.Flow > 0 || subLoad.Flow > 0 ? psLoad.Flow / (psLoad.Flow + subLoad.Flow) * 100d : 0,
			Sediment = psLoad.Sediment > 0 || subLoad.Sediment > 0 ? psLoad.Sediment / (psLoad.Sediment + subLoad.Sediment) * 100d : 0,
			Nitrogen = psLoad.Nitrogen > 0 || subLoad.Nitrogen > 0 ? psLoad.Nitrogen / (psLoad.Nitrogen + subLoad.Nitrogen) * 100d : 0,
			Phosphorus = psLoad.Phosphorus > 0 || subLoad.Phosphorus > 0 ? psLoad.Phosphorus / (psLoad.Phosphorus + subLoad.Phosphorus) * 100d : 0
		};

		List<string> warnings = new List<string>();

		if (fromLoad.Flow > 30)
			warnings.Add("Inlets/point sources contribute more than 30% of the total streamflow");
		if (fromLoad.Phosphorus > 55)
			warnings.Add("Inlets/point sources contribute more than 55% of the total phosphorus");
		if (fromLoad.Nitrogen > 20)
			warnings.Add("Inlets/point sources contribute more than 20% of the total nitrogen");
		if (fromLoad.Sediment > 30)
			warnings.Add("Inlets/point sources contribute more than 30% of the total sediment");

		if (psLoad.Flow == 0 && psLoad.Sediment > 0)
			warnings.Add("Inlets/point sources contribute sediment but not flow, error likely");
		if (psLoad.Flow == 0 && psLoad.Phosphorus > 0)
			warnings.Add("Inlets/point sources contribute phosphorus but not flow, error likely");
		if (psLoad.Flow == 0 && psLoad.Nitrogen > 0)
			warnings.Add("Inlets/point sources contribute nitrogen but not flow, error likely");

		if (psLoad.Flow > 0 && psLoad.Sediment == 0)
			warnings.Add("Inlets/point sources contribute flow, but not sediment");
		if (psLoad.Flow > 0 && psLoad.Phosphorus == 0)
			warnings.Add("Inlets/point sources contribute flow, but not phosphorus");
		if (psLoad.Flow > 0 && psLoad.Nitrogen == 0)
			warnings.Add("Inlets/point sources contribute flow, but not nitrogen");

		if (psLoad.Flow == 0 && psLoad.Nitrogen == 0 && psLoad.Phosphorus == 0 && psLoad.Sediment == 0)
			warnings.Add("Inlets/point source not present");

		if (psLoad.Phosphorus > 0)
		{
			if (psLoad.Nitrogen / psLoad.Phosphorus > 8.8d)
				warnings.Add("Inlets/point sources N:P ratio greater than 8.8");
			else if (psLoad.Nitrogen / psLoad.Phosphorus < 2.8d)
				warnings.Add("Inlets/point sources N:P ratio less than 2.8");
		}

		return new PointSources
		{
			SubbasinLoad = subLoad,
			PointSourceInletLoad = psLoad,
			FromInletAndPointSource = fromLoad,
			Warnings = warnings
		};
	}
}

public class PointSourcesLoad
{
	public double Flow { get; set; }
	public double Sediment { get; set; }
	public double Nitrogen { get; set; }
	public double Phosphorus { get; set; }
}
