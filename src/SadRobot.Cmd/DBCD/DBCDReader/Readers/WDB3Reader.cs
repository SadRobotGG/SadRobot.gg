using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SadRobot.Cmd.DBCD.DBCDReader.Common;

namespace SadRobot.Cmd.DBCD.DBCDReader.Readers
{
    class WDB3Reader : BaseReader
    {
        private const int HeaderSize = 48;
        private const uint WDB3FmtSig = 0x33424457; // WDB3

        // flags were added inline in WDB4, these are from meta
        // not worth documenting Index as this can be calculated
        private readonly Dictionary<uint, DB2Flags> FlagTable = new Dictionary<uint, DB2Flags>
        {
            { 3348406326u, DB2Flags.Sparse }, // conversationline
            { 2442913102u, DB2Flags.Sparse }, // item-sparse
            { 2982519032u, DB2Flags.Sparse | DB2Flags.SecondaryKey }, // wmominimaptexture
        };

        public WDB3Reader(string dbcFile) : this(new FileStream(dbcFile, FileMode.Open)) { }

        public WDB3Reader(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8))
            {
                if (reader.BaseStream.Length < HeaderSize)
                    throw new InvalidDataException("WDB3 file is corrupted!");

                uint magic = reader.ReadUInt32();

                if (magic != WDB3FmtSig)
                    throw new InvalidDataException("WDB3 file is corrupted!");

                RecordsCount = reader.ReadInt32();
                FieldsCount = reader.ReadInt32();
                RecordSize = reader.ReadInt32();
                StringTableSize = reader.ReadInt32();
                TableHash = reader.ReadUInt32();
                uint build = reader.ReadUInt32();
                uint timestamp = reader.ReadUInt32();
                MinIndex = reader.ReadInt32();
                MaxIndex = reader.ReadInt32();
                int locale = reader.ReadInt32();
                int copyTableSize = reader.ReadInt32();

                if (RecordsCount == 0)
                    return;

                // apply known flags
                if (FlagTable.TryGetValue(TableHash, out var flags))
                    Flags |= flags;

                // sparse data with inlined strings
                if (Flags.HasFlagExt(DB2Flags.Sparse))
                {
                    int sparseCount = MaxIndex - MinIndex + 1;

                    m_sparseEntries = new List<SparseEntry>(sparseCount);
                    m_copyData = new Dictionary<int, int>(sparseCount);
                    var sparseIdLookup = new Dictionary<uint, int>(sparseCount);

                    for (int i = 0; i < sparseCount; i++)
                    {
                        SparseEntry sparse = reader.Read<SparseEntry>();
                        if (sparse.Offset == 0 || sparse.Size == 0)
                            continue;

                        if (sparseIdLookup.TryGetValue(sparse.Offset, out int copyId))
                        {
                            m_copyData[MinIndex + i] = copyId;
                        }
                        else
                        {
                            m_sparseEntries.Add(sparse);
                            sparseIdLookup.Add(sparse.Offset, MinIndex + i);
                        }
                    }

                    // secondary key
                    if (Flags.HasFlagExt(DB2Flags.SecondaryKey))
                        m_foreignKeyData = reader.ReadArray<int>(MaxIndex - MinIndex + 1);

                    recordsData = reader.ReadBytes(m_sparseEntries.Sum(x => x.Size));
                }
                else
                {
                    // secondary key
                    if (Flags.HasFlagExt(DB2Flags.SecondaryKey))
                        m_foreignKeyData = reader.ReadArray<int>(MaxIndex - MinIndex + 1);

                    // record data
                    recordsData = reader.ReadBytes(RecordsCount * RecordSize);
                    Array.Resize(ref recordsData, recordsData.Length + 8); // pad with extra zeros so we don't crash when reading
                }

                // string table
                m_stringsTable = new Dictionary<long, string>(StringTableSize / 0x20);
                for (int i = 0; i < StringTableSize;)
                {
                    long oldPos = reader.BaseStream.Position;
                    m_stringsTable[i] = reader.ReadCString();
                    i += (int)(reader.BaseStream.Position - oldPos);
                }

                // index table
                if ((reader.BaseStream.Position + copyTableSize) < reader.BaseStream.Length)
                {
                    m_indexData = reader.ReadArray<int>(RecordsCount);
                    Flags |= DB2Flags.Index;
                }

                // duplicate rows data
                if (m_copyData == null)
                    m_copyData = new Dictionary<int, int>(copyTableSize / 8);

                for (int i = 0; i < copyTableSize / 8; i++)
                    m_copyData[reader.ReadInt32()] = reader.ReadInt32();

                int position = 0;
                for (int i = 0; i < RecordsCount; i++)
                {
                    BitReader bitReader = new BitReader(recordsData) { Position = 0 };

                    if (Flags.HasFlagExt(DB2Flags.Sparse))
                    {
                        bitReader.Position = position;
                        position += m_sparseEntries[i].Size * 8;
                    }
                    else
                    {
                        bitReader.Offset = i * RecordSize;
                    }

                    IDBRow rec = new WDB3Row(this, bitReader, Flags.HasFlagExt(DB2Flags.Index) ? m_indexData[i] : -1, i);
                    _Records.Add(i, rec);
                }
            }
        }
    }
}
