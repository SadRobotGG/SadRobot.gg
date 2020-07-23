using System.Collections.Generic;
using SadRobot.Cmd.DBCD.DBCDReader;

namespace SadRobot.Cmd.DBCD
{
    public interface IDBCDStorage : IEnumerable<DynamicKeyValuePair<int>>, IDictionary<int, DBCDRow>
    {
        string[] AvailableColumns { get; }

        Dictionary<ulong, int> GetEncryptedSections();
        IDBCDStorage ApplyingHotfixes(HotfixReader hotfixReader);
    }
}