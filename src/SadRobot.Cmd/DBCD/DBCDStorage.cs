using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using SadRobot.Cmd.DBCD.DBCDReader;
using SadRobot.Cmd.DBCD.Helpers;

namespace SadRobot.Cmd.DBCD
{
    public class DBCDStorage<T> : ReadOnlyDictionary<int, DBCDRow>, IDBCDStorage where T : class, new()
    {
        private readonly FieldAccessor fieldAccessor;
        private readonly ReadOnlyDictionary<int, T> storage;
        private readonly DBCDInfo info;
        private readonly DBReader reader;

        string[] IDBCDStorage.AvailableColumns => this.info.availableColumns;
        public override string ToString() => $"{this.info.tableName}";

        public DBCDStorage(Stream stream, DBCDInfo info) : this(new DBReader(stream), info) { }

        public DBCDStorage(DBReader dbReader, DBCDInfo info) : this(dbReader, new ReadOnlyDictionary<int, T>(dbReader.GetRecords<T>()), info) { }

        public DBCDStorage(DBReader reader, ReadOnlyDictionary<int, T> storage, DBCDInfo info) : base(new Dictionary<int, DBCDRow>())
        {
            this.info = info;
            this.fieldAccessor = new FieldAccessor(typeof(T), info.availableColumns);
            this.reader = reader;
            this.storage = storage;

            foreach (var record in storage)
                base.Dictionary.Add(record.Key, new DBCDRow(record.Key, record.Value, fieldAccessor));
        }

        public IDBCDStorage ApplyingHotfixes(HotfixReader hotfixReader)
        {
            var mutableStorage = this.storage.ToDictionary(k => k.Key, v => v.Value);

            hotfixReader.ApplyHotfixes(mutableStorage, this.reader);

            return new DBCDStorage<T>(this.reader, new ReadOnlyDictionary<int, T>(mutableStorage), this.info);
        }

        IEnumerator<DynamicKeyValuePair<int>> IEnumerable<DynamicKeyValuePair<int>>.GetEnumerator()
        {
            var enumerator = GetEnumerator();
            while (enumerator.MoveNext())
                yield return new DynamicKeyValuePair<int>(enumerator.Current.Key, enumerator.Current.Value);
        }
        
        public Dictionary<ulong, int> GetEncryptedSections() => this.reader.GetEncryptedSections();
    }
}
