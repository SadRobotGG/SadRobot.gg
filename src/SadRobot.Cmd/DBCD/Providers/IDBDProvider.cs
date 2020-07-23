using System.IO;

namespace SadRobot.Cmd.DBCD.Providers
{
    public interface IDBDProvider
    {
        Stream StreamForTableName(string tableName, string build = null);
    }
}