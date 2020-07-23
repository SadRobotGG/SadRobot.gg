using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SadRobot.Cmd.DBCD.DBCDReader.Common;

namespace SadRobot.Cmd.DBCD.DBCDReader.Readers
{
    class WDB2Reader : BaseReader
    {
        private const int HeaderSize = 28;
        private const int ExtendedHeaderSize = 48;
        private const uint WDB2FmtSig = 0x32424457; // WDB2

        public WDB2Reader(string dbcFile) : this(new FileStream(dbcFile, FileMode.Open)) { }

        public WDB2Reader(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8))
            {
                if (reader.BaseStream.Length < HeaderSize)
                    throw new InvalidDataException("WDB2 file is corrupted!");

                uint magic = reader.ReadUInt32();

                if (magic != WDB2FmtSig)
                    throw new InvalidDataException("WDB2 file is corrupted!");

                RecordsCount = reader.ReadInt32();
                FieldsCount = reader.ReadInt32();
                RecordSize = reader.ReadInt32();
                StringTableSize = reader.ReadInt32();
                TableHash = reader.ReadUInt32();
                uint build = reader.ReadUInt32();
                uint timestamp = reader.ReadUInt32();

                if (RecordsCount == 0)
                    return;

                // Extended header 
                if (build > 12880)
                {
                    if (reader.BaseStream.Length < ExtendedHeaderSize)
                        throw new InvalidDataException("WDB2 file is corrupted!");

                    MinIndex = reader.ReadInt32();
                    MaxIndex = reader.ReadInt32();
                    int locale = reader.ReadInt32();
                    int copyTableSize = reader.ReadInt32();

                    if (MaxIndex > 0)
                    {
                        int diff = MaxIndex - MinIndex + 1;
                        reader.BaseStream.Position += diff * 4; // indicies uint[]
                        reader.BaseStream.Position += diff * 2; // string lengths ushort[]
                    }
                }

                recordsData = reader.ReadBytes(RecordsCount * RecordSize);
                Array.Resize(ref recordsData, recordsData.Length + 8); // pad with extra zeros so we don't crash when reading

                for (int i = 0; i < RecordsCount; i++)
                {
                    BitReader bitReader = new BitReader(recordsData) { Position = i * RecordSize * 8 };
                    IDBRow rec = new WDB2Row(this, bitReader, i);
                    _Records.Add(i, rec);
                }

                m_stringsTable = new Dictionary<long, string>(StringTableSize / 0x20);
                for (int i = 0; i < StringTableSize;)
                {
                    long oldPos = reader.BaseStream.Position;
                    m_stringsTable[i] = reader.ReadCString();
                    i += (int)(reader.BaseStream.Position - oldPos);
                }
            }
        }
    }
}
