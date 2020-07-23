using System;
using System.Collections.Generic;

namespace SadRobot.Cmd.Casc
{
    public class WDC3Row : IDB2Row
    {
        private BitReader m_data;
        private WDC3Reader m_reader;
        private int m_dataOffset;
        private int m_recordOffset;
        private bool m_isSparse;
        private int m_refId;
        private Dictionary<long, string> m_stringsTable;
        private int m_id;

        public int GetId() => m_id;
        public void SetId(int id) => m_id = id;

        public WDC3Row(WDC3Reader reader, BitReader data, int recordOffset, int id, int refId, bool isSparse, Dictionary<long, string> stringsTable)
        {
            m_reader = reader;
            m_data = data;
            m_recordOffset = recordOffset;
            m_refId = refId;
            m_isSparse = isSparse;
            m_stringsTable = stringsTable;

            m_dataOffset = m_data.Offset;

            if (id != -1)
                m_id = id;
            else
            {
                int idFieldIndex = reader.IdFieldIndex;

                m_data.Position = reader.ColumnMeta[idFieldIndex].RecordOffset;

                m_id = FieldReader.GetFieldValue<int>(0, m_data, reader.Meta[idFieldIndex], reader.ColumnMeta[idFieldIndex], reader.PalletData[idFieldIndex], reader.CommonData[idFieldIndex]);
            }
        }

        private static Dictionary<Type, Func<int, BitReader, int, FieldMetaData, ColumnMetaData, Value32[], Dictionary<int, Value32>, Dictionary<long, string>, object>> simpleReaders = new Dictionary<Type, Func<int, BitReader, int, FieldMetaData, ColumnMetaData, Value32[], Dictionary<int, Value32>, Dictionary<long, string>, object>>
        {
            [typeof(float)] = (id, data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => FieldReader.GetFieldValue<float>(id, data, fieldMeta, columnMeta, palletData, commonData),
            [typeof(int)] = (id, data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => FieldReader.GetFieldValue<int>(id, data, fieldMeta, columnMeta, palletData, commonData),
            [typeof(uint)] = (id, data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => FieldReader.GetFieldValue<uint>(id, data, fieldMeta, columnMeta, palletData, commonData),
            [typeof(short)] = (id, data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => FieldReader.GetFieldValue<short>(id, data, fieldMeta, columnMeta, palletData, commonData),
            [typeof(ushort)] = (id, data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => FieldReader.GetFieldValue<ushort>(id, data, fieldMeta, columnMeta, palletData, commonData),
            [typeof(sbyte)] = (id, data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => FieldReader.GetFieldValue<sbyte>(id, data, fieldMeta, columnMeta, palletData, commonData),
            [typeof(byte)] = (id, data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => FieldReader.GetFieldValue<byte>(id, data, fieldMeta, columnMeta, palletData, commonData),
            [typeof(string)] = (id, data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => { var pos = recordOffset + (data.Position >> 3); int strOfs = FieldReader.GetFieldValue<int>(id, data, fieldMeta, columnMeta, palletData, commonData); return stringTable[pos + strOfs]; },
        };

        private static Dictionary<Type, Func<BitReader, int, FieldMetaData, ColumnMetaData, Value32[], Dictionary<int, Value32>, Dictionary<long, string>, int, object>> arrayReaders = new Dictionary<Type, Func<BitReader, int, FieldMetaData, ColumnMetaData, Value32[], Dictionary<int, Value32>, Dictionary<long, string>, int, object>>
        {
            [typeof(float)] = (data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable, arrayIndex) => FieldReader.GetFieldValueArray<float>(data, fieldMeta, columnMeta, palletData, commonData, arrayIndex + 1)[arrayIndex],
            [typeof(int)] = (data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable, arrayIndex) => FieldReader.GetFieldValueArray<int>(data, fieldMeta, columnMeta, palletData, commonData, arrayIndex + 1)[arrayIndex],
            [typeof(uint)] = (data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable, arrayIndex) => FieldReader.GetFieldValueArray<uint>(data, fieldMeta, columnMeta, palletData, commonData, arrayIndex + 1)[arrayIndex],
            [typeof(ulong)] = (data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable, arrayIndex) => FieldReader.GetFieldValueArray<ulong>(data, fieldMeta, columnMeta, palletData, commonData, arrayIndex + 1)[arrayIndex],
            [typeof(ushort)] = (data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable, arrayIndex) => FieldReader.GetFieldValueArray<ushort>(data, fieldMeta, columnMeta, palletData, commonData, arrayIndex + 1)[arrayIndex],
            [typeof(byte)] = (data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable, arrayIndex) => FieldReader.GetFieldValueArray<byte>(data, fieldMeta, columnMeta, palletData, commonData, arrayIndex + 1)[arrayIndex],
            [typeof(string)] = (data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable, arrayIndex) => FieldReader.GetFieldValueStringsArray(data, fieldMeta, columnMeta, palletData, commonData, stringTable, false, recordOffset, arrayIndex + 1)[arrayIndex],
        };

        public T GetField<T>(int fieldIndex, int arrayIndex = -1)
        {
            object value = null;

            if (fieldIndex >= m_reader.Meta.Length)
            {
                if (m_refId != -1)
                    value = m_refId;
                else
                    value = 0;
                return (T)value;
            }

            m_data.Position = m_reader.ColumnMeta[fieldIndex].RecordOffset;
            m_data.Offset = m_dataOffset;

            if (arrayIndex >= 0)
            {
                if (arrayReaders.TryGetValue(typeof(T), out var reader))
                    value = reader(m_data, m_recordOffset, m_reader.Meta[fieldIndex], m_reader.ColumnMeta[fieldIndex], m_reader.PalletData[fieldIndex], m_reader.CommonData[fieldIndex], m_stringsTable, arrayIndex);
                else
                    throw new Exception("Unhandled array type: " + typeof(T).Name);
            }
            else
            {
                if (simpleReaders.TryGetValue(typeof(T), out var reader))
                    value = reader(m_id, m_data, m_recordOffset, m_reader.Meta[fieldIndex], m_reader.ColumnMeta[fieldIndex], m_reader.PalletData[fieldIndex], m_reader.CommonData[fieldIndex], m_stringsTable);
                else
                    throw new Exception("Unhandled field type: " + typeof(T).Name);
            }

            return (T)value;
        }

        public IDB2Row Clone() => (IDB2Row)MemberwiseClone();
    }
}