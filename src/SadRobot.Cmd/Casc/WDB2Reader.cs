﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SadRobot.Cmd.Casc
{
    public class WDB2Reader : IEnumerable<KeyValuePair<int, DB2Row>>
    {
        private const int HeaderSize = 48;
        private const uint DB2FmtSig = 0x32424457;          // WDB2

        public int RecordsCount { get; private set; }
        public int FieldsCount { get; private set; }
        public int RecordSize { get; private set; }
        public int StringTableSize { get; private set; }
        public int MinIndex { get; private set; }
        public int MaxIndex { get; private set; }

        private readonly DB2Row[] m_rows;
        public byte[] StringTable { get; private set; }

        readonly Dictionary<int, DB2Row> m_index = new Dictionary<int, DB2Row>();

        public WDB2Reader(string dbcFile) : this(new FileStream(dbcFile, FileMode.Open)) { }

        public WDB2Reader(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8))
            {
                if (reader.BaseStream.Length < HeaderSize)
                {
                    throw new InvalidDataException(string.Format("DB2 file is corrupted!"));
                }

                if (reader.ReadUInt32() != DB2FmtSig)
                {
                    throw new InvalidDataException(string.Format("DB2 file is corrupted!"));
                }

                RecordsCount = reader.ReadInt32();
                FieldsCount = reader.ReadInt32();
                RecordSize = reader.ReadInt32();
                StringTableSize = reader.ReadInt32();

                // WDB2 specific fields
                uint tableHash = reader.ReadUInt32();   // new field in WDB2
                uint build = reader.ReadUInt32();       // new field in WDB2
                uint unk1 = reader.ReadUInt32();        // new field in WDB2

                if (build > 12880) // new extended header
                {
                    int MinId = reader.ReadInt32();     // new field in WDB2
                    int MaxId = reader.ReadInt32();     // new field in WDB2
                    int locale = reader.ReadInt32();    // new field in WDB2
                    int unk5 = reader.ReadInt32();      // new field in WDB2

                    if (MaxId != 0)
                    {
                        var diff = MaxId - MinId + 1;   // blizzard is weird people...
                        reader.ReadBytes(diff * 4);     // an index for rows
                        reader.ReadBytes(diff * 2);     // a memory allocation bank
                    }
                }

                m_rows = new DB2Row[RecordsCount];

                for (int i = 0; i < RecordsCount; i++)
                {
                    m_rows[i] = new DB2Row(this, reader.ReadBytes(RecordSize));

                    int idx = BitConverter.ToInt32(m_rows[i].Data, 0);

                    if (idx < MinIndex)
                        MinIndex = idx;

                    if (idx > MaxIndex)
                        MaxIndex = idx;

                    m_index[idx] = m_rows[i];
                }

                StringTable = reader.ReadBytes(StringTableSize);
            }
        }

        public bool HasRow(int index)
        {
            return m_index.ContainsKey(index);
        }

        public DB2Row GetRow(int index)
        {
            m_index.TryGetValue(index, out DB2Row row);
            return row;
        }

        public IEnumerator<KeyValuePair<int, DB2Row>> GetEnumerator()
        {
            return m_index.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_index.GetEnumerator();
        }
    }
}
