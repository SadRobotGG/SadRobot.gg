using System;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.Management.Smo;
using SadRobot.Cmd.DBCD;
using SadRobot.Cmd.DBCD.DBCDReader;
using SqlBulkCopy = System.Data.SqlClient.SqlBulkCopy;
using SqlConnection = System.Data.SqlClient.SqlConnection;
using SqlConnectionStringBuilder = System.Data.SqlClient.SqlConnectionStringBuilder;

namespace SadRobot.Cmd.Commandlet
{
    public class ExportToSql
    {
        const string definitionsPath = @"D:\Work\Games\WoW\wowdev\WoWDBDefs\definitions";

        const string dbPath = @"E:\WoW\dbfilesclient\";
        const string cachePath = @"C:\Games\World of Warcraft\_retail_\Cache\ADB\enUS\";

        public static void Execute(string filter = null)
        {
            var dbcd = new DBCD.DBCD(new DBCProvider(dbPath), new DBDProvider(definitionsPath));
            
            var build = "8.3.0.34220";

            var connectionString = new SqlConnectionStringBuilder
            {
                InitialCatalog = "wow", 
                DataSource = "(local)", 
                IntegratedSecurity = true
            }.ToString();

            using var connection = new SqlConnection(connectionString);
            connection.Open();
            var server = new Server();
            var database = server.Databases["wow"];

            // The DBCache which stores downloaded / patched database records from the server side
            Console.WriteLine("Loading in hotfix cache...");
            var hotfix = new HotfixReader(Path.Combine(cachePath, "DBCache.bin"));
            var caches = Directory.EnumerateFiles(cachePath, "DBCache.bin*.tmp").ToList();
            hotfix.CombineCaches(caches.ToArray());

            Console.WriteLine("Loading tables..");
            foreach (var databaseFile in Directory.EnumerateFiles(dbPath, "*.db?", SearchOption.TopDirectoryOnly))
            {
                Console.WriteLine();

                var name = Path.GetFileNameWithoutExtension(databaseFile);

                if (name.StartsWith("UnitTest", StringComparison.OrdinalIgnoreCase)) continue;

                if (!string.IsNullOrWhiteSpace(filter) && !filter.Contains(name,StringComparison.OrdinalIgnoreCase)) continue;

                var storage = dbcd.Load(name, build, Locale.EnUS).ApplyingHotfixes(hotfix);
                
                DBCDRow item = storage.Values.FirstOrDefault();
                if (item == null)
                {
                    Console.WriteLine( name + ": **EMPTY**");
                    continue;
                }

                Console.WriteLine(name);
                Console.WriteLine(string.Join("", Enumerable.Repeat("=", name.Length)));
                
                using var table = new DataTable(name);
                
                for (var j = 0; j < storage.AvailableColumns.Length; ++j)
                {
                    string fieldname = storage.AvailableColumns[j];
                    var field = item[fieldname];

                    var isEndOfRecord = j == storage.AvailableColumns.Length - 1;

                    if (field is Array a)
                    {
                        for (var i = 0; i < a.Length; i++)
                        {
                            var isEndOfArray = a.Length - 1 == i;
                            Console.Write($"{fieldname}[{i}]");
                            if (!isEndOfArray) Console.Write(",");

                            table.Columns.Add(new DataColumn(fieldname + "_" + i)
                            {
                                DataType = a.GetType().GetElementType()
                            });
                        }
                    }
                    else
                    {
                        var column = new DataColumn(fieldname)
                        {
                            DataType = field.GetType()
                        };

                        table.Columns.Add(column);

                        if (fieldname.Equals("id", StringComparison.OrdinalIgnoreCase))
                        {
                            table.PrimaryKey = new[] {column};
                        }
                        
                        Console.Write(fieldname);
                    }

                    if (!isEndOfRecord) Console.Write(",");
                }
                
                database.CreateTableSchema(table);

                // Process rows
                foreach (var row in storage.Values)
                {
                    var dataRow = table.NewRow();

                    foreach (var fieldName in storage.AvailableColumns)
                    {
                        var value = row[fieldName];
                        
                        if (value is Array a)
                        {
                            for (var j = 0; j < a.Length; j++)
                            {
                                var arrayValue = a.GetValue(j).ToString();

                                // if (searchValues.Contains(arrayValue))
                                // {
                                //     Console.ForegroundColor = ConsoleColor.Yellow;
                                //     Console.WriteLine($"Found matching record: {table.TableName}#{row.ID}");
                                //     Console.ResetColor();
                                // }

                                dataRow[fieldName + "_" + j] = arrayValue;
                            }
                        }
                        else
                        {
                            dataRow[fieldName] = value;
                        }
                    }

                    table.Rows.Add(dataRow);
                }
                
                // Bulk import the data
                var bulk = new SqlBulkCopy(connection);
                bulk.DestinationTableName = table.TableName;
                bulk.WriteToServer(table);
                
                Console.WriteLine();
            }
        }
    }
}
