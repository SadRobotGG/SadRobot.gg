using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SadRobot.Cmd.DBCD.DBCDReader.Common;

namespace SadRobot.Cmd.DBCD.DBCDReader.Readers
{
    class WDBCReader : BaseReader
    {
        private const int HeaderSize = 20;
        private const uint WDBCFmtSig = 0x43424457; // WDBC

        public WDBCReader(string dbcFile) : this(new FileStream(dbcFile, FileMode.Open)) { }

        public WDBCReader(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8))
            {
                if (reader.BaseStream.Length < HeaderSize)
                    throw new InvalidDataException("WDBC file is corrupted!");

                uint magic = reader.ReadUInt32();

                if (magic != WDBCFmtSig)
                    throw new InvalidDataException("WDBC file is corrupted!");

                RecordsCount = reader.ReadInt32();
                FieldsCount = reader.ReadInt32();
                RecordSize = reader.ReadInt32();
                StringTableSize = reader.ReadInt32();

                if (RecordsCount == 0)
                    return;

                recordsData = reader.ReadBytes(RecordsCount * RecordSize);
                Array.Resize(ref recordsData, recordsData.Length + 8); // pad with extra zeros so we don't crash when reading

                for (int i = 0; i < RecordsCount; i++)
                {
                    BitReader bitReader = new BitReader(recordsData) { Position = i * RecordSize * 8 };
                    IDBRow rec = new WDBCRow(this, bitReader, i);
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
