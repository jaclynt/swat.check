namespace SWAT.Check.Schemas;

public class OutputStdSchema
{
	public static SchemaLine SWATVersion = new SchemaLine { StartIndex = 0, Length = 70, LineNumber = 2 };
	public static SchemaLine TotalArea = new SchemaLine { StartIndex = 27, Length = 14, LineNumber = 8 };
	public static SchemaLine HRU = new SchemaLine { StartIndex = 0, Length = 7 };
	public static SchemaLine Sub = new SchemaLine { StartIndex = 7, Length = 4 };
	public static SchemaLine Soil = new SchemaLine { StartIndex = 11, Length = 14 };
	public static SchemaLine Area = new SchemaLine { StartIndex = 25, Length = 8 };
	public static SchemaLine CN = new SchemaLine { StartIndex = 36, Length = 5 };
	public static SchemaLine AWC = new SchemaLine { StartIndex = 41, Length = 8 };
	public static SchemaLine USLE_LS = new SchemaLine { StartIndex = 49, Length = 8 };
	public static SchemaLine IRR = new SchemaLine { StartIndex = 57, Length = 8 };
	public static SchemaLine AUTON = new SchemaLine { StartIndex = 65, Length = 8 };
	public static SchemaLine AUTOP = new SchemaLine { StartIndex = 73, Length = 8 };
	public static SchemaLine MIXEF = new SchemaLine { StartIndex = 81, Length = 8 };
	public static SchemaLine PREC = new SchemaLine { StartIndex = 89, Length = 8 };
	public static SchemaLine SURQ = new SchemaLine { StartIndex = 97, Length = 8 };
	public static SchemaLine GWQ = new SchemaLine { StartIndex = 105, Length = 8 };
	public static SchemaLine ET = new SchemaLine { StartIndex = 113, Length = 8 };
	public static SchemaLine SED = new SchemaLine { StartIndex = 121, Length = 8 };
	public static SchemaLine NO3 = new SchemaLine { StartIndex = 129, Length = 8 };
	public static SchemaLine ORGN = new SchemaLine { StartIndex = 137, Length = 8 };
	public static SchemaLine BIOM = new SchemaLine { StartIndex = 145, Length = 8 };
	public static SchemaLine YLD = new SchemaLine { StartIndex = 153, Length = 8 };
	public static SchemaLine LandUse = new SchemaLine { StartIndex = 22, Length = 6 };
	public static SchemaLine HRUForLandUse = new SchemaLine { StartIndex = 5, Length = 8 };

	public static SchemaLine AMB_Mon = new SchemaLine { StartIndex = 0, Length = 5 };
	public static SchemaLine AMB_Rain = new SchemaLine { StartIndex = 5, Length = 9 };
	public static SchemaLine AMB_SnowFall = new SchemaLine { StartIndex = 14, Length = 9 };
	public static SchemaLine AMB_SurfQ = new SchemaLine { StartIndex = 23, Length = 9 };
	public static SchemaLine AMB_LatQ = new SchemaLine { StartIndex = 32, Length = 9 };
	public static SchemaLine AMB_WaterYield = new SchemaLine { StartIndex = 41, Length = 9 };
	public static SchemaLine AMB_ET = new SchemaLine { StartIndex = 50, Length = 9 };
	public static SchemaLine AMB_SedYield = new SchemaLine { StartIndex = 59, Length = 9 };
	public static SchemaLine AMB_PET = new SchemaLine { StartIndex = 68, Length = 9 };
}
