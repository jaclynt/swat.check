using Dapper.Contrib.Extensions;

namespace SWAT.Check.Models;

[Table("OutputStdAvgMonBasin")]
public class OutputStdAvgMonBasin
{
	public int ID { get; set; }

	public int Mon { get; set; }
	public double Rain { get; set; }
	public double SnowFall { get; set; }
	public double SurfQ { get; set; }
	public double LatQ { get; set; }
	public double WaterYield { get; set; }
	public double ET { get; set; }
	public double SedYield { get; set; }
	public double PET { get; set; }
}
