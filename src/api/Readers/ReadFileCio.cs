using SWAT.Check.Models;
using SWAT.Check.Schemas;

namespace SWAT.Check.Readers;

public class ReadFileCio : OutputFileReader
{
    public int NBYR { get; set; }
    public int IYR { get; set; }
    public int IDAF { get; set; }
    public int IDAL { get; set; }
    public int NYSKIP { get; set; }
    public int PCPSIM { get; set; }
    public int IPRINT { get; set; }
    public int ICALEN { get; set; }

    public ReadFileCio(SWATOutputConfig configSettings, string fileName = OutputFileNames.FileCio) : base(configSettings, fileName)
    {
    }

    public override void ReadFile(bool abort)
    {
        if (abort) return;
        string[] lines = File.ReadAllLines(_filePath);

        if (lines.Length < FileCioSchema.MinLines)
            throw new ArgumentOutOfRangeException("Your file.cio is not formatted correctly. Please check the SWAT IO documentation.");

        NBYR = FileCioSchema.NBYR.GetInt(lines);
        IYR = FileCioSchema.IYR.GetInt(lines);
        IDAF = FileCioSchema.IDAF.GetInt(lines);
        IDAL = FileCioSchema.IDAL.GetInt(lines);
        NYSKIP = FileCioSchema.NYSKIP.GetInt(lines);
        PCPSIM = FileCioSchema.PCPSIM.GetInt(lines);
        IPRINT = FileCioSchema.IPRINT.GetInt(lines);

        //Note: ICALEN is not required. It's a newer parameter and some projects may not have it defined.
        ICALEN = 0;
        if (lines.Length >= FileCioSchema.ICALEN.LineNumber)
            ICALEN = FileCioSchema.ICALEN.GetInt(lines);
    }
}
