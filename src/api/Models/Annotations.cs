namespace SWAT.Check.Models;

public class TextFileHeading : Attribute
{
    public TextFileHeading(string value)
    {
        Value = value;
        AltValue = null;
    }

    public TextFileHeading(string value, string altValue)
    {
        Value = value;
        AltValue = altValue;
    }

    public string Value { get; set; }
    public string AltValue { get; set; }
}

public class IgnoreInCsv : Attribute
{
    public IgnoreInCsv()
    {
        Value = true;
    }

    public bool Value { get; set; }
}
