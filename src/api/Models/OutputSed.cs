using Dapper.Contrib.Extensions;

namespace SWAT.Check.Models;

[Table("OutputSed")]
public class OutputSed
{
	public int ID { get; set; }
	public int RCH { get; set; }

	/// <remarks>
	/// Should be set to 0 for annual timesteps and year-span rows.
	/// </remarks>
	public int Month { get; set; }

	/// <remarks>
	/// Should be set to 0 for monthly and annual timesteps.
	/// </remarks>
	public int Day { get; set; }

	/// <remarks>
	/// Should be set to 0 for year-span rows.
	/// </remarks>
	public int Year { get; set; }

	/// <remarks>
	/// If this value is > 0, then Month, Day, and Year fields need to be 0.
	/// </remarks>
	public double YearSpan { get; set; }

	public double AREA { get; set; }
	public double SED_IN { get; set; }
	public double SED_OUT { get; set; }
	public double SAND_IN { get; set; }
	public double SAND_OUT { get; set; }
	public double SILT_IN { get; set; }
	public double SILT_OUT { get; set; }
	public double CLAY_IN { get; set; }
	public double CLAY_OUT { get; set; }
	public double SMAG_IN { get; set; }
	public double SMAG_OUT { get; set; }
	public double LAG_IN { get; set; }
	public double LAG_OUT { get; set; }
	public double GRA_IN { get; set; }
	public double GRA_OUT { get; set; }
	public double CH_BNK { get; set; }
	public double CH_BED { get; set; }
	public double CH_DEP { get; set; }
	public double FP_DEP { get; set; }
	public double TSS { get; set; }
}
