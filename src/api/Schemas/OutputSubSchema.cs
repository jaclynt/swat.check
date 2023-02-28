namespace SWAT.Check.Schemas;

/// <summary>
/// Defines line numbers and column indices for data in output.sub as definied in the SWAT 2012 IO Documentation, Chapter 32.4 
/// http://swat.tamu.edu/documentation/2012-io/
/// </summary>
/// <remarks>
/// <para>
/// Line numbers start at 1, so will need to subtract by one if finding in an array.
/// Column indices start at 0.
/// </para>
/// <para>
/// Output.sub is a fixed width file and cannot be delimited by spaces. All columns through AREA are required, but after this the user
/// may specify which variables to print in their file.cio.
/// </para>
/// <para>
/// For daily print output there are two date formatting options: calendar date with MO DA YR columns, or the julian day in the MON column.
/// This is defined by ICALEN in file.cio.
/// There is an issue with the headers and values of daily calendar date format being off by 1, so it needs to be accounted for.
/// </para>
/// </remarks>
public static class OutputSubSchema
{
	public const int HeaderLineNumber = 9;
	public const int AreaHeaderIndex = 24;
	public const int AreaHeaderIndexWithCalendarDate = 30;
	public const int AreaValueIndexWithCalendarDate = 31;
	public const int ValuesColumnLength = 10;

	public static SchemaLine SUB = new SchemaLine { StartIndex = 6, Length = 4 };
	public static SchemaLine GIS = new SchemaLine { StartIndex = 10, Length = 9 };
	public static SchemaLine MON = new SchemaLine { StartIndex = 19, Length = 5 };

	public static SchemaLine MO = new SchemaLine { StartIndex = 19, Length = 3 };
	public static SchemaLine DA = new SchemaLine { StartIndex = 22, Length = 3 };
	public static SchemaLine YR = new SchemaLine { StartIndex = 25, Length = 5 };
}

public class OutputSubSchemaInstance
{
	public int HeaderLineNumber { get; set; }
	public int AreaHeaderIndex { get; set; }
	public int AreaHeaderIndexWithCalendarDate { get; set; }
	public int AreaValueIndexWithCalendarDate { get; set; }
	public int ValuesColumnLength { get; set; }

	public SchemaLine SUB { get; set; }
	public SchemaLine GIS { get; set; }
	public SchemaLine MON { get; set; }

	public SchemaLine MO { get; set; }
	public SchemaLine DA { get; set; }
	public SchemaLine YR { get; set; }

	public OutputSubSchemaInstance(int adjustSpace = 0)
	{
		HeaderLineNumber = 9;
		AreaHeaderIndex = 24 + adjustSpace;
		AreaHeaderIndexWithCalendarDate = 30 + adjustSpace;
		AreaValueIndexWithCalendarDate = 31 + adjustSpace;
		ValuesColumnLength = 10;

		SUB = new SchemaLine { StartIndex = 6 + adjustSpace, Length = 4 };
		GIS = new SchemaLine { StartIndex = 10 + adjustSpace, Length = 9 };
		MON = new SchemaLine { StartIndex = 19 + adjustSpace, Length = 5 };

		MO = new SchemaLine { StartIndex = 19 + adjustSpace, Length = 3 };
		DA = new SchemaLine { StartIndex = 22 + adjustSpace, Length = 3 };
		YR = new SchemaLine { StartIndex = 25 + adjustSpace, Length = 5 };
	}
}
