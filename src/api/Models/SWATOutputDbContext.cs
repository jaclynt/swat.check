using Dapper;
using System.Data.SQLite;
using System.Reflection;
using System.Text;

namespace SWAT.Check.Models;

public class SWATOutputDbContext
{
    private string _databaseFile;
    private string _connectionString;

    public SWATOutputDbContext(string databaseFile)
    {
        _databaseFile = databaseFile;
        _connectionString = new SQLiteConnectionStringBuilder { DataSource = databaseFile, ForeignKeys = true, JournalMode = SQLiteJournalModeEnum.Off }.ConnectionString;
    }

    public SQLiteConnection GetConnection()
    {
        return new SQLiteConnection(_connectionString);
    }

    public void CreateDatabase()
    {
        try
        {
            //File.Delete(_databaseFile);
            DeleteTables();
        }
        catch (Exception) {
            
        }

        if (!File.Exists(_databaseFile))
        {
            SQLiteConnection.CreateFile(_databaseFile);
        }

        using (var conn = new SQLiteConnection(_connectionString))
        {
            conn.Open();

            using (var command = new SQLiteCommand(conn))
            {
                using (var transaction = conn.BeginTransaction())
                {
                    command.CommandText = new SWATCheckStatus().GetCreateTableSql();
                    command.ExecuteNonQuery();

                    command.CommandText = new HydOut().GetCreateTableSql();
                    command.ExecuteNonQuery();

                    command.CommandText = new OutputRch().GetCreateTableSql();
                    command.ExecuteNonQuery();

                    command.CommandText = new OutputRsv().GetCreateTableSql();
                    command.ExecuteNonQuery();

                    command.CommandText = new OutputStd().GetCreateTableSql();
                    command.ExecuteNonQuery();

                    command.CommandText = new OutputStdAvgAnnual().GetCreateTableSql();
                    command.ExecuteNonQuery();

                    command.CommandText = new OutputStdAvgMonBasin().GetCreateTableSql();
                    command.ExecuteNonQuery();

                    command.CommandText = new OutputSub().GetCreateTableSql();
                    command.ExecuteNonQuery();

                    command.CommandText = new OutputHru().GetCreateTableSql();
                    command.ExecuteNonQuery();

                    command.CommandText = new OutputSed().GetCreateTableSql();
                    command.ExecuteNonQuery();

                    command.CommandText = "CREATE INDEX IF NOT EXISTS idx_OutputRch_RCH ON OutputRch(RCH);";
                    command.ExecuteNonQuery();

                    command.CommandText = "CREATE INDEX IF NOT EXISTS idx_OutputSub_SUB ON OutputSub(SUB);";
                    command.ExecuteNonQuery();

                    command.CommandText = "CREATE INDEX IF NOT EXISTS idx_OutputHru_SUB ON OutputHru(SUB);";
                    command.ExecuteNonQuery();

                    transaction.Commit();
                }
            }

            conn.Close();
        }
    }

    public void DeleteTables()
    {
        try
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var command = new SQLiteCommand(conn))
                {
                    using (var transaction = conn.BeginTransaction())
                    {
                        command.CommandText = @"DROP TABLE HydOut;";
                        command.ExecuteNonQuery();
                        command.CommandText = @"DROP TABLE OutputRch;";
                        command.ExecuteNonQuery();
                        command.CommandText = @"DROP TABLE OutputRsv;";
                        command.ExecuteNonQuery();
                        command.CommandText = @"DROP TABLE OutputStd;";
                        command.ExecuteNonQuery();
                        command.CommandText = @"DROP TABLE OutputStdAvgAnnual;";
                        command.ExecuteNonQuery();
                        command.CommandText = @"DROP TABLE OutputStdAvgMonBasin;";
                        command.ExecuteNonQuery();
                        command.CommandText = @"DROP TABLE OutputSub;";
                        command.ExecuteNonQuery();
                        command.CommandText = @"DROP TABLE OutputHru;";
                        command.ExecuteNonQuery();

                        transaction.Commit();
                    }
                }

                conn.Close();
            }
        }
        catch (Exception) { }
    }
}

public static class SWATOutputDbHelpers
{
    public static string GetCreateTableSql<T>(this T obj)
    {
        Type objType = obj.GetType();
        PropertyInfo[] properties = objType.GetProperties();
        StringBuilder sb = new StringBuilder($"CREATE TABLE IF NOT EXISTS `{objType.Name}` (");

        foreach (var property in properties)
        {
            sb.Append($"`{property.Name}`");

            Type type = property.PropertyType;
            Type nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
            {
                type = nullableType;
            }

            var code = Type.GetTypeCode(type);
            switch (code)
            {
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    sb.AppendLine(" INTEGER,");
                    break;
                case TypeCode.Double:
                    sb.AppendLine(" DOUBLE,");
                    break;
                default:
                    sb.AppendLine(" TEXT,");
                    break;
            }
        }

        sb.AppendLine("PRIMARY KEY(ID));");

        return sb.ToString();
    }

    public static List<dynamic> GetOutputHruAvgAnnual(this SQLiteConnection conn, List<string> fields, SWATPrintSetting printCode)
    {
        string sql = "";

        if (printCode == SWATPrintSetting.Daily)
        {
            List<string> selectParams = new();
            List<string> joinParams = new();
            foreach (var field in fields)
            {
                selectParams.Add($"AVG(t1.{field}1) as {field}");
                joinParams.Add($"SUM({field}) as {field}1");
            }

            string selects = string.Join(", ", selectParams);
            string joins = string.Join(", ", joinParams);

            sql = $"SELECT t1.LULC, {selects} FROM ( SELECT LULC, {joins} FROM OutputHru GROUP BY LULC, Year ) as t1";
        }
        else
        {
            List<string> selectParams = new();
            foreach (var field in fields)
            {
                selectParams.Add($"AVG({field}) as {field}");
            }

            string selects = string.Join(", ", selectParams);

            sql = $"SELECT LULC, {selects} FROM OutputHru WHERE YearSpan > 0 GROUP BY LULC";
        }

        return conn.Query(sql).AsList();
    }

    public static List<double> GetAvg(this SQLiteConnection conn, string tableName, string groupFieldName, string avgFieldName, string where)
    {
        string sql = string.Format("SELECT AVG({0}) FROM {1} {2}GROUP BY {3}", avgFieldName, tableName, where, groupFieldName);

        return conn.Query<double>(sql).AsList();
    }

    public static List<double> GetSum(this SQLiteConnection conn, string tableName, string groupFieldName, string sumFieldName, string where, double divideBy = 0)
    {
        return GetSum(conn, tableName, groupFieldName, new List<string> { sumFieldName }, where, divideBy);
    }

    public static List<double> GetSum(this SQLiteConnection conn, string tableName, string groupFieldName, List<string> sumFieldsNames, string where, double divideBy = 0)
    {
        string sum = string.Empty;
        int i = 0;
        foreach (string field in sumFieldsNames)
        {
            if (i > 0)
                sum += " + ";

            sum += string.Format("SUM({0})", field);
            i++;
        }

        string div = divideBy > 0 ? "/" + divideBy : "";
        string sql = string.Format("SELECT ({0}){1} FROM {2} {3}GROUP BY {4}", sum, div, tableName, where, groupFieldName);

        return conn.Query<double>(sql).AsList();
    }
}
