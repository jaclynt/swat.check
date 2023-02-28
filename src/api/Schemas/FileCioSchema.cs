namespace SWAT.Check.Schemas;

/// <summary>
/// Defines line numbers and column indices for data in file.cio as shown in the SWAT IO Documentation, Chapter 3.
/// http://swat.tamu.edu/documentation/2012-io/
/// </summary>
/// <remarks>
/// Line numbers start at 1, so will need to subtract by one if finding in an array.
/// Column start index starts at 0.
/// </remarks>
public static class FileCioSchema
{
    //There are optional parameters in file.cio, such as ICALEN, so we just need to make sure the file contains at least this many lines.
    public const int MinLines = 60;

    public static SchemaLine NBYR = new SchemaLine(8);
    public static SchemaLine IYR = new SchemaLine(9);
    public static SchemaLine IDAF = new SchemaLine(10);
    public static SchemaLine IDAL = new SchemaLine(11);
    public static SchemaLine PCPSIM = new SchemaLine(14);
    public static SchemaLine NYSKIP = new SchemaLine(60);
    public static SchemaLine IPRINT = new SchemaLine(59);
    public static SchemaLine ICALEN = new SchemaLine(85);
}
