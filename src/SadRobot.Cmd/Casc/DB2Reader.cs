using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CASCLib;

namespace SadRobot.Cmd.Casc
{
    public class FieldCache<T, V> : FieldCache
    {
        public readonly Action<T, V> Setter;
        public readonly Func<T, V> Getter;

        public FieldCache(FieldInfo field)
        {
            Field = field;
            IsArray = field.FieldType.IsArray;

            if (IsArray)
            {
                ArraySizeAttribute atr = (ArraySizeAttribute)field.GetCustomAttribute(typeof(ArraySizeAttribute));

                if (atr == null)
                    throw new Exception(typeof(T).Name + "." + field.Name + " missing ArraySizeAttribute");

                ArraySize = atr.Size;
            }

            Setter = field.GetSetter<T, V>();
            Getter = field.GetGetter<T, V>();
        }
    }

    public abstract class DB2Reader<T> : IEnumerable<KeyValuePair<int, T>> where T : IDB2Row
    {
        public int RecordsCount { get; protected set; }
        public int FieldsCount { get; protected set; }
        public int RecordSize { get; protected set; }
        public int StringTableSize { get; protected set; }
        public uint TableHash { get; protected set; }
        public uint LayoutHash { get; protected set; }
        public int MinIndex { get; protected set; }
        public int MaxIndex { get; protected set; }
        public int IdFieldIndex { get; protected set; }

        protected FieldMetaData[] m_meta;
        public FieldMetaData[] Meta => m_meta;

        protected ColumnMetaData[] m_columnMeta;
        public ColumnMetaData[] ColumnMeta => m_columnMeta;

        protected Value32[][] m_palletData;
        public Value32[][] PalletData => m_palletData;

        protected Dictionary<int, Value32>[] m_commonData;
        public Dictionary<int, Value32>[] CommonData => m_commonData;

        protected SortedDictionary<int, T> _Records = new SortedDictionary<int, T>();

        public bool HasRow(int id) => _Records.ContainsKey(id);

        public T GetRow(int id)
        {
            _Records.TryGetValue(id, out T row);
            return row;
        }

        public IEnumerator<KeyValuePair<int, T>> GetEnumerator() => _Records.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
