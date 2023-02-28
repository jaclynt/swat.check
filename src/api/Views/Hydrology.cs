using Dapper;
using Dapper.Contrib.Extensions;
using SWAT.Check.Models;
using System.Data.SQLite;

namespace SWAT.Check.Views;

public class Hydrology
{
	public List<string> Warnings { get; set; }
	public double ET { get; set; }
	public double PET { get; set; }
	public double Precipitation { get; set; }
	public double AverageCN { get; set; }
	public double SurfaceRunoff { get; set; }
	public double LateralFlow { get; set; }
	public double ReturnFlow { get; set; }
	public double Percolation { get; set; }
	public double Revap { get; set; }
	public double Recharge { get; set; }
	public double StreamflowPrecipitation { get; set; }
	public double BaseflowTotalFlow { get; set; }
	public double SurfaceRunoffTotalFlow { get; set; }
	public double PercolationPrecipitation { get; set; }
	public double DeepRechargePrecipitation { get; set; }
	public double ETPrecipitation { get; set; }
	public List<OutputStdAvgMonBasin> MonthlyBasinValues { get; set; }

	public static Hydrology Get(SQLiteConnection conn, SWATOutputConfig configSettings, OutputStd outputStd, int sub = 0)
	{
		Hydrology hyd = new Hydrology();

		double totalFlow = 0;

		if (sub == 0)
		{
			totalFlow = outputStd.GroundWaterQ + outputStd.LateralSoilQ + outputStd.SurfaceRunoffQ;

			if (totalFlow > 0)
			{
				hyd.BaseflowTotalFlow = (outputStd.GroundWaterQ + outputStd.LateralSoilQ) / totalFlow;
				hyd.SurfaceRunoffTotalFlow = outputStd.SurfaceRunoffQ / totalFlow;
			}

			if (outputStd.Precipitation > 0)
			{
				hyd.StreamflowPrecipitation = totalFlow / outputStd.Precipitation;
				hyd.PercolationPrecipitation = outputStd.TotalAQRecharge / outputStd.Precipitation;
				hyd.DeepRechargePrecipitation = outputStd.DeepAQRecharge / outputStd.Precipitation;
				hyd.ETPrecipitation = outputStd.ET / outputStd.Precipitation;
			}

			hyd.AverageCN = conn.QuerySingle<double>("SELECT SUM(CN * Area) FROM OutputStdAvgAnnual") / outputStd.TotalArea;
			hyd.ET = outputStd.ET;
			hyd.PET = outputStd.PET;
			hyd.Precipitation = outputStd.Precipitation;
			hyd.Revap = outputStd.Revap;
			hyd.Percolation = outputStd.TotalAQRecharge;
			hyd.SurfaceRunoff = outputStd.SurfaceRunoffQ;
			hyd.LateralFlow = outputStd.LateralSoilQ;
			hyd.ReturnFlow = outputStd.GroundWaterQ;
			hyd.Recharge = outputStd.DeepAQRecharge;
			hyd.MonthlyBasinValues = conn.GetAll<OutputStdAvgMonBasin>().AsList();
		}

		//Calculate warnings
		List<string> warnings = new List<string>();

		if (hyd.Precipitation < 65)
			warnings.Add("Precipitation too small. (< 65mm)");
		else if (hyd.Precipitation > 3400)
			warnings.Add("Precipitation may be too high. (> 3400 mm)");

		if (hyd.SurfaceRunoffTotalFlow > 0.78d)
			warnings.Add("Surface runoff ratio may be high (> 0.8)");
		else if (hyd.SurfaceRunoffTotalFlow < 0.31d)
			warnings.Add("Surface runoff ratio may be low (< 0.2)");

		if (totalFlow > 0)
		{
			double gwqRatio = hyd.ReturnFlow / totalFlow;

			if (gwqRatio > 0.69d)
				warnings.Add("Groundwater ratio may be high");
			else if (gwqRatio < 0.22d)
				warnings.Add("Groundwater ratio may be low");
		}

		if (hyd.LateralFlow > hyd.ReturnFlow)
			warnings.Add("Lateral flow is greater than groundwater flow, may indicate a problem");

		if (hyd.ET > hyd.Precipitation)
			warnings.Add("ET Greater than precip, may indicate a problem unless irrigated");

        if (configSettings.SkipYears < 1)
            warnings.Add("It is highly recomended that you use at least 1 year of model warmup. 2-5 years is better");

        if (configSettings.PrecipMethod == WeatherMethod.Simulated)
            warnings.Add("You are using simulated precipitation data, If you intend to calibrate, you should used measured precipitation data");

        //Check hydro using Jimmy Williams equations
        double eWaterYield = 0.26d * hyd.Precipitation;
		double eET = 0.74d * hyd.Precipitation;
		double ratio = 0.0129d * hyd.AverageCN - 0.2857d;
		double eSurq = ratio * eWaterYield;

		if (eWaterYield != 0)
		{
			ratio = totalFlow / eWaterYield;
			if (ratio > 1.5d)
				warnings.Add("Water yield may be excessive");
			else if (ratio < 0.5d)
				warnings.Add("Water yield may be too low");
		}

		if (eSurq != 0)
		{
			ratio = hyd.SurfaceRunoff / eSurq;
			if (ratio > 1.5d)
				warnings.Add("Surface runoff may be excessive");
			else if (ratio < 0.5d)
				warnings.Add("Surface runoff may be too low");
		}

		hyd.Warnings = warnings;

		return hyd;
	}
}
