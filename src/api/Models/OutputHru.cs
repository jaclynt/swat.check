namespace SWAT.Check.Models;

public class OutputHruBase
{
	[TextFileHeading("PRECIPmm")]
	public double? PRECIP { get; set; }
	[TextFileHeading("SNOFALLmm")]
	public double? SNOFALL { get; set; }
	[TextFileHeading("SNOMELTmm")]
	public double? SNOMELT { get; set; }
	[TextFileHeading("IRRmm")]
	public double? IRR { get; set; }
	[TextFileHeading("PETmm")]
	public double? PET { get; set; }
	[TextFileHeading("ETmm")]
	public double? ET { get; set; }
	[TextFileHeading("SW_INITmm")]
	public double? SW_INIT { get; set; }
	[TextFileHeading("SW_ENDmm")]
	public double? SW_END { get; set; }
	[TextFileHeading("PERCmm")]
	public double? PERC { get; set; }
	[TextFileHeading("GW_RCHGmm")]
	public double? GW_RCHG { get; set; }
	[TextFileHeading("DA_RCHGmm")]
	public double? DA_RCHG { get; set; }
	[TextFileHeading("REVAPmm")]
	public double? REVAP { get; set; }
	[TextFileHeading("SA_IRRmm")]
	public double? SA_IRR { get; set; }
	[TextFileHeading("DA_IRRmm")]
	public double? DA_IRR { get; set; }
	[TextFileHeading("SA_STmm")]
	public double? SA_ST { get; set; }
	[TextFileHeading("DA_STmm")]
	public double? DA_ST { get; set; }
	[TextFileHeading("SURQ_GENmm")]
	public double? SURQ_GEN { get; set; }
	[TextFileHeading("SURQ_CNTmm")]
	public double? SURQ_CNT { get; set; }
	[TextFileHeading("TLOSSmm")]
	public double? TLOSS { get; set; }
	[TextFileHeading("LATQGENmm")]
	public double? LATQGEN { get; set; }
	[TextFileHeading("GW_Qmm")]
	public double? GW_Q { get; set; }
	[TextFileHeading("WYLDmm")]
	public double? WYLD { get; set; }
	[TextFileHeading("DAILYCN")]
	public double? DAILYCN { get; set; }
	[TextFileHeading("TMP_AVdgC")]
	public double? TMP_AV { get; set; }
	[TextFileHeading("TMP_MXdgC")]
	public double? TMP_MX { get; set; }
	[TextFileHeading("TMP_MNdgC")]
	public double? TMP_MN { get; set; }
	[TextFileHeading("SOL_TMPdgC")]
	public double? CSOL_TMP { get; set; }
	[TextFileHeading("SOLARMJ/m2")]
	public double? CSOLAR { get; set; }
	[TextFileHeading("SYLDt/ha")]
	public double? SYLD { get; set; }
	[TextFileHeading("USLEt/ha")]
	public double? USLE { get; set; }
	[TextFileHeading("N_APPkg/ha")]
	public double? N_APP { get; set; }
	[TextFileHeading("P_APPkg/ha")]
	public double? P_APP { get; set; }
	[TextFileHeading("NAUTOkg/ha")]
	public double? NAUTO { get; set; }
	[TextFileHeading("PAUTOkg/ha")]
	public double? PAUTO { get; set; }
	[TextFileHeading("NGRZkg/ha")]
	public double? NGRZ { get; set; }
	[TextFileHeading("PGRZkg/ha")]
	public double? PGRZ { get; set; }
	[TextFileHeading("NCFRTkg/ha")]
	public double? NCFRT { get; set; }
	[TextFileHeading("PCFRTkg/ha")]
	public double? PCFRT { get; set; }
	[TextFileHeading("NRAINkg/ha")]
	public double? NRAIN { get; set; }
	[TextFileHeading("NFIXkg/ha")]
	public double? NFIX { get; set; }
	[TextFileHeading("F-MNkg/ha")]
	public double? F_MN	{ get; set; }
	[TextFileHeading("A-MNkg/ha")]
	public double? A_MN	{ get; set; }
	[TextFileHeading("A-SNkg/ha")]
	public double? A_SN	{ get; set; }
	[TextFileHeading("F-MPkg/ha")]
	public double? F_MP	{ get; set; }
	[TextFileHeading("AO-LPkg/ha")]
	public double? AO_LP	{ get; set; }
	[TextFileHeading("L-APkg/ha")]
	public double? L_AP	{ get; set; }
	[TextFileHeading("A-SPkg/ha")]
	public double? A_SP	{ get; set; }
	[TextFileHeading("DNITkg/ha")]
	public double? DNIT { get; set; }
	[TextFileHeading("NUPkg/ha")]
	public double? NUP { get; set; }
	[TextFileHeading("PUPkg/ha")]
	public double? PUP { get; set; }
	[TextFileHeading("ORGNkg/ha")]
	public double? ORGN { get; set; }
	[TextFileHeading("ORGPkg/ha")]
	public double? ORGP { get; set; }
	[TextFileHeading("SEDPkg/ha")]
	public double? SEDP { get; set; }
	[TextFileHeading("NSURQkg/ha")]
	public double? NSURQ { get; set; }
	[TextFileHeading("NLATQkg/ha")]
	public double? NLATQ { get; set; }
	[TextFileHeading("NO3Lkg/ha")]
	public double? NO3L { get; set; }
	[TextFileHeading("NO3GWkg/ha")]
	public double? NO3GW { get; set; }
	[TextFileHeading("SOLPkg/ha")]
	public double? SOLP { get; set; }
	[TextFileHeading("P_GWkg/ha")]
	public double? P_GW { get; set; }
	[TextFileHeading("W_STRS")]
	public double? W_STRS { get; set; }
	[TextFileHeading("TMP_STRS")]
	public double? TMP_STRS { get; set; }
	[TextFileHeading("N_STRS")]
	public double? N_STRS { get; set; }
	[TextFileHeading("P_STRS")]
	public double? P_STRS { get; set; }
	[TextFileHeading("BIOMt/ha")]
	public double? BIOM { get; set; }
	[TextFileHeading("LAI")]
	public double? LAI { get; set; }
	[TextFileHeading("YLDt/ha")]
	public double? YLD { get; set; }
	[TextFileHeading("BACTPct")]
	public double? BACTP { get; set; }
	[TextFileHeading("BACTLPct")]
	public double? BACTLP { get; set; }
	[TextFileHeading("WTAB_CLIm", "WTAB CLIm")]
	public double? WTAB_CLI { get; set; }
	[TextFileHeading("WTAB_SOLm", "WTAB SOLm")]
	public double? WTAB_SOL { get; set; }
	[TextFileHeading("SNOmm")]
	public double? SNO { get; set; }
	[TextFileHeading("CMUPkg/ha")]
	public double? CMUP { get; set; }
	[TextFileHeading("CMTOTkg/ha")]
	public double? CMTOT { get; set; }
	[TextFileHeading("QTILEmm")]
	public double? QTILE { get; set; }
	[TextFileHeading("TNO3kg/ha")]
	public double? TNO3 { get; set; }
	[TextFileHeading("LNO3kg/ha")]
	public double? LNO3 { get; set; }
	[TextFileHeading("GW_Q_Dmm")]
	public double? GW_Q_D { get; set; }
	[TextFileHeading("LATQCNTmm")]
	public double? LATQCNT { get; set; }
	[TextFileHeading("TVAPkg/ha")]
	public double? TVAP { get; set; }
}

public class OutputHru : OutputHruBase
{
	public int ID { get; set; }

	public string LULC { get; set; }
	public int HRU { get; set; }
	public string GIS { get; set; }
	public int SUB { get; set; }
	public int MGT { get; set; }
	public int Month { get; set; }
	public int Day { get; set; }
	public int Year { get; set; }
	public double YearSpan { get; set; }
	[TextFileHeading("AREAkm2")]
	public double Area { get; set; }

	public static List<string> DefaultColumns
	{
		get {
			return new List<string> { "PRECIP", "SNOFALL", "SNOMELT", "IRR", "PET", "ET", "SW_INIT", "SW_END", "PERC", "GW_RCHG", "DA_RCHG", "REVAP", "SA_IRR", "DA_IRR", "SA_ST", "DA_ST", "SURQ_GEN", "SURQ_CNT", "TLOSS", "LATQGEN" };
		}
		private set { }
	}
}
