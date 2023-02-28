using Dapper.Contrib.Extensions;

namespace SWAT.Check.Models;

[Table("OutputRsv")]
public class OutputRsv
{
	public int ID { get; set; }

	[TextFileHeading("RES")]
	public int RES { get; set; }

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

	[TextFileHeading("VOLUMEm3")]
	public double VOLUME { get; set; }

	[TextFileHeading("FLOW_INcms")]
	public double FLOW_IN { get; set; }

	[TextFileHeading("FLOW_OUTcms")]
	public double FLOW_OUT { get; set; }

	[TextFileHeading("PRECIPm3")]
	public double PRECIP { get; set; }

	[TextFileHeading("EVAPm3")]
	public double EVAP { get; set; }

	[TextFileHeading("SEEPAGEm3")]
	public double SEEPAGE { get; set; }

	[TextFileHeading("SED_INtons")]
	public double SED_IN { get; set; }

	[TextFileHeading("SED_OUTtons")]
	public double SED_OUT { get; set; }

	[TextFileHeading("SED_CONCppm")]
	public double SED_CONC { get; set; }

	[TextFileHeading("ORGN_INkg")]
	public double ORGN_IN { get; set; }

	[TextFileHeading("ORGN_OUTkg")]
	public double ORGN_OUT { get; set; }

	[TextFileHeading("RES_ORGNppm")]
	public double RES_ORGN { get; set; }

	[TextFileHeading("ORGP_INkg")]
	public double ORGP_IN { get; set; }

	[TextFileHeading("ORGP_OUTkg")]
	public double ORGP_OUT { get; set; }

	[TextFileHeading("RES_ORGPppm")]
	public double RES_ORGP { get; set; }

	[TextFileHeading("NO3_INkg")]
	public double NO3_IN { get; set; }

	[TextFileHeading("NO3_OUTkg")]
	public double NO3_OUT { get; set; }

	[TextFileHeading("RES_NO3ppm")]
	public double RES_NO3 { get; set; }

	[TextFileHeading("NO2_INkg")]
	public double NO2_IN { get; set; }

	[TextFileHeading("NO2_OUTkg")]
	public double NO2_OUT { get; set; }

	[TextFileHeading("RES_NO2ppm")]
	public double RES_NO2 { get; set; }

	[TextFileHeading("NH3_INkg")]
	public double NH3_IN { get; set; }

	[TextFileHeading("NH3_OUTkg")]
	public double NH3_OUT { get; set; }

	[TextFileHeading("RES_NH3ppm")]
	public double RES_NH3 { get; set; }

	[TextFileHeading("MINP_INkg")]
	public double MINP_IN { get; set; }

	[TextFileHeading("MINP_OUTkg")]
	public double MINP_OUT { get; set; }

	[TextFileHeading("RES_MINPppm")]
	public double RES_MINP { get; set; }

	[TextFileHeading("CHLA_INkg")]
	public double CHLA_IN { get; set; }

	[TextFileHeading("CHLA_OUTkg")]
	public double CHLA_OUT { get; set; }

	[TextFileHeading("SECCHIDEPTHm")]
	public double SECCHIDEPTH { get; set; }

	[TextFileHeading("PEST_INmg")]
	public double PEST_IN { get; set; }

	[TextFileHeading("REACTPSTmg")]
	public double REACTPST { get; set; }

	[TextFileHeading("VOLPSTmg")]
	public double VOLPST { get; set; }

	[TextFileHeading("SETTLPSTmg")]
	public double SETTLPST { get; set; }

	[TextFileHeading("RESUSP_PSTmg")]
	public double RESUSP_PST { get; set; }

	[TextFileHeading("DIFFUSEPSTmg")]
	public double DIFFUSEPST { get; set; }

	[TextFileHeading("REACBEDPSTmg")]
	public double REACBEDPST { get; set; }

	[TextFileHeading("BURYPSTmg")]
	public double BURYPST { get; set; }

	[TextFileHeading("PEST_OUTmg")]
	public double PEST_OUT { get; set; }

	[TextFileHeading("PSTCNCWmg/m3")]
	public double PSTCNCW { get; set; }

	[TextFileHeading("PSTCNCBmg/m3")]
	public double PSTCNCB { get; set; }
}

public class OutputRsvStatistic
{
	public int RES { get; set; }
	public int Total { get; set; }
	public double SED_OUT { get; set; }
	public double SED_IN { get; set; }
	public double N_OUT { get; set; }
	public double N_IN { get; set; }
	public double P_OUT { get; set; }
	public double P_IN { get; set; }
	public double FLOW_OUT { get; set; }
	public double FLOW_IN { get; set; }
	public double VOLUME { get; set; }
	public double SEEPAGE { get; set; }
	public double EVAP { get; set; }
}
