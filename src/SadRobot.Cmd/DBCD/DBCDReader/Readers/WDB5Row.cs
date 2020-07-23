using System;
using System.Collections.Generic;
using System.Linq;
using SadRobot.Cmd.DBCD.DBCDReader.Common;

namespace SadRobot.Cmd.DBCD.DBCDReader.Readers
{
    class WDB5Row : IDBRow
    {
        private BitReader m_data;
        private BaseReader m_reader;
        private readonly int m_dataOffset;
        private readonly int m_dataPosition;
        private readonly int m_recordIndex;

        public int Id { get; set; }
        public BitReader Data { get => m_data; set => m_data = value; }

        private readonly FieldMetaData[] m_fieldMeta;

        public WDB5Row(BaseReader reader, BitReader data, int id, int recordIndex)
        {
            m_reader = reader;
            m_data = data;
            m_recordIndex = recordIndex;

            Id = id;

            m_dataOffset = m_data.Offset;
            m_dataPosition = m_data.Position;
            m_fieldMeta = reader.Meta;
        }

        private static Dictionary<Type, Func<BitReader, FieldMetaData, Dictionary<long, string>, BaseReader, object>> simpleReaders = new Dictionary<Type, Func<BitReader, FieldMetaData, Dictionary<long, string>, BaseReader, object>>
        {
            [typeof(long)] = (data, fieldMeta, stringTable, header) => GetFieldValue<long>(data, fieldMeta),
            [typeof(float)] = (data, fieldMeta, stringTable, header) => GetFieldValue<float>(data, fieldMeta),
            [typeof(int)] = (data, fieldMeta, stringTable, header) => GetFieldValue<int>(data, fieldMeta),
            [typeof(uint)] = (data, fieldMeta, stringTable, header) => GetFieldValue<uint>(data, fieldMeta),
            [typeof(short)] = (data, fieldMeta, stringTable, header) => GetFieldValue<short>(data, fieldMeta),
            [typeof(ushort)] = (data, fieldMeta, stringTable, header) => GetFieldValue<ushort>(data, fieldMeta),
            [typeof(sbyte)] = (data, fieldMeta, stringTable, header) => GetFieldValue<sbyte>(data, fieldMeta),
            [typeof(byte)] = (data, fieldMeta, stringTable, header) => GetFieldValue<byte>(data, fieldMeta),
            [typeof(string)] = (data, fieldMeta, stringTable, header) => header.Flags.HasFlagExt(DB2Flags.Sparse) ? data.ReadCString() : stringTable[GetFieldValue<int>(data, fieldMeta)],
        };

        private static Dictionary<Type, Func<BitReader, FieldMetaData, Dictionary<long, string>, int, object>> arrayReaders = new Dictionary<Type, Func<BitReader, FieldMetaData, Dictionary<long, string>, int, object>>
        {
            [typeof(ulong[])] = (data, fieldMeta, stringTable, cardinality) => GetFieldValueArray<ulong>(data, fieldMeta, cardinality),
            [typeof(long[])] = (data, fieldMeta, stringTable, cardinality) => GetFieldValueArray<long>(data, fieldMeta, cardinality),
            [typeof(float[])] = (data, fieldMeta, stringTable, cardinality) => GetFieldValueArray<float>(data, fieldMeta, cardinality),
            [typeof(int[])] = (data, fieldMeta, stringTable, cardinality) => GetFieldValueArray<int>(data, fieldMeta, cardinality),
            [typeof(uint[])] = (data, fieldMeta, stringTable, cardinality) => GetFieldValueArray<uint>(data, fieldMeta, cardinality),
            [typeof(ulong[])] = (data, fieldMeta, stringTable, cardinality) => GetFieldValueArray<ulong>(data, fieldMeta, cardinality),
            [typeof(ushort[])] = (data, fieldMeta, stringTable, cardinality) => GetFieldValueArray<ushort>(data, fieldMeta, cardinality),
            [typeof(short[])] = (data, fieldMeta, stringTable, cardinality) => GetFieldValueArray<short>(data, fieldMeta, cardinality),
            [typeof(byte[])] = (data, fieldMeta, stringTable, cardinality) => GetFieldValueArray<byte>(data, fieldMeta, cardinality),
            [typeof(sbyte[])] = (data, fieldMeta, stringTable, cardinality) => GetFieldValueArray<sbyte>(data, fieldMeta, cardinality),
            [typeof(string[])] = (data, fieldMeta, stringTable, cardinality) => GetFieldValueArray<int>(data, fieldMeta, cardinality).Select(i => stringTable[i]).ToArray(),
        };

