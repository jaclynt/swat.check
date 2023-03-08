using Dapper;
using SWAT.Check.Models;
using System.Data.SQLite;

namespace SWAT.Check.Views;

public class LandUseSummary
{
	public List<string> Warnings { get; set; }
	public List<string> HruLevelWarnings { get; set; }
	public List<LandUseRow> LandUseRows { get; set; }

	public static LandUseSummary Get(SQLiteConnection conn)
	{
		List<LandUseRow> rows = new List<LandUseRow>();
		List<string> warnings = new List<string>();
		List<string> hruWarnings = new List<string>();

		int hruWarningLimit = 50;
		foreach (var row in conn.Query<OutputStdAvgAnnual>("SELECT * FROM OutputStdAvgAnnual WHERE CN > 98"))
		{
			if (hruWarnings.Count < hruWarningLimit)
			{
				hruWarnings.Add(string.Format("HRU# {0} Subbasin# {1} Crop {2} Soil {3}: curve number may be too high (>98)",
					row.HRU, row.Sub, row.LandUse, row.Soil));
			}
		}

		foreach (var row in conn.Query<OutputStdAvgAnnual>("SELECT * FROM OutputStdAvgAnnual WHERE CN < 35"))
		{
			if (hruWarnings.Count < hruWarningLimit)
			{
				hruWarnings.Add(string.Format("HRU# {0} Subbasin# {1} Crop {2} Soil {3}: curve number may be too low (<35)",
					row.HRU, row.Sub, row.LandUse, row.Soil));
			}
		}

		var groups = conn.Query(@"SELECT LandUse, SUM(Area) AS Area,
				SUM(CN * Area) AS CN,
				SUM(AWC * Area) AS AWC,
				SUM(USLE_LS * Area) AS USLE_LS,
				SUM(IRR * Area) AS IRR,
				SUM(PREC * Area) AS PREC,
				SUM(SURQ * Area) AS SURQ,
				SUM(GWQ * Area) AS GWQ,
				SUM(ET * Area) AS ET,
				SUM(SED * Area) AS SED,
				SUM(NO3 * Area) AS NO3,
				SUM(ORGN * Area) AS ORGN,
				SUM(BIOM * Area) AS BIOM,
				SUM(YLD * Area) AS YLD
				FROM OutputStdAvgAnnual GROUP BY LandUse");

		foreach (var group in groups)
		{
			double totalArea = (double)group.Area;
			LandUseRow row = new LandUseRow
			{
				LandUse = (string)group.LandUse,
				Area = totalArea,
				CN = totalArea > 0 ? (double)group.CN / totalArea : 0,
				AWC = totalArea > 0 ? (double)group.AWC / totalArea : 0,
				USLE_LS = totalArea > 0 ? (double)group.USLE_LS / totalArea : 0,
				IRR = totalArea > 0 ? (double)group.IRR / totalArea : 0,
				PREC = totalArea > 0 ? (double)group.PREC / totalArea : 0,
				SURQ = totalArea > 0 ? (double)group.SURQ / totalArea : 0,
				GWQ = totalArea > 0 ? (double)group.GWQ / totalArea : 0,
				ET = totalArea > 0 ? (double)group.ET / totalArea : 0,
				SED = totalArea > 0 ? (double)group.SED / totalArea : 0,
				NO3 = totalArea > 0 ? (double)group.NO3 / totalArea : 0,
				ORGN = totalArea > 0 ? (double)group.ORGN / totalArea : 0,
				BIOM = totalArea > 0 ? (double)group.BIOM / totalArea : 0,
				YLD = totalArea > 0 ? (double)group.YLD / totalArea : 0
			};

			rows.Add(row);

			if (!string.IsNullOrEmpty(row.LandUse) && !row.LandUse.ToUpper().Equals("WATR"))
			{
				string crop = string.Format("Crop {0}: ", row.LandUse);

				if (row.CN > 95)
					warnings.Add(crop + "curve number may be too high");
				else if (row.CN < 35)
					warnings.Add(crop + "curve number may be too low");

				if (row.AWC > 606 || row.CN > 700)
					warnings.Add(crop + "available water may be too high");
				else if (row.AWC < 41)
					warnings.Add(crop + "available water may be too low");

				if (row.USLE_LS > 16.4d)
					warnings.Add(crop + "USLE LS factor may be too high");
				else if (row.USLE_LS < 0.02d)
					warnings.Add(crop + "USLE LS factor may be too low");

				if (row.SED > 21)
					warnings.Add(crop + "sediment yield may be too high");

				if (row.PREC != 0)
				{
					if (row.ET / row.PREC < 0.31d)
						warnings.Add(crop + "ET less than 31% of irrigation water + precip");
					if (row.ET / (row.PREC + row.IRR) > 0.98d)
						warnings.Add(crop + "ET more than 31% of irrigation water + precip");

					if (row.SURQ / row.PREC > 0.5d)
						warnings.Add(crop + "more than 1/2 precipitation is runoff");
					else if (row.SURQ / row.PREC < 0.01d)
						warnings.Add(crop + "less than 1% of precipitation in runoff");
				}

				double ratio = 0.0129d * row.CN - 0.2857d;
				double eSurq = ratio * 0.26d * row.PREC;

				if (eSurq != 0)
				{
					ratio = row.SURQ / eSurq;

					if (ratio > 1.5d)
						warnings.Add(crop + "surface runoff may be excessive");
					else if (ratio < 0.5d)
						warnings.Add(crop + "surface runoff may be too low");
				}

				if (row.NO3 > 80)
					warnings.Add(string.Format("{0} nitrate yield may be too high {1:0.00} kg/ha", crop, row.NO3));

				if (row.BIOM > 50)
					warnings.Add(string.Format("{0} biomass may be too high {1:0.00} mg/ha", crop, row.BIOM));
				else if (row.BIOM < 1)
					warnings.Add(string.Format("{0} biomass may be too low {1:0.00} mg/ha", crop, row.BIOM));

				if (row.SURQ != 0 || row.GWQ != 0)
				{
					if (row.GWQ / (row.SURQ + row.GWQ) > 0.69d)
						warnings.Add(crop + "more than 69% of water yield is baseflow");
					else if (row.GWQ / (row.SURQ + row.GWQ) < 0.22d)
						warnings.Add(crop + "less than 22% of water yield is baseflow");
				}

				if (row.Area < 0.05d)
					warnings.Add(crop + "HRU area is less than 5 hactares, is this necessary?");

				if (row.PREC > 3400)
					warnings.Add(crop + "precipitation greater than 3400mm/yr");
				else if (row.PREC < 65)
					warnings.Add(crop + "precipitation less than 65mm/yr");
			}
		}

		return new LandUseSummary
		{
			LandUseRows = rows,
			Warnings = warnings,
			HruLevelWarnings = hruWarnings
		};
	}
}

public class LandUseRow
{
	public string LandUse { get; set; }
	public double Area { get; set; }
	public double CN { get; set; }
	public double AWC { get; set; }
	public double USLE_LS { get; set; }
	public double IRR { get; set; }
	public double PREC { get; set; }
	public double SURQ { get; set; }
	public double GWQ { get; set; }
	public double ET { get; set; }
	public double SED { get; set; }
	public double NO3 { get; set; }
	public double ORGN { get; set; }
	public double BIOM { get; set; }
	public double YLD { get; set; }
}
