using System;
using System.Linq;
using System.Threading.Tasks;

namespace SadRobot.Cmd.Commandlet
{
    public static class SearchDatabase
    {
        

        public static async Task Execute(string search)
        {
            if (string.IsNullOrWhiteSpace(search)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(search));

            await using var connection = new Microsoft.Data.SqlClient.SqlConnection(Configuration.ConnectionString);

            await connection.OpenAsync();

            foreach (var table in connection.ExecuteQuery("SELECT * FROM INFORMATION_SCHEMA.TABLES ORDER BY TABLE_NAME").ToList())
            {
                var tableName = table["TABLE_NAME"];

                //Console.WriteLine(tableName);

                foreach (var row in connection.ExecuteQuery($"SELECT * FROM [{tableName}]"))
                {
                    foreach (var value in row.Values)
                    {
                        if (value == null) continue;
                        var stringValue = value.ToString();
                        if (stringValue.Equals(search, StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine($"{tableName} " + row.First().Key + " #" + row.First().Value);
                        }
                    }
                }
            }
        }
    }
}