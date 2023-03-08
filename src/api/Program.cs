using Dapper;
using Dapper.Contrib.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SWAT.Check.Helpers;
using SWAT.Check.Models;
using SWAT.Check.Readers;
using SWAT.Check.Views;
using System.Diagnostics;

string SWAT_CHECK_VERSION = "2.0";

Stopwatch timer = new();
timer.Start();

RunStatus status = new();
WriteStatus(0, "Starting analysis");

if (args.Length < 1)
{
    return ExitWithError("Path to project files required");
}
else 
{
    try
    {
        string projectPath = args[0];
        if (!Directory.Exists(projectPath)) return ExitWithError($"The directory \"{projectPath}\" does not exist on your computer. Please select a valid path to your project files and try again.");

        bool skipDbRead = args.Length > 1 ? args[1] == "1" : false;

        List<string> requiredFiles = new()
        {
            OutputFileNames.FileCio,
            OutputFileNames.FigFig,
            OutputFileNames.OutputStd,
            OutputFileNames.OutputRch,
            OutputFileNames.OutputRsv,
            OutputFileNames.HydOut
        };

        List<string> missing = new();
        foreach (string file in requiredFiles)
        {
            if (!File.Exists(Path.Combine(projectPath, file))) missing.Add(file);
        }

        if (missing.Any()) return ExitWithError($"The directory \"{projectPath}\" does contain the required SWAT files to run SWAT Check. The following files are missing: {string.Join(", ", missing)}. Please verify the directory location is correct and that you have SUCCESSFULLY run the model and try again.");

        WriteStatus(5, "Creating output database, SWATOutput.sqlite");
        string outputDbFile = Path.Combine(projectPath, "SWATOutput.sqlite");
        var outputContext = new SWATOutputDbContext(outputDbFile);
        if (!skipDbRead) outputContext.CreateDatabase();

        bool abort = false;
        SWATOutputConfig outputConfig = new()
        {
            FilePath = projectPath,
            DatabaseFile = outputDbFile,
            NumHrusPrinted = 1
        };

        WriteStatus(7, "Reading file.cio");
        ReadFileCio fileCio = new(outputConfig);
        fileCio.ReadFile(abort);

        outputConfig.SimulationYears = fileCio.NBYR;
        outputConfig.SkipYears = fileCio.NYSKIP;
        outputConfig.PrintCode = (SWATPrintSetting)fileCio.IPRINT;
        outputConfig.PrecipMethod = (WeatherMethod)fileCio.PCPSIM;
        outputConfig.SimulationStartOn = new DateTime(fileCio.IYR, 1, 1).AddDays(fileCio.IDAF - 1);
        outputConfig.SimulationEndOn = new DateTime(fileCio.IYR + fileCio.NBYR - 1, 1, 1).AddDays(fileCio.IDAL - 1);
        outputConfig.UseCalendarDateFormat = fileCio.ICALEN == 1;

        WriteStatus(8, "Reading fig.fig");
        ReadFig fig = new(outputConfig);
        outputConfig.NumSubbasins = fig.ReadNumSubs();
        outputConfig.NumReservoirs= fig.ReadNumReservoirs();

        if (!skipDbRead)
        {
            WriteStatus(10, "Reading output.std");
            new ReadOutputStd(outputConfig).ReadFile(abort);

            WriteStatus(25, "Reading output.rch");
            new ReadOutputRch(outputConfig).ReadFile(abort);

            WriteStatus(50, "Reading output.sub");
            new ReadOutputSub(outputConfig).ReadFile(abort);

            WriteStatus(65, "Reading output.rsv");
            new ReadOutputRsv(outputConfig).ReadFile(abort);

            WriteStatus(75, "Reading hyd.out");
            new ReadHydOut(outputConfig).ReadFile(abort);

            if (File.Exists(Path.Combine(projectPath, OutputFileNames.OutputHru)))
            {
                WriteStatus(80, "Reading output.hru");
                new ReadOutputHru(outputConfig).ReadFile(abort);
            }
        }

        WriteStatus(95, "Loading data for SWAT Check");
        using var conn = outputContext.GetConnection();
        conn.Open();
        conn.Execute("delete from SWATCheckStatus");

        SWATCheckStatus s = new()
        {
            Version = SWAT_CHECK_VERSION,
            LastRunAt = DateTime.Now
        };

        conn.Insert(s);

        var outputStd = conn.Get<OutputStd>(1);
        var numHrus = conn.ExecuteScalar<int>("select count(*) from OutputStdAvgAnnual");

        SWATCheckInstance model = new();
        model.Setup = Setup.Get(outputConfig, outputStd, numHrus);
        model.Hydrology = Hydrology.Get(conn, outputConfig, outputStd);
        model.NitrogenCycle = NitrogenCycle.Get(outputStd, conn, outputConfig);
        model.PhosphorusCycle = PhosphorusCycle.Get(outputStd, conn, outputConfig);
        model.PlantGrowth = PlantGrowth.Get(conn, outputStd);
        model.LandscapeNutrientLosses = LandscapeNutrientLosses.Get(outputStd, model.NitrogenCycle);
        model.LandUseSummary = LandUseSummary.Get(conn);
        model.PointSources = PointSources.Get(conn, outputConfig);
        model.Reservoirs = Reservoirs.Get(conn, outputConfig);
        model.InstreamProcesses = InstreamProcesses.Get(conn, outputConfig, outputStd, model.PointSources);
        model.Sediment = Sediment.Get(conn, outputStd, model.InstreamProcesses, model.PointSources);

        conn.Close();

        string swatCheckFile = Path.Combine(projectPath, "SWATCheck.json");
        using (StreamWriter outfile = new StreamWriter(swatCheckFile))
        {
            await outfile.FlushAsync();
            await outfile.WriteAsync(JsonConvert.SerializeObject(model, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }));
        }

        timer.Stop();
        status.RunTime = timer.Elapsed.TimeSpanToString();
        //status.Data = model;
        return ExitSuccessfully();
    }
    catch (Exception ex)
    {
        timer.Stop();
        string heading = $"Error while {status.Message.FirstCharToLowerCase()}: ";
        return ExitWithError(heading + ex.Message, ErrorUtility.GetExceptionMessages(ex));
    }
}

int ExitWithError(string message, string exception = null)
{
    status.Message = message;
    status.Exception = exception;
    status.Progress = 100;

    Console.Error.WriteLine(JsonConvert.SerializeObject(status, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }));
    return (int)ExitCode.Success;
}

int ExitSuccessfully()
{
    WriteStatus(100, "Complete");
    return (int)ExitCode.Success;
}

void WriteStatus(int progress, string message = null)
{
    status.Progress = progress;
    if (!string.IsNullOrWhiteSpace(message)) status.Message = message;
    Console.WriteLine(JsonConvert.SerializeObject(status, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }));
}

class RunStatus
{
    public int Progress { get; set; }
    public string Message { get; set; }
    public string Exception { get; set; }
    public SWATCheckInstance Data { get; set; }
    public string RunTime { get; set; }
}