using SWAT.Check.Models;
using SWAT.Check.Schemas;

namespace SWAT.Check.Readers;

public class ReadFig : OutputFileReader
{
    public ReadFig(SWATOutputConfig configSettings, string fileName = OutputFileNames.FigFig) : base(configSettings, fileName)
    {
    }

    public int ReadNumSubs()
    {
        IEnumerable<string> lines = File.ReadLines(_filePath);
        bool readCommand = true;
        int numSubs = 0;
        foreach (string line in lines)
        {
            int command = -1;
            if (readCommand)
            {
                command = FigSchema.Command.GetInt(line);
            }

            if (command == (int)FigCommandCode.Subbasin)
            {
                numSubs++;
                readCommand = false;
            }
            else if (command == -1)
            {
                readCommand = true;
            }
            else
            {
                break;
            }
        }

        return numSubs;
    }

    public int ReadNumReservoirs()
    {
        IEnumerable<string> lines = File.ReadLines(_filePath);
        int numRes = 0;
        foreach (string line in lines)
        {
            if (line.StartsWith("routres"))
                numRes++;
        }

        return numRes;
    }

    public override void ReadFile(bool abort)
    {
        if (abort) return;
        throw new NotImplementedException("This method is not implemented for fig.fig.");
    }
}
