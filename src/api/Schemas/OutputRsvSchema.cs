namespace SWAT.Check.Schemas;

/// <summary>
/// Defines line numbers and column indices for data in output.rsv as definied in the SWAT 2012 IO Documentation, Chapter 32.7 
/// http://swat.tamu.edu/documentation/2012-io/
/// </summary>
/// <remarks>
/// <para>
/// Line numbers start at 1, so will need to subtract by one if finding in an array.
/// Column indices start at 0.
/// </para>
/// <para>
/// Output.rsv is a fixed width file and cannot be delimited by spaces.
/// </para>
/// </remarks>
public static class OutputRsvSchema
{
	public const int HeaderLineNumber = 9;
	public const int ValuesStartIndex = 19;
	public const int ValuesColumnLength = 12;

	public static SchemaLine RES = new SchemaLine { StartIndex = 6, Length = 8 };
	public static SchemaLine MON = new SchemaLine { StartIndex = 15, Length = 4 };
}