        public void GetFields<T>(FieldCache<T>[] fields, T entry)
        {
            int indexFieldOffSet = 0;

            m_data.Position = m_dataPosition;
            m_data.Offset = m_dataOffset;

            for (int i = 0; i < fields.Length; i++)
            {
                FieldCache<T> info = fields[i];
                if (info.IndexMapField)
                {
                    if (Id != -1)
                        indexFieldOffSet++;
                    else
                        Id = GetFieldValue<int>(m_data, m_fieldMeta[i]);

                    info.Setter(entry, Convert.ChangeType(Id, info.Field.FieldType));
                    continue;
                }

                object value = null;
                int fieldIndex = i - indexFieldOffSet;

                // 0x2 SecondaryKey
                if (fieldIndex >= m_reader.Meta.Length)
                {
                    info.Setter(entry, Convert.ChangeType(m_reader.ForeignKeyData[Id - m_reader.MinIndex], info.Field.FieldType));
                    continue;
                }

                if (info.IsArray)
                {
                    if (info.Cardinality <= 1)
                        SetCardinality(info, fieldIndex);

                    if (arrayReaders.TryGetValue(info.Field.FieldType, out var reader))
                        value = reader(m_data, m_fieldMeta[fieldIndex], m_reader.StringTable, info.Cardinality);
                    else
                        throw new Exception("Unhandled array type: " + typeof(T).Name);
                }
                else
                {
                    if (simpleReaders.TryGetValue(info.Field.FieldType, out var reader))
                        value = reader(m_data, m_fieldMeta[fieldIndex], m_reader.StringTable, m_reader);
                    else
                        throw new Exception("Unhandled field type: " + typeof(T).Name);
                }

                info.Setter(entry, value);
            }
        }

        /// <summary>
        /// Cardinality can be calculated from the file itself
        /// - Last field of the record : (header.RecordSize - current offset) / sizeof(ValueType)
        /// - Middle field : (next field offset - current offset) / sizeof(ValueType)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="info"></param>
        /// <param name="fieldIndex"></param>
        private void SetCardinality<T>(FieldCache<T> info, int fieldIndex)
        {
            int fieldOffset = m_fieldMeta[fieldIndex].Offset;
            int fieldValueSize = (32 - m_fieldMeta[fieldIndex].Bits) >> 3;

            int nextOffset;
            if (fieldIndex + 1 >= m_fieldMeta.Length)
                nextOffset = m_reader.RecordSize; // get total record size
            else
                nextOffset = m_fieldMeta[fieldIndex + 1].Offset; // get next field offset

            info.Cardinality = (nextOffset - fieldOffset) / fieldValueSize;
        }

        private static T GetFieldValue<T>(BitReader r, FieldMetaData fieldMeta) where T : struct
        {
            return r.ReadValue64(32 - fieldMeta.Bits).GetValue<T>();
        }

        private static T[] GetFieldValueArray<T>(BitReader r, FieldMetaData fieldMeta, int cardinality) where T : struct
        {
            T[] array = new T[cardinality];
            for (int i = 0; i < array.Length; i++)
                array[i] = r.ReadValue64(32 - fieldMeta.Bits).GetValue<T>();

            return array;
        }

        public IDBRow Clone()
        {
            return (IDBRow)MemberwiseClone();
        }
    }
}