using System;
using System.Collections.Generic;

namespace SadRobot.Cmd.Casc
{
    public class WDB5Row : IDB2Row
    {
        private byte[] m_data;
        private WDB5Reader m_reader;
        private Dictionary<long, string> m_stringsTable;
        private int m_id;

        public int GetId() => m_id;
        public void SetId(int id) => m_id = id;

        public byte[] Data => m_data;

        public WDB5Row(WDB5Reader reader, byte[] data, Dictionary<long, string> stringsTable)
        {
            m_reader = reader;
            m_data = data;
            m_stringsTable = stringsTable;
        }

        public T GetField<T>(int field, int arrayIndex = 0)
        {
            FieldMetaData meta = m_reader.Meta[field];

            if (meta.Bits != 0x00 && meta.Bits != 0x08 && meta.Bits != 0x10 && meta.Bits != 0x18 && meta.Bits != -32)
                throw new Exception("Unknown meta.Flags");

            int bytesCount = (32 - meta.Bits) >> 3;

            TypeCode code = Type.GetTypeCode(typeof(T));

            object value = null;

            switch (code)
            {
                case TypeCode.Byte:
                    if (meta.Bits != 0x18)
                        throw new Exception("TypeCode.Byte Unknown meta.Bits");
                    value = m_data[meta.Offset + bytesCount * arrayIndex];
                    break;
                case TypeCode.SByte:
                    if (meta.Bits != 0x18)
                        throw new Exception("TypeCode.SByte Unknown meta.Bits");
                    value = (sbyte)m_data[meta.Offset + bytesCount * arrayIndex];
                    break;
                case TypeCode.Int16:
                    if (meta.Bits != 0x10)
                        throw new Exception("TypeCode.Int16 Unknown meta.Bits");
                    value = BitConverter.ToInt16(m_data, meta.Offset + bytesCount * arrayIndex);
                    break;
                case TypeCode.UInt16:
                    if (meta.Bits != 0x10)
                        throw new Exception("TypeCode.UInt16 Unknown meta.Bits");
                    value = BitConverter.ToUInt16(m_data, meta.Offset + bytesCount * arrayIndex);
                    break;
                case TypeCode.Int32:
                    byte[] b1 = new byte[4];
                    Array.Copy(m_data, meta.Offset + bytesCount * arrayIndex, b1, 0, bytesCount);
                    value = BitConverter.ToInt32(b1, 0);
                    break;
                case TypeCode.UInt32:
                    byte[] b2 = new byte[4];
                    Array.Copy(m_data, meta.Offset + bytesCount * arrayIndex, b2, 0, bytesCount);
                    value = BitConverter.ToUInt32(b2, 0);
                    break;
                case TypeCode.Int64:
                    byte[] b3 = new byte[8];
                    Array.Copy(m_data, meta.Offset + bytesCount * arrayIndex, b3, 0, bytesCount);
                    value = BitConverter.ToInt64(b3, 0);
                    break;
                case TypeCode.UInt64:
                    byte[] b4 = new byte[8];
                    Array.Copy(m_data, meta.Offset + bytesCount * arrayIndex, b4, 0, bytesCount);
                    value = BitConverter.ToUInt64(b4, 0);
                    break;
                case TypeCode.String:
                    if (meta.Bits != 0x00)
                        throw new Exception("TypeCode.String Unknown meta.Bits");
                    byte[] b5 = new byte[4];
                    Array.Copy(m_data, meta.Offset + bytesCount * arrayIndex, b5, 0, bytesCount);
                    int start = BitConverter.ToInt32(b5, 0);
                    value = m_stringsTable[start];
                    break;
                case TypeCode.Single:
                    if (meta.Bits != 0x00)
                        throw new Exception("TypeCode.Single Unknown meta.Bits");
                    value = BitConverter.ToSingle(m_data, meta.Offset + bytesCount * arrayIndex);
                    break;
                default:
                    throw new Exception("Unknown TypeCode " + code);
            }

            return (T)value;
        }

        public IDB2Row Clone() => (IDB2Row)MemberwiseClone();
    }
}