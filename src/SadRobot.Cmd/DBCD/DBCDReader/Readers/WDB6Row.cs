using System;
using System.Collections.Generic;
using System.Linq;
using SadRobot.Cmd.DBCD.DBCDReader.Common;

namespace SadRobot.Cmd.DBCD.DBCDReader.Readers
{
    class WDB6Row : IDBRow
    {
        private BitReader m_data;
        private BaseReader m_reader;
        private readonly int m_dataOffset;
        private readonly int m_dataPosition;
        private readonly int m_recordIndex;

        public int Id { get; set; }
        public BitReader Data { get => m_data; set => m_data = value; }

        private readonly FieldMetaData[] m_fieldMeta;
        private readonly Dictionary<int, Value32>[] m_commonData;

        public WDB6Row(BaseReader reader, BitReader data, int id, int recordIndex)
        {
            m_reader = reader;
            m_data = data;
            m_recordIndex = recordIndex;

            m_dataOffset = m_data.Offset;
            m_dataPosition = m_data.Position;

            m_fieldMeta = reader.Meta;
            m_commonData = reader.CommonData;

            Id = id;
        }

        private static Dictionary<Type, Func<int, BitReader, FieldMetaData, Dictionary<int, Value32>, Dictionary<long, string>, BaseReader, object>> simpleReaders = new Dictionary<Type, Func<int, BitReader, FieldMetaData, Dictionary<int, Value32>, Dictionary<long, string>, BaseReader, object>>
        {
            [typeof(long)] = (id, data, fieldMeta, commonData, stringTable, header) => GetFieldValue<long>(id, data, fieldMeta, commonData),
            [typeof(float)] = (id, data, fieldMeta, commonData, stringTable, header) => GetFieldValue<float>(id, data, fieldMeta, commonData),
            [typeof(int)] = (id, data, fieldMeta, commonData, stringTable, header) => GetFieldValue<int>(id, data, fieldMeta, commonData),
            [typeof(uint)] = (id, data, fieldMeta, commonData, stringTable, header) => GetFieldValue<uint>(id, data, fieldMeta, commonData),
            [typeof(short)] = (id, data, fieldMeta, commonData, stringTable, header) => GetFieldValue<short>(id, data, fieldMeta, commonData),
            [typeof(ushort)] = (id, data, fieldMeta, commonData, stringTable, header) => GetFieldValue<ushort>(id, data, fieldMeta, commonData),
            [typeof(sbyte)] = (id, data, fieldMeta, commonData, stringTable, header) => GetFieldValue<sbyte>(id, data, fieldMeta, commonData),
            [typeof(byte)] = (id, data, fieldMeta, commonData, stringTable, header) => GetFieldValue<byte>(id, data, fieldMeta, commonData),
            [typeof(string)] = (id, data, fieldMeta, commonData, stringTable, header) => header.Flags.HasFlagExt(DB2Flags.Sparse) ? data.ReadCString() : stringTable[GetFieldValue<int>(id, data, fieldMeta, commonData)],
        };

        private static Dictionary<Type, Func<int, BitReader, FieldMetaData, Dictionary<int, Value32>, Dictionary<long, string>, int, object>> arrayReaders = new Dictionary<Type, Func<int, BitReader, FieldMetaData, Dictionary<int, Value32>, Dictionary<long, string>, int, object>>
        {
            [typeof(ulong[])] = (id, data, fieldMeta, commonData, stringTable, cardinality) => GetFieldValueArray<ulong>(id, data, fieldMeta, commonData, cardinality),
            [typeof(long[])] = (id, data, fieldMeta, commonData, stringTable, cardinality) => GetFieldValueArray<long>(id, data, fieldMeta, commonData, cardinality),
            [typeof(float[])] = (id, data, fieldMeta, commonData, stringTable, cardinality) => GetFieldValueArray<float>(id, data, fieldMeta, commonData, cardinality),
            [typeof(int[])] = (id, data, fieldMeta, commonData, stringTable, cardinality) => GetFieldValueArray<int>(id, data, fieldMeta, commonData, cardinality),
            [typeof(uint[])] = (id, data, fieldMeta, commonData, stringTable, cardinality) => GetFieldValueArray<uint>(id, data, fieldMeta, commonData, cardinality),
            [typeof(ulong[])] = (id, data, fieldMeta, commonData, stringTable, cardinality) => GetFieldValueArray<ulong>(id, data, fieldMeta, commonData, cardinality),
            [typeof(ushort[])] = (id, data, fieldMeta, commonData, stringTable, cardinality) => GetFieldValueArray<ushort>(id, data, fieldMeta, commonData, cardinality),
            [typeof(short[])] = (id, data, fieldMeta, commonData, stringTable, cardinality) => GetFieldValueArray<short>(id, data, fieldMeta, commonData, cardinality),
            [typeof(byte[])] = (id, data, fieldMeta, commonData, stringTable, cardinality) => GetFieldValueArray<byte>(id, data, fieldMeta, commonData, cardinality),
            [typeof(sbyte[])] = (id, data, fieldMeta, commonData, stringTable, cardinality) => GetFieldValueArray<sbyte>(id, data, fieldMeta, commonData, cardinality),
            [typeof(string[])] = (id, data, fieldMeta, commonData, stringTable, cardinality) => GetFieldValueArray<int>(id, data, fieldMeta, commonData, cardinality).Select(i => stringTable[i]).ToArray(),
        };

        public void GetFields<T>(FieldCache<T>[] fields, T entry)
        {
            int indexFieldOffSet = 0;

            m_data.Position = m_dataPosition;
            m_data.Offset = m_dataOffset;

            for (int i = 0; i < fields.Length; i++)
            {
                FieldCache<T> info = fields[i];
                if (i == m_reader.IdFieldIndex)
                {
                    if (Id != -1)
                        indexFieldOffSet++;
                    else
                        Id = GetFieldValue<int>(0, m_data, m_fieldMeta[i], m_commonData?[i]);

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
                        value = reader(Id, m_data, m_fieldMeta[fieldIndex], m_commonData?[fieldIndex], m_reader.StringTable, info.Cardinality);
                    else
                        throw new Exception("Unhandled array type: " + typeof(T).Name);
                }
                else
                {
                    if (simpleReaders.TryGetValue(info.Field.FieldType, out var reader))
                        value = reader(Id, m_data, m_fieldMeta[fieldIndex], m_commonData?[fieldIndex], m_reader.StringTable, m_reader);
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

        private static T GetFieldValue<T>(int Id, BitReader r, FieldMetaData fieldMeta, Dictionary<int, Value32> commonData) where T : struct
        {
            if (commonData?.TryGetValue(Id, out var value) == true)
                return value.GetValue<T>();

            return r.ReadValue64(32 - fieldMeta.Bits).GetValue<T>();
        }

        private static T[] GetFieldValueArray<T>(int Id, BitReader r, FieldMetaData fieldMeta, Dictionary<int, Value32> commonData, int cardinality) where T : struct
        {
            T[] array = new T[cardinality];
            for (int i = 0; i < array.Length; i++)
            {
                if (commonData?.TryGetValue(Id, out var value) == true)
                    array[1] = value.GetValue<T>();
                else
                    array[i] = r.ReadValue64(32 - fieldMeta.Bits).GetValue<T>();
            }

            return array;
        }

        public IDBRow Clone()
        {
            return (IDBRow)MemberwiseClone();
        }
    }
}