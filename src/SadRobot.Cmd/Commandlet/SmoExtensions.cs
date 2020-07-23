using System;
using System.Data;
using System.Linq;
using Microsoft.SqlServer.Management.Smo;

namespace SadRobot.Cmd.Commandlet
{
    public static class SmoExtensions
    {
        public static bool DropTableIfExists(this Database database, string tableName)
        {
            var table = database.Tables[tableName];
            if (table == null) return false;
            table.Drop();
            return true;
        }

        public static void CreateTableSchema(this Database database, DataTable dt)
        {
            database.DropTableIfExists(dt.TableName);

            var table = new Table(database, dt.TableName);
            
            var pk = dt.PrimaryKey.SingleOrDefault();
            if (pk == null)
            {
                Console.WriteLine("No primary key set for table " + dt.TableName);
            }

            foreach (DataColumn col in dt.Columns)
            {
                if (pk != null && pk.ColumnName.Equals(col.ColumnName))
                {
                    table.AddPrimaryKey(col.ColumnName, AdaptDataType(col));
                }
                else
                {
                    table.AddColumn(col.ColumnName, AdaptDataType(col), false);
                }
            }

            table.Create();
        }

        static DataType AdaptDataType(DataColumn column)
        {
            if( column.DataType == typeof(string)) return DataType.NVarCharMax;
            if( column.DataType == typeof(int)) return DataType.Int;
            if( column.DataType == typeof(uint)) return DataType.Int;
            if( column.DataType == typeof(long)) return DataType.BigInt;
            if( column.DataType == typeof(ulong)) return DataType.BigInt;
            if( column.DataType == typeof(short)) return DataType.Int;
            if( column.DataType == typeof(ushort)) return DataType.Int;
            if( column.DataType == typeof(byte)) return DataType.Int;
            if( column.DataType == typeof(float)) return DataType.Float;

            throw new ArgumentOutOfRangeException(nameof(column));
        }
    }
}