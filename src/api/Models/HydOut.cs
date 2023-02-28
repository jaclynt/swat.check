using Dapper.Contrib.Extensions;

namespace SWAT.Check.Models;

[Table("HydOut")]
public class HydOut
{
	public int ID { get; set; }

	public int ICode { get; set; }
	public double Flow { get; set; }
	public double Sed { get; set; }
	public double OrgN { get; set; }
	public double OrgP { get; set; }
	public double Nitrate { get; set; }
	public double SolP { get; set; }
	public double SolPst { get; set; }
	public double SorPst { get; set; }
}
