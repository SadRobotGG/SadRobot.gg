using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using SadRobot.Cmd.DBCD.DBCDReader.Common;

namespace SadRobot.Cmd.DBCD.DBCDReader.Readers
{
    class HTFXReader : BaseReader
    {
        public readonly int Version;
        public readonly int BuildId;

        private const int HeaderSize = 12;
        private const int ExtendedHeaderSize = 44;
        private const uint HTFXFmtSig = 0x48544658; // XFTH

        public HTFXReader(string dbcFile) : this(new FileStream(dbcFile, FileMode.Open)) { }

        public HTFXReader(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8))
            {
                if (reader.BaseStream.Length < HeaderSize)
                    throw new InvalidDataException("Hotfix file is corrupted!");

                uint magic = reader.ReadUInt32();
                if (magic != HTFXFmtSig)
                    throw new InvalidDataException("Hotfix file is corrupted!");

                Version = reader.ReadInt32();
                BuildId = reader.ReadInt32();

                // Extended header
                if (Version >= 5)
                {
                    if (reader.BaseStream.Length < ExtendedHeaderSize)
                        throw new InvalidDataException("Hotfix file is corrupted!");

                    reader.BaseStream.Position += 32; // sha hash
                }

                var readerFunc = GetReaderFunc();

                long length = reader.BaseStream.Length;
                while (reader.BaseStream.Position < length)
                {
                    magic = reader.ReadUInt32();
                    if (magic != HTFXFmtSig)
                        throw new InvalidDataException("Hotfix file is corrupted!");

                    IHotfixEntry hotfixEntry = readerFunc.Invoke(reader);
                    BitReader bitReader = new BitReader(reader.ReadBytes(hotfixEntry.DataSize));
                    HTFXRow rec = new HTFXRow(bitReader, hotfixEntry);

                    _Records.Add(_Records.Count, rec);
                }
            }
        }


        public IEnumerable<HTFXRow> GetRecords(uint tablehash)
        {
            foreach (HTFXRow record in _Records.Values)
                if (record.TableHash == tablehash)
                    yield return record;
        }

        public void Combine(HTFXReader reader)
        {
            var lookup = new HashSet<HTFXRow>(_Records.Values.Cast<HTFXRow>());

            // copy records not in the current set
            foreach (HTFXRow row in reader._Records.Values)
            {
                if (!lookup.Contains(row))
                {
                    _Records.Add(_Records.Count, row);
                    lookup.Add(row);
                }
            }
        }


        private Func<BinaryReader, IHotfixEntry> GetReaderFunc()
        {
            Type hotfixType;

            if (Version == 1)
                hotfixType = typeof(HotfixEntryV1);
            else if (Version >= 2 && Version <= 6)
                hotfixType = typeof(HotfixEntryV2);
            else if (Version == 7)
                hotfixType = typeof(HotfixEntryV7);
            else
                throw new NotSupportedException($"Hotfix version {Version} is not supported");

            var param = Expression.Parameter(typeof(BinaryReader), "reader");
            var readMethod = typeof(Extensions).GetMethod("Read").MakeGenericMethod(hotfixType);
            var callExpression = Expression.Call(readMethod, param);
            var convertExpression = Expression.Convert(callExpression, typeof(IHotfixEntry));

            return Expression.Lambda<Func<BinaryReader, IHotfixEntry>>(convertExpression, param).Compile();
        }
    }
}
