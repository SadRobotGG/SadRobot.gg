using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using SadRobot.Cmd.DBCD.DBCDReader.Common;

namespace SadRobot.Cmd.DBCD.DBCDReader.Readers
{
    class HTFXRow : IDBRow, IHotfixEntry, IEquatable<HTFXRow>
    {
        private BitReader m_data;
        private readonly IHotfixEntry m_hotfixEntry;

        public int Id { get; set; }
        public BitReader Data { get => m_data; set => m_data = value; }

        public int PushId => m_hotfixEntry.PushId;
        public uint TableHash => m_hotfixEntry.TableHash;
        public int RecordId => m_hotfixEntry.RecordId;
        public bool IsValid => m_hotfixEntry.IsValid;
        public int DataSize => m_hotfixEntry.DataSize;

        public HTFXRow(BitReader data, IHotfixEntry hotfixEntry)
        {
            m_data = data;
            m_hotfixEntry = hotfixEntry;

            Id = hotfixEntry.RecordId;
        }

        private static Dictionary<Type, Func<BitReader, object>> simpleReaders = new Dictionary<Type, Func<BitReader, object>>
        {
            [typeof(long)] = (data) => GetFieldValue<long>(data),
            [typeof(float)] = (data) => GetFieldValue<float>(data),
            [typeof(int)] = (data) => GetFieldValue<int>(data),
            [typeof(uint)] = (data) => GetFieldValue<uint>(data),
            [typeof(short)] = (data) => GetFieldValue<short>(data),
            [typeof(ushort)] = (data) => GetFieldValue<ushort>(data),
            [typeof(sbyte)] = (data) => GetFieldValue<sbyte>(data),
            [typeof(byte)] = (data) => GetFieldValue<byte>(data),
            [typeof(string)] = (data) => data.ReadCString(),
        };

        private static Dictionary<Type, Func<BitReader, int, object>> arrayReaders = new Dictionary<Type, Func<BitReader, int, object>>
        {
            [typeof(ulong[])] = (data, cardinality) => GetFieldValueArray<ulong>(data, cardinality),
            [typeof(long[])] = (data, cardinality) => GetFieldValueArray<long>(data, cardinality),
            [typeof(float[])] = (data, cardinality) => GetFieldValueArray<float>(data, cardinality),
            [typeof(int[])] = (data, cardinality) => GetFieldValueArray<int>(data, cardinality),
            [typeof(uint[])] = (data, cardinality) => GetFieldValueArray<uint>(data, cardinality),
            [typeof(ulong[])] = (data, cardinality) => GetFieldValueArray<ulong>(data, cardinality),
            [typeof(ushort[])] = (data, cardinality) => GetFieldValueArray<ushort>(data, cardinality),
            [typeof(short[])] = (data, cardinality) => GetFieldValueArray<short>(data, cardinality),
            [typeof(byte[])] = (data, cardinality) => GetFieldValueArray<byte>(data, cardinality),
            [typeof(sbyte[])] = (data, cardinality) => GetFieldValueArray<sbyte>(data, cardinality),
            [typeof(string[])] = (data, cardinality) => Enumerable.Range(0, cardinality).Select(i => data.ReadCString()).ToArray(),
        };

        public void GetFields<T>(FieldCache<T>[] fields, T entry)
        {
            Data.Position = 0;

            for (int i = 0; i < fields.Length; i++)
            {
                FieldCache<T> info = fields[i];
                if (info.IndexMapField)
                {
                    info.Setter(entry, Convert.ChangeType(Id, info.Field.FieldType));
                    continue;
                }

                object value = null;

                if (info.IsArray)
                {
                    if (arrayReaders.TryGetValue(info.Field.FieldType, out var reader))
                        value = reader(m_data, info.Cardinality);
                    else
                        throw new Exception("Unhandled array type: " + typeof(T).Name);
                }
                else
                {
                    if (simpleReaders.TryGetValue(info.Field.FieldType, out var reader))
                        value = reader(m_data);
                    else
                        throw new Exception("Unhandled field type: " + typeof(T).Name);
                }

                info.Setter(entry, value);
            }
        }

        private static T GetFieldValue<T>(BitReader r) where T : struct
        {
            return r.ReadValue64(Unsafe.SizeOf<T>() * 8).GetValue<T>();
        }

        private static T[] GetFieldValueArray<T>(BitReader r, int cardinality) where T : struct
        {
            T[] array = new T[cardinality];
            for (int i = 0; i < array.Length; i++)
                array[i] = r.ReadValue64(Unsafe.SizeOf<T>() * 8).GetValue<T>();

            return array;
        }


        #region Interface Methods

        public IDBRow Clone()
        {
            return (IDBRow)MemberwiseClone();
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 486187739) + PushId;
                hash = (hash * 486187739) + TableHash.GetHashCode();
                hash = (hash * 486187739) + RecordId;
                hash = (hash * 486187739) + (IsValid ? 1 : 0);
                hash = (hash * 486187739) + DataSize;
                hash = (hash * 486187739) + m_data.GetHashCode();
                return hash;
            }
        }

        public bool Equals(HTFXRow other)
        {
            return PushId == other.PushId &&
                   TableHash == other.TableHash &&
                   RecordId == other.RecordId &&
                   IsValid == other.IsValid &&
                   DataSize == other.DataSize;
        }

        #endregion
    }
}