using Dapper.Contrib.Extensions;

namespace SWAT.Check.Models;

public class OutputSubBase
{
	[TextFileHeading("PRECIPmm")]
	public double? PRECIP { get; set; }

	[TextFileHeading("SNOMELTmm")]
	public double? SNOMELT { get; set; }

	[TextFileHeading("PETmm")]
	public double? PET { get; set; }

	[TextFileHeading("ETmm")]
	public double? ET { get; set; }

	[TextFileHeading("SWmm")]
	public double? SW { get; set; }

	[TextFileHeading("PERCmm")]
	public double? PERC { get; set; }

	[TextFileHeading("SURQmm")]
	public double? SURQ { get; set; }

	[TextFileHeading("GW_Qmm")]
	public double? GW_Q { get; set; }

	[TextFileHeading("WYLDmm")]
	public double? WYLD { get; set; }

	[TextFileHeading("SYLDt/ha")]
	public double? SYLD { get; set; }

	[TextFileHeading("ORGNkg/ha")]
	public double? ORGN { get; set; }

	[TextFileHeading("ORGPkg/ha")]
	public double? ORGP { get; set; }

	[TextFileHeading("NSURQkg/ha")]
	public double? NSURQ { get; set; }

	[TextFileHeading("SOLPkg/ha")]
	public double? SOLP { get; set; }

	[TextFileHeading("SEDPkg/ha")]
	public double? SEDP { get; set; }

	[TextFileHeading("LAT Q(mm)")]
	public double? LATQ { get; set; }

	[TextFileHeading("LATNO3kg/h")]
	public double? LATNO3 { get; set; }

	[TextFileHeading("GWNO3kg/ha")]
	public double? GWNO3 { get; set; }

	[TextFileHeading("CHOLAmic/L")]
	public double? CHOLA { get; set; }

	[TextFileHeading("CBODU mg/L")]
	public double? CBODU { get; set; }

	[TextFileHeading("DOXQ mg/L")]
	public double? DOXQ { get; set; }

	[TextFileHeading("TNO3kg/ha")]
	public double? TNO3 { get; set; }

	[TextFileHeading("QTILEmm")]
	public double? QTILE { get; set; }

	[TextFileHeading("TVAPkg/ha")]
	public double? TVAP { get; set; }
}

[Table("OutputSub")]
public class OutputSub : OutputSubBase
{
	[IgnoreInCsv]
	public int ID { get; set; }

	[TextFileHeading("SUB")]
	public int SUB { get; set; }

	[TextFileHeading("GIS")]
	public int GIS { get; set; }

	/// <remarks>
	/// Should be set to 0 for annual timesteps and year-span rows.
	/// </remarks>
	[IgnoreInCsv]
	public int Month { get; set; }

	/// <remarks>
	/// Should be set to 0 for monthly and annual timesteps.
	/// </remarks>
	[IgnoreInCsv]
	public int Day { get; set; }

	/// <remarks>
	/// Should be set to 0 for year-span rows.
	/// </remarks>
	[IgnoreInCsv]
	public int Year { get; set; }

	/// <remarks>
	/// If this value is > 0, then Month, Day, and Year fields need to be 0.
	/// </remarks>
	[IgnoreInCsv]
	public double YearSpan { get; set; }

	[TextFileHeading("AREAkm2")]
	public double Area { get; set; }
}
