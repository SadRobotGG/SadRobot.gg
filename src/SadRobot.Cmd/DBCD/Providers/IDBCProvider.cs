using System.IO;

namespace SadRobot.Cmd.DBCD.Providers
{
    public interface IDBCProvider
    {
        Stream StreamForTableName(string tableName, string build);
    }
}