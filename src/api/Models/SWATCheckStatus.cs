using Dapper.Contrib.Extensions;

namespace SWAT.Check.Models;

[Table("SWATCheckStatus")]
public class SWATCheckStatus
{
	public int ID { get; set; }
	public string Version { get; set; }
	public DateTime LastRunAt { get; set; }
}
