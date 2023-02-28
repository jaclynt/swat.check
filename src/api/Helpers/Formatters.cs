using System.Globalization;

namespace SWAT.Check.Helpers;

public static class Formatters
{
    public static double ParseDouble(this string line, int startIndex, int length)
    {
        return line.Substring(startIndex, length).Trim().ParseDouble();
    }

    public static double ParseDouble(this string value)
    {
        if (value.Equals("NaN"))
        {
            return 0;
        }

        double number;
        double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out number);

        return number;
    }

    public static string Find(this string[] lines, string value)
    {
        return lines.Where(l => l.Contains(value)).SingleOrDefault();
    }

    public static string TimeSpanToString(this TimeSpan ts)
    {
        double minutes = ts.TotalMinutes;
        double hours = ts.TotalHours;

        string daysStr = ts.Days == 1 ? "day" : "days";
        string hoursStr = ts.Hours == 1 ? "hour" : "hours";
        string minutesStr = ts.Minutes == 1 ? "minute" : "minutes";
        string secondsStr = ts.Seconds == 1 ? "second" : "seconds";

        string description = $"{ts.Minutes} {minutesStr}";
        if (minutes < 1)
        {
            description = $"{ts.Seconds} {secondsStr}";
        }
        else if (minutes > 60)
        {
            description = $"{ts.Hours} {hoursStr} and {ts.Minutes} {minutesStr}";
            if (hours > 24)
            {
                description = $"{ts.Days} {daysStr}, {description}";
            }
        }

        return description;
    }

    public static string FirstCharToLowerCase(this string str)
    {
        if (!string.IsNullOrEmpty(str) && char.IsUpper(str[0]))
            return str.Length == 1 ? char.ToLower(str[0]).ToString() : char.ToLower(str[0]) + str[1..];

        return str;
    }
}
