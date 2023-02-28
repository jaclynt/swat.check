namespace SWAT.Check.Schemas;

/// <summary>
/// Defines line numbers and column indices for data in output.rch. These numbers are defined in the IO docs at: http://swat.tamu.edu/documentation/2012-io/
/// </summary>
/// <remarks>
/// Line numbers start at 1, so will need to subtract by one if finding in an array.
/// Column indices start at 0.
/// </remarks>
public static class OutputRchSchema
{
    public const int HeaderLineNumber = 9;
    public const int AreaHeaderIndex = 25;
    public const int AreaHeaderIndexWithCalendarDate = 31;
    public const int ValuesColumnLength = 12;

    public static SchemaLine RCH = new SchemaLine { StartIndex = 5, Length = 5 };
    public static SchemaLine GIS = new SchemaLine { StartIndex = 10, Length = 9 };
    public static SchemaLine MON = new SchemaLine { StartIndex = 19, Length = 6 };

    public static SchemaLine MO = new SchemaLine { StartIndex = 19, Length = 4 };
    public static SchemaLine DA = new SchemaLine { StartIndex = 23, Length = 3 };
    public static SchemaLine YR = new SchemaLine { StartIndex = 26, Length = 5 };
}

public class OutputRchSchemaInstance
{
    public int HeaderLineNumber { get; set; }
    public int AreaHeaderIndex { get; set; }
    public int AreaHeaderIndexWithCalendarDate { get; set; }
    public int ValuesColumnLength { get; set; }

    public SchemaLine RCH { get; set; }
    public SchemaLine GIS { get; set; }
    public SchemaLine MON { get; set; }

    public SchemaLine MO { get; set; }
    public SchemaLine DA { get; set; }
    public SchemaLine YR { get; set; }

    public OutputRchSchemaInstance(int adjustSpace = 0)
    {
        HeaderLineNumber = 9;
        AreaHeaderIndex = 25 + adjustSpace;
        AreaHeaderIndexWithCalendarDate = 31 + adjustSpace;
        ValuesColumnLength = 12;

        RCH = new SchemaLine { StartIndex = 5 + adjustSpace, Length = 5 };
        GIS = new SchemaLine { StartIndex = 10 + adjustSpace, Length = 9 };
        MON = new SchemaLine { StartIndex = 19 + adjustSpace, Length = 6 };

        MO = new SchemaLine { StartIndex = 19 + adjustSpace, Length = 4 };
        DA = new SchemaLine { StartIndex = 23 + adjustSpace, Length = 3 };
        YR = new SchemaLine { StartIndex = 26 + adjustSpace, Length = 5 };
    }
}
