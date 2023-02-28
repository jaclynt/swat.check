namespace SWAT.Check.Schemas;

public class SchemaLine
{
    public const int DefaultStartIndex = 0;
    public const int DefaultFreeFormatItemLength = 16;

    public int StartIndex { get; set; }
    public int Length { get; set; }
    public int LineNumber { get; set; }

    public SchemaLine()
    {
    }

    public SchemaLine(int lineNumber, int startIndex = DefaultStartIndex, int length = DefaultFreeFormatItemLength)
    {
        LineNumber = lineNumber;
        StartIndex = startIndex;
        Length = length;
    }

    public string Get(string line)
    {
        if (StartIndex > line.Length)
            return "BARE";

        return line.Substring(StartIndex, Length).Trim();
    }

    /// <exception cref="FormatException">Will throw an ArgumentException if parsed value is not a double and returnZeroIfNotNumeric parameter is false (default).</exception>
    public double GetDouble(string line, bool returnZeroIfNotNumeric = false)
    {
        string textValue = Get(line);
        double numValue;

        if (textValue.Equals("NaN"))
        {
            return 0;
        }

        if (!double.TryParse(textValue, out numValue))
        {
            if (returnZeroIfNotNumeric)
                return 0;
            else
                throw new FormatException(String.Format("Value \"{0}\" is not a double.", textValue));
        }

        return numValue;
    }

    /// <exception cref="FormatException">Will throw an ArgumentException if parsed value is not an int and returnZeroIfNotNumeric parameter is false (default).</exception>
    public int GetInt(string line, bool returnZeroIfNotNumeric = false)
    {
        string textValue = Get(line);
        int numValue;

        if (!int.TryParse(textValue, out numValue))
        {
            if (returnZeroIfNotNumeric)
                return 0;
            else
                throw new FormatException(String.Format("Value \"{0}\" is not an int.", textValue));
        }

        return numValue;
    }

    public string Get(string[] lines)
    {
        return Get(lines[LineNumber - 1]);
    }

    /// <exception cref="FormatException">Will throw an ArgumentException if parsed value is not a double and returnZeroIfNotNumeric parameter is false (default).</exception>
    public double GetDouble(string[] lines, bool returnZeroIfNotNumeric = false)
    {
        return GetDouble(lines[LineNumber - 1], returnZeroIfNotNumeric);
    }

    /// <exception cref="FormatException">Will throw an ArgumentException if parsed value is not an int and returnZeroIfNotNumeric parameter is false (default).</exception>
    public int GetInt(string[] lines, bool returnZeroIfNotNumeric = false)
    {
        return GetInt(lines[LineNumber - 1], returnZeroIfNotNumeric);
    }
}
