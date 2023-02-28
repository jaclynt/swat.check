namespace SWAT.Check.Models;

public enum SWATPrintSetting
{
    Monthly = 0,
    Daily = 1,
    Yearly = 2
}

public enum WeatherMethod
{
    Measured = 1,
    Simulated = 2
}

public enum FigCommandCode
{
    Finish,
    Subbasin,
    Route,
    RoutRes,
    Transfer,
    Add,
    RecHour,
    RecMon,
    RecYear,
    Save,
    RecDay,
    RecCnst,
    Structure,
    Apex,
    SaveConc,
    AutoCal = 16
}

public enum ExitCode
{
    Success = 0,
    Error = 1
}
