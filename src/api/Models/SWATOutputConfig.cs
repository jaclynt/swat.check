namespace SWAT.Check.Models;

public class SWATOutputConfig
{
    public const int NUM_BULK_INSERT_ROWS = 100;
    public int SimulationYears { get; set; }
    public DateTime SimulationStartOn { get; set; }
    public DateTime SimulationEndOn { get; set; }
    public int SkipYears { get; set; }
    public SWATPrintSetting PrintCode { get; set; }
    public WeatherMethod PrecipMethod { get; set; }
    public bool UseCalendarDateFormat { get; set; }
    public string FilePath { get; set; }
    public string DatabaseFile { get; set; }

    public int NumSubbasins { get; set; }
    public int NumReservoirs { get; set; }
    public int NumHrusPrinted { get; set; }
}
