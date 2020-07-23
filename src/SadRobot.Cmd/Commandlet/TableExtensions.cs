using Microsoft.SqlServer.Management.Smo;

namespace SadRobot.Cmd.Commandlet
{
    public static class TableExtensions
    {
        public static Table AddPrimaryKey(this Table table, string name, DataType dataType, bool isIdentityColumn = false, bool isClusteredIndex = true)
        {
            var column = new Column(table, name, dataType);
            column.Nullable = false;

            if (isIdentityColumn)
            {
                column.Identity = true;
                column.IdentitySeed = 1;
                column.IdentityIncrement = 1;
            }
            else if (dataType.Equals(DataType.UniqueIdentifier))
            {
                column.AddDefaultConstraint();
                column.DefaultConstraint.Text = "NEWID()";
            }

            table.Columns.Add(column);

            // Create the primary key index.
            var index = new Index(table, "PK_" + table.Name);

            index.IsClustered = isClusteredIndex;
            index.IsUnique = true;
            index.IndexKeyType = IndexKeyType.DriPrimaryKey;

            index.IndexedColumns.Add(new IndexedColumn(index, column.Name));
            table.Indexes.Add(index);

            return table;
        }

        public static Table AddColumn(this Table table, string name, DataType dataType, bool nullable)
        {
            var column = new Column(table, name, dataType);
            column.Nullable = nullable;
            table.Columns.Add(column);
            return table;
        }
        
        public static string GetUniqueIndexName(this Table table, params string[] columns)
        {
            return "UX_" + string.Join("_", columns);
        }

        public static Table AddUniqueIndex(this Table table, params string[] columns)
        {
            var index = new Index(table, "UX_" + string.Join("_", columns));
            index.IsUnique = true;
            index.IndexKeyType = IndexKeyType.DriUniqueKey;

            foreach (var column in columns)
            {
                index.IndexedColumns.Add(new IndexedColumn(index, column));
            }

            table.Indexes.Add(index);

            return table;
        }

        public static Table DropColumnIfExists(this Table table, string columnName)
        {
            var column = table.Columns[columnName];
            if (column == null) return table;

            column.Drop();
            return table;
        }
    }
}