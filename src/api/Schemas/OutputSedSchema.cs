namespace SWAT.Check.Schemas;

public static class OutputSedSchema
{
	public const int HeaderLineNumber = 1;
	public const int ValuesStartIndex = 25;
	public const int ValuesColumnLength = 12;
	public static List<string> ValueFields = new List<string> { "AREA", "SED_IN", "SED_OUT", "SAND_IN", "SAND_OUT", "SILT_IN", "SILT_OUT", "CLAY_IN", "CLAY_OUT", "SMAG_IN", "SMAG_OUT", "LAG_IN", "LAG_OUT", "GRA_IN", "GRA_OUT", "CH_BNK", "CH_BED", "CH_DEP", "FP_DEP", "TSS" };

	public static SchemaLine RCH = new SchemaLine { StartIndex = 5, Length = 5 };
	public static SchemaLine MON = new SchemaLine { StartIndex = 19, Length = 6 };

	public static SchemaLine RCHAdj = new SchemaLine { StartIndex = 5, Length = 7 };
	public static SchemaLine MONAdj = new SchemaLine { StartIndex = 21, Length = 6 };
}
