using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using SadRobot.Cmd.DBCD.DBCDReader.Common;

namespace SadRobot.Cmd.DBCD.DBCDReader.Readers
{
    class WDB6Reader : BaseReader
    {
        private const int HeaderSize = 56;
        private const uint WDB6FmtSig = 0x36424457; // WDB6

        // CommonData type enum to bit size
        private readonly Dictionary<byte, short> CommonTypeBits = new Dictionary<byte, short>
        {
            { 0, 0 },  // string
            { 1, 16 }, // short
            { 2, 24 }, // byte
            { 3, 0 },  // float
            { 4, 0 },  // int
        };

        public WDB6Reader(string dbcFile) : this(new FileStream(dbcFile, FileMode.Open)) { }

        public unsafe WDB6Reader(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8))
            {
                if (reader.BaseStream.Length < HeaderSize)
                    throw new InvalidDataException("WDB6 file is corrupted!");

                uint magic = reader.ReadUInt32();

                if (magic != WDB6FmtSig)
                    throw new InvalidDataException("WDB6 file is corrupted!");

                RecordsCount = reader.ReadInt32();
                FieldsCount = reader.ReadInt32();
                RecordSize = reader.ReadInt32();
                StringTableSize = reader.ReadInt32();
                TableHash = reader.ReadUInt32();
                LayoutHash = reader.ReadUInt32();
                MinIndex = reader.ReadInt32();
                MaxIndex = reader.ReadInt32();
                int locale = reader.ReadInt32();
                int copyTableSize = reader.ReadInt32();
                Flags = (DB2Flags)reader.ReadUInt16();
                IdFieldIndex = reader.ReadUInt16();
                int totalFieldCount = reader.ReadInt32();
                int commonDataSize = reader.ReadInt32();

                if (RecordsCount == 0)
                    return;

                // field meta data
                m_meta = reader.ReadArray<FieldMetaData>(FieldsCount);

                if (!Flags.HasFlagExt(DB2Flags.Sparse))
                {
                    // records data
                    recordsData = reader.ReadBytes(RecordsCount * RecordSize);
                    Array.Resize(ref recordsData, recordsData.Length + 8); // pad with extra zeros so we don't crash when reading

                    // string table
                    m_stringsTable = new Dictionary<long, string>(StringTableSize / 0x20);
                    for (int i = 0; i < StringTableSize;)
                    {
                        long oldPos = reader.BaseStream.Position;
                        m_stringsTable[i] = reader.ReadCString();
                        i += (int)(reader.BaseStream.Position - oldPos);
                    }
                }
                else
                {
                    // sparse data with inlined strings
                    recordsData = reader.ReadBytes(StringTableSize - (int)reader.BaseStream.Position);

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
                }

                // secondary key
                if (Flags.HasFlagExt(DB2Flags.SecondaryKey))
                    m_foreignKeyData = reader.ReadArray<int>(MaxIndex - MinIndex + 1);

                // index table
                if (Flags.HasFlagExt(DB2Flags.Index))
                    m_indexData = reader.ReadArray<int>(RecordsCount);

                // duplicate rows data
                if (m_copyData == null)
                    m_copyData = new Dictionary<int, int>(copyTableSize / 8);

                for (int i = 0; i < copyTableSize / 8; i++)
                    m_copyData[reader.ReadInt32()] = reader.ReadInt32();

                if (commonDataSize > 0)
                {
                    Array.Resize(ref m_meta, totalFieldCount);

                    int fieldCount = reader.ReadInt32();
                    m_commonData = new Dictionary<int, Value32>[fieldCount];

                    // HACK as of 24473 values are 4 byte aligned
                    // try to calculate this by seeing if all <id, value> tuples are 8 bytes
                    bool aligned = (commonDataSize - 4 - (fieldCount * 5)) % 8 == 0;

                    for (int i = 0; i < fieldCount; i++)
                    {
                        int count = reader.ReadInt32();
                        byte type = reader.ReadByte();
                        int size = aligned ? 4 : (32 - CommonTypeBits[type]) >> 3;

                        // add the new meta entry
                        if (i > FieldsCount)
                        {
                            m_meta[i] = new FieldMetaData()
                            {
                                Bits = CommonTypeBits[type],
                                Offset = (short)(m_meta[i - 1].Offset + ((32 - m_meta[i - 1].Bits) >> 3))
                            };
                        }

                        var commonValues = new Dictionary<int, Value32>(count);
                        for (int j = 0; j < count; j++)
                        {
                            int id = reader.ReadInt32();
                            byte[] buffer = reader.ReadBytes(size);
                            Value32 value = Unsafe.ReadUnaligned<Value32>(ref buffer[0]);

                            commonValues.Add(id, value);
                        }

                        m_commonData[i] = commonValues;
                    }
                }

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

                    IDBRow rec = new WDB6Row(this, bitReader, Flags.HasFlagExt(DB2Flags.Index) ? m_indexData[i] : -1, i);
                    _Records.Add(i, rec);
                }
            }
        }
    }

}
