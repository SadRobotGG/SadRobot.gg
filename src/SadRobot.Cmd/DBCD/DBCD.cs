using System;
using SadRobot.Cmd.DBCD.DBCDReader;
using SadRobot.Cmd.DBCD.Providers;

namespace SadRobot.Cmd.DBCD
{

    public class DBCD
    {
        private readonly IDBCProvider dbcProvider;
        private readonly IDBDProvider dbdProvider;
        public DBCD(IDBCProvider dbcProvider, IDBDProvider dbdProvider)
        {
            this.dbcProvider = dbcProvider;
            this.dbdProvider = dbdProvider;
        }

        public IDBCDStorage Load(string tableName, string build = null, Locale locale = Locale.None)
        {
            var dbcStream = this.dbcProvider.StreamForTableName(tableName, build);
            var dbdStream = this.dbdProvider.StreamForTableName(tableName, build);

            var builder = new DBCDBuilder(locale);

            var dbReader = new DBReader(dbcStream);
            var definition = builder.Build(dbReader, dbdStream, tableName, build);

            var type = typeof(DBCDStorage<>).MakeGenericType(definition.Item1);

            try
            {
                return (IDBCDStorage)Activator.CreateInstance(type, new object[2] {
                    dbReader,
                    definition.Item2
                });
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}