using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CASCLib;

namespace SadRobot.Cmd.Casc
{
    public class WDB5Reader : DB2Reader<WDB5Row>
    {
        private const int HeaderSize = 48;
        private const uint DB5FmtSig = 0x35424457;          // WDB5

        public WDB5Reader(string dbcFile) : this(new FileStream(dbcFile, FileMode.Open)) { }

        public WDB5Reader(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8))
            {
                if (reader.BaseStream.Length < HeaderSize)
                {
                    throw new InvalidDataException(String.Format("DB5 file is corrupted!"));
                }

                uint magic = reader.ReadUInt32();

                if (magic != DB5FmtSig)
                {
                    throw new InvalidDataException(String.Format("DB5 file is corrupted!"));
                }

                RecordsCount = reader.ReadInt32();
                FieldsCount = reader.ReadInt32();
                RecordSize = reader.ReadInt32();
                StringTableSize = reader.ReadInt32();

                uint tableHash = reader.ReadUInt32();
                uint layoutHash = reader.ReadUInt32();
                MinIndex = reader.ReadInt32();
                MaxIndex = reader.ReadInt32();
                int locale = reader.ReadInt32();
                int copyTableSize = reader.ReadInt32();
                int flags = reader.ReadUInt16();
                int idIndex = reader.ReadUInt16();

                bool isSparse = (flags & 0x1) != 0;
                bool hasIndex = (flags & 0x4) != 0;

                m_meta = new FieldMetaData[FieldsCount];

                for (int i = 0; i < m_meta.Length; i++)
                {
                    m_meta[i] = new FieldMetaData()
                    {
                        Bits = reader.ReadInt16(),
                        Offset = reader.ReadInt16()
                    };
                }

                Dictionary<long, string> stringsTable = new Dictionary<long, string>();

                WDB5Row[] m_rows = new WDB5Row[RecordsCount];

                for (int i = 0; i < RecordsCount; i++)
                {
                    m_rows[i] = new WDB5Row(this, reader.ReadBytes(RecordSize), stringsTable);
                }

                for (int i = 0; i < StringTableSize;)
                {
                    long oldPos = reader.BaseStream.Position;

                    stringsTable[i] = CStringExtensions.ReadCString(reader);

                    i += (int)(reader.BaseStream.Position - oldPos);
                }

                if (isSparse)
                {
                    // code...
                    throw new Exception("can't do sparse table");
                }

                if (hasIndex)
                {
                    for (int i = 0; i < RecordsCount; i++)
                    {
                        int id = reader.ReadInt32();
                        var row = m_rows[i];
                        row.SetId(id);
                        _Records[id] = row;
                    }
                }
                else
                {
                    for (int i = 0; i < RecordsCount; i++)
                    {
                        int id = m_rows[i].Data.Skip(m_meta[idIndex].Offset).Take((32 - m_meta[idIndex].Bits) >> 3).Select((b, k) => b << k * 8).Sum();
                        var row = m_rows[i];
                        row.SetId(id);
                        _Records[id] = row;
                    }
                }

                if (copyTableSize > 0)
                {
                    int copyCount = copyTableSize / 8;

                    for (int i = 0; i < copyCount; i++)
                    {
                        int newId = reader.ReadInt32();
                        int oldId = reader.ReadInt32();

                        WDB5Row rec = (WDB5Row)_Records[oldId].Clone();
                        rec.SetId(newId);
                        _Records.Add(newId, rec);
                    }
                }
            }
        }
    }
}
