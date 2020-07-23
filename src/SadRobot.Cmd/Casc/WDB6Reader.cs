using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CASCLib;

namespace SadRobot.Cmd.Casc
{
    public class WDB6Reader : DB2Reader<WDB6Row>
    {
        private const int HeaderSize = 56;
        private const uint DB6FmtSig = 0x36424457;          // WDB6

        public WDB6Reader(string dbcFile) : this(new FileStream(dbcFile, FileMode.Open)) { }

        public WDB6Reader(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8))
            {
                if (reader.BaseStream.Length < HeaderSize)
                {
                    throw new InvalidDataException(String.Format("DB6 file is corrupted!"));
                }

                uint magic = reader.ReadUInt32();

                if (magic != DB6FmtSig)
                {
                    throw new InvalidDataException(String.Format("DB6 file is corrupted!"));
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

                int totalFieldsCount = reader.ReadInt32();
                int commonDataSize = reader.ReadInt32();

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

                Dictionary<long, string> m_stringsTable = new Dictionary<long, string>();

                WDB6Row[] m_rows = new WDB6Row[RecordsCount];

                for (int i = 0; i < RecordsCount; i++)
                {
                    m_rows[i] = new WDB6Row(this, reader.ReadBytes(RecordSize), m_stringsTable);
                }

                for (int i = 0; i < StringTableSize;)
                {
                    long oldPos = reader.BaseStream.Position;

                    m_stringsTable[i] = CStringExtensions.ReadCString(reader);

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

                        WDB6Row rec = (WDB6Row)_Records[oldId].Clone();
                        rec.SetId(newId);
                        _Records.Add(newId, rec);
                    }
                }

                if (commonDataSize > 0)
                {
                    Array.Resize(ref m_meta, totalFieldsCount);

                    Dictionary<byte, short> typeToBits = new Dictionary<byte, short>()
                    {
                        [1] = 16,
                        [2] = 24,
                        [3] = 0,
                        [4] = 0,
                    };

                    int fieldsCount = reader.ReadInt32();
                    Dictionary<int, byte[]>[] fieldData = new Dictionary<int, byte[]>[fieldsCount];

                    for (int i = 0; i < fieldsCount; i++)
                    {
                        int count = reader.ReadInt32();
                        byte type = reader.ReadByte();

                        if (i >= FieldsCount)
                            m_meta[i] = new FieldMetaData() { Bits = typeToBits[type], Offset = (short)(m_meta[i - 1].Offset + ((32 - m_meta[i - 1].Bits) >> 3)) };

                        fieldData[i] = new Dictionary<int, byte[]>();

                        for (int j = 0; j < count; j++)
                        {
                            int id = reader.ReadInt32();

                            byte[] data;

                            switch (type)
                            {
                                case 1: // 2 bytes
                                    data = reader.ReadBytes(2);
                                    reader.Skip(2); // 7.3 fix
                                    break;
                                case 2: // 1 bytes
                                    data = reader.ReadBytes(1);
                                    reader.Skip(3); // 7.3 fix
                                    break;
                                case 3: // 4 bytes
                                case 4:
                                    data = reader.ReadBytes(4);
                                    break;
                                default:
                                    throw new Exception("Invalid data type " + type);
                            }

                            fieldData[i].Add(id, data);
                        }
                    }

                    var keys = _Records.Keys.ToArray();
                    foreach (var row in keys)
                    {
                        for (int i = 0; i < fieldData.Length; i++)
                        {
                            var col = fieldData[i];

                            if (col.Count == 0)
                                continue;

                            WDB6Row rowRef = _Records[row];
                            byte[] rowData = rowRef.Data;

                            byte[] data = col.ContainsKey(row) ? col[row] : new byte[col.First().Value.Length];

                            Array.Resize(ref rowData, rowData.Length + data.Length);
                            Array.Copy(data, 0, rowData, m_meta[i].Offset, data.Length);

                            rowRef.Data = rowData;
                        }
                    }

                    FieldsCount = totalFieldsCount;
                }
            }
        }
    }
}
