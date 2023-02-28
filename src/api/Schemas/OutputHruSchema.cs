namespace SWAT.Check.Schemas;

/// <summary>
/// Defines line numbers and column indices for data in output.hru as definied in the SWAT 2012 IO Documentation, Chapter 32.4 
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
public static class OutputHruSchema
{
    public const int HeaderLineNumber = 9;
    public const int AreaHeaderIndex = 34;
    public const int AreaHeaderIndexWithCalendarDate = 40;
    public const int AreaValueIndexWithCalendarDate = 41;
    public const int ValuesColumnLength = 10;

    public static SchemaLine LULC = new SchemaLine { StartIndex = 0, Length = 4 };
    public static SchemaLine HRU = new SchemaLine { StartIndex = 4, Length = 5 };
    public static SchemaLine GIS = new SchemaLine { StartIndex = 9, Length = 10 };
    public static SchemaLine SUB = new SchemaLine { StartIndex = 19, Length = 5 };
    public static SchemaLine MGT = new SchemaLine { StartIndex = 24, Length = 5 };
    public static SchemaLine MON = new SchemaLine { StartIndex = 29, Length = 5 };

    public static SchemaLine MO = new SchemaLine { StartIndex = 29, Length = 3 };
    public static SchemaLine DA = new SchemaLine { StartIndex = 32, Length = 3 };
    public static SchemaLine YR = new SchemaLine { StartIndex = 35, Length = 5 };
}

public class OutputHruSchemaInstance
{
    public int HeaderLineNumber { get; set; }
    public int AreaHeaderIndex { get; set; }
    public int AreaHeaderIndexWithCalendarDate { get; set; }
    public int AreaValueIndexWithCalendarDate { get; set; }
    public int ValuesColumnLength { get; set; }

    public SchemaLine LULC { get; set; }
    public SchemaLine HRU { get; set; }
    public SchemaLine GIS { get; set; }
    public SchemaLine SUB { get; set; }
    public SchemaLine MGT { get; set; }
    public SchemaLine MON { get; set; }

    public SchemaLine MO { get; set; }
    public SchemaLine DA { get; set; }
    public SchemaLine YR { get; set; }

    public OutputHruSchemaInstance(int adjustSpace = 0)
    {
        HeaderLineNumber = 9;
        AreaHeaderIndex = 34 + adjustSpace;
        AreaHeaderIndexWithCalendarDate = 40 + adjustSpace;
        AreaValueIndexWithCalendarDate = 41 + adjustSpace;
        ValuesColumnLength = 10;

        LULC = new SchemaLine { StartIndex = 0 + adjustSpace, Length = 4 };
        HRU = new SchemaLine { StartIndex = 4 + adjustSpace, Length = 5 };
        GIS = new SchemaLine { StartIndex = 9 + adjustSpace, Length = 10 };
        SUB = new SchemaLine { StartIndex = 19 + adjustSpace, Length = 5 };
        MGT = new SchemaLine { StartIndex = 24 + adjustSpace, Length = 5 };
        MON = new SchemaLine { StartIndex = 29 + adjustSpace, Length = 5 };

        MO = new SchemaLine { StartIndex = 29 + adjustSpace, Length = 3 };
        DA = new SchemaLine { StartIndex = 32 + adjustSpace, Length = 3 };
        YR = new SchemaLine { StartIndex = 35 + adjustSpace, Length = 5 };
    }
}
