using Dapper.Contrib.Extensions;

namespace SWAT.Check.Models;

[Table("OutputStdAvgAnnual")]
public class OutputStdAvgAnnual
{
	public int ID { get; set; }

	public int HRU { get; set; }
	public int Sub { get; set; }
	public string Soil { get; set; }
	public double Area { get; set; }
	public double CN { get; set; }
	public double AWC { get; set; }
	public double USLE_LS { get; set; }
	public double IRR { get; set; }
	public double AUTON { get; set; }
	public double AUTOP { get; set; }
	public double MIXEF { get; set; }
	public double PREC { get; set; }
	public double SURQ { get; set; }
	public double GWQ { get; set; }
	public double ET { get; set; }
	public double SED { get; set; }
	public double NO3 { get; set; }
	public double ORGN { get; set; }
	public double BIOM { get; set; }
	public double YLD { get; set; }

	public string LandUse { get; set; }
}
