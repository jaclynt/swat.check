using Dapper.Contrib.Extensions;

namespace SWAT.Check.Models;

public class OutputRchBase
{
	[TextFileHeading("FLOW_INcms")]
	public double? FLOW_IN { get; set; }

	[TextFileHeading("FLOW_OUTcms")]
	public double? FLOW_OUT { get; set; }

	[TextFileHeading("EVAPcms")]
	public double? EVAP { get; set; }

	[TextFileHeading("TLOSScms")]
	public double? TLOSS { get; set; }

	[TextFileHeading("SED_INtons")]
	public double? SED_IN { get; set; }

	[TextFileHeading("SED_OUTtons")]
	public double? SED_OUT { get; set; }

	[TextFileHeading("SEDCONCmg/kg", "SEDCONCmg/L")]
	public double? SEDCONC { get; set; }

	[TextFileHeading("ORGN_INkg")]
	public double? ORGN_IN { get; set; }

	[TextFileHeading("ORGN_OUTkg")]
	public double? ORGN_OUT { get; set; }

	[TextFileHeading("ORGP_INkg")]
	public double? ORGP_IN { get; set; }

	[TextFileHeading("ORGP_OUTkg")]
	public double? ORGP_OUT { get; set; }

	[TextFileHeading("NO3_INkg")]
	public double? NO3_IN { get; set; }

	[TextFileHeading("NO3_OUTkg")]
	public double? NO3_OUT { get; set; }

	[TextFileHeading("NH4_INkg")]
	public double? NH4_IN { get; set; }

	[TextFileHeading("NH4_OUTkg")]
	public double? NH4_OUT { get; set; }

	[TextFileHeading("NO2_INkg")]
	public double? NO2_IN { get; set; }

	[TextFileHeading("NO2_OUTkg")]
	public double? NO2_OUT { get; set; }

	[TextFileHeading("MINP_INkg")]
	public double? MINP_IN { get; set; }

	[TextFileHeading("MINP_OUTkg")]
	public double? MINP_OUT { get; set; }

	[TextFileHeading("CHLA_INkg")]
	public double? CHLA_IN { get; set; }

	[TextFileHeading("CHLA_OUTkg")]
	public double? CHLA_OUT { get; set; }

	[TextFileHeading("CBOD_INkg")]
	public double? CBOD_IN { get; set; }

	[TextFileHeading("CBOD_OUTkg")]
	public double? CBOD_OUT { get; set; }

	[TextFileHeading("DISOX_INkg")]
	public double? DISOX_IN { get; set; }

	[TextFileHeading("DISOX_OUTkg")]
	public double? DISOX_OUT { get; set; }

	[TextFileHeading("SOLPST_INmg")]
	public double? SOLPST_IN { get; set; }

	[TextFileHeading("SOLPST_OUTmg")]
	public double? SOLPST_OUT { get; set; }

	[TextFileHeading("SORPST_INmg")]
	public double? SORPST_IN { get; set; }

	[TextFileHeading("SORPST_OUTmg")]
	public double? SORPST_OUT { get; set; }

	[TextFileHeading("REACTPSTmg")]
	public double? REACTPST { get; set; }

	[TextFileHeading("VOLPSTmg")]
	public double? VOLPST { get; set; }

	[TextFileHeading("SETTLPSTmg")]
	public double? SETTLPST { get; set; }

	[TextFileHeading("RESUSP_PSTmg")]
	public double? RESUSP_PST { get; set; }

	[TextFileHeading("DIFFUSEPSTmg")]
	public double? DIFFUSEPST { get; set; }

	[TextFileHeading("REACBEDPSTmg")]
	public double? REACBEDPST { get; set; }

	[TextFileHeading("BURYPSTmg")]
	public double? BURYPST { get; set; }

	[TextFileHeading("BED_PSTmg")]
	public double? BED_PST { get; set; }

	[TextFileHeading("BACTP_OUTct")]
	public double? BACTP_OUT { get; set; }

	[TextFileHeading("BACTLP_OUTct")]
	public double? BACTLP_OUT { get; set; }

	[TextFileHeading("CMETAL#1kg")]
	public double? CMETAL1 { get; set; }

	[TextFileHeading("CMETAL#2kg")]
	public double? CMETAL2 { get; set; }

	[TextFileHeading("CMETAL#3kg")]
	public double? CMETAL3 { get; set; }

	[TextFileHeading("TOT Nkg")]
	public double? TOT_N { get; set; }

	[TextFileHeading("TOT Pkg")]
	public double? TOT_P { get; set; }

	[TextFileHeading("NO3ConcMg/l")]
	public double? NO3CONC { get; set; }

	[TextFileHeading("WTMPdegc")]
	public double? WTMP { get; set; }

	public double? TOT_N_CONC { get; set; }
	public double? TOT_P_CONC { get; set; }
	public double? DISOX_CONC { get; set; }
}

[Table("OutputRch")]
public class OutputRch : OutputRchBase
{
	[IgnoreInCsv]
	public int ID { get; set; }

	[TextFileHeading("RCH")]
	public int RCH { get; set; }

	[TextFileHeading("GIS")]
	[IgnoreInCsv]
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

	public static double GetConcentration(double value, double flowOut, SWATPrintSetting printSetting, int year, int month)
	{
		double additionalTimeFactor = 1d;
		if (printSetting == SWATPrintSetting.Monthly)
		{
			additionalTimeFactor = 30d;
			if (month > 0 && year > 0) additionalTimeFactor = DateTime.DaysInMonth(year, month);
		}
		else if (printSetting == SWATPrintSetting.Yearly)
		{
			additionalTimeFactor = 365d;
			if (year > 0 && DateTime.IsLeapYear(year)) additionalTimeFactor = 366d;
		}

		if (flowOut <= 0)
			return 0;

		return (value * 1000000d) / (flowOut * 86400d * 1000d * additionalTimeFactor);
	}
}
