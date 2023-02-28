using SWAT.Check.Models;
using SWAT.Check.Schemas;
using System.Data.SQLite;

namespace SWAT.Check.Readers;

public class ReadHydOut : OutputFileReader
{
	public ReadHydOut(SWATOutputConfig configSettings, string fileName = OutputFileNames.HydOut) : base(configSettings, fileName)
	{
	}

	public override void ReadFile(bool abort)
	{
		if (abort) return;

		var context = new SWATOutputDbContext(_configSettings.DatabaseFile);
		using (var conn = context.GetConnection())
		{
			conn.Open();

			using (var cmd = new SQLiteCommand(conn))
			{
				using (var transaction = conn.BeginTransaction())
				{
					cmd.CommandText = "INSERT INTO `HydOut` (`ICode`, `Flow`, `Sed`, `OrgN`, `OrgP`, `Nitrate`, `SolP`, `SolPst`, `SorPst`) VALUES (@ICode, @Flow, @Sed, @OrgN, @OrgP, @Nitrate, @SolP, @SolPst, @SorPst);";
					IEnumerable<string> lines = File.ReadLines(_filePath);

					foreach (string line in lines.Skip(1))
					{
						cmd.Parameters.Clear();
						cmd.Parameters.AddWithValue("@ICode", HydOutSchema.ICode.GetInt(line));
						cmd.Parameters.AddWithValue("@Flow", HydOutSchema.Flow.GetDouble(line));
						cmd.Parameters.AddWithValue("@Sed", HydOutSchema.Sed.GetDouble(line));
						cmd.Parameters.AddWithValue("@OrgN", HydOutSchema.OrgN.GetDouble(line));
						cmd.Parameters.AddWithValue("@OrgP", HydOutSchema.OrgP.GetDouble(line));
						cmd.Parameters.AddWithValue("@Nitrate", HydOutSchema.Nitrate.GetDouble(line));
						cmd.Parameters.AddWithValue("@SolP", HydOutSchema.SolP.GetDouble(line));
						cmd.Parameters.AddWithValue("@SolPst", HydOutSchema.SolPst.GetDouble(line));
						cmd.Parameters.AddWithValue("@SorPst", HydOutSchema.SorPst.GetDouble(line));
						cmd.ExecuteNonQuery();
					}
					transaction.Commit();
				}
			}

			conn.Close();
		}
	}
}
