using System;
using System.Text;

namespace SadRobot.Cmd.Casc
{
    public class DB2Row
    {
        private readonly byte[] m_data;
        private readonly WDB2Reader m_reader;

        public byte[] Data => m_data;

        public DB2Row(WDB2Reader reader, byte[] data)
        {
            m_reader = reader;
            m_data = data;
        }

        public T GetField<T>(int field)
        {
            object retVal;

            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.String:
                    int start = BitConverter.ToInt32(m_data, field * 4), len = 0;
                    while (m_reader.StringTable[start + len] != 0)
                        len++;
                    retVal = Encoding.UTF8.GetString(m_reader.StringTable, start, len);
                    return (T)retVal;
                case TypeCode.Int32:
                    retVal = BitConverter.ToInt32(m_data, field * 4);
                    return (T)retVal;
                case TypeCode.Single:
                    retVal = BitConverter.ToSingle(m_data, field * 4);
                    return (T)retVal;
                default:
                    return default(T);
            }
        }
    }
}