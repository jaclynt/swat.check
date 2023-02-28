using SWAT.Check.Models;
using System.Reflection;

namespace SWAT.Check.Readers;

public abstract class OutputFileReader
{
    protected string _filePath;
    protected SWATOutputConfig _configSettings;

    public OutputFileReader(SWATOutputConfig configSettings, string fileName)
    {
        _configSettings = configSettings;
        _filePath = Path.Combine(configSettings.FilePath, fileName);
    }

    public abstract void ReadFile(bool abort);

    public Dictionary<string, string> LoadColumnNamesToHeadingsDictionary(Type type, List<string> headerColumns, int startingColumnIndex, int columnLength)
    {
        Dictionary<string, string> headingMap = new Dictionary<string, string>();
        int columnIndex = startingColumnIndex;

        PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (PropertyInfo property in properties)
        {
            if (!property.GetAccessors().Where(a => a.IsVirtual).Any())
            {
                TextFileHeading textHeading = (TextFileHeading)property.GetCustomAttribute(typeof(TextFileHeading));

                if (textHeading != null && headerColumns.Contains(textHeading.Value))
                {
                    headingMap.Add(textHeading.Value, property.Name);
                    columnIndex += columnLength;
                }
                else if (textHeading != null && headerColumns.Contains(textHeading.AltValue))
                {
                    headingMap.Add(textHeading.AltValue, property.Name);
                    columnIndex += columnLength;
                }
            }
        }

        return headingMap;
    }
}
