namespace SadRobot.Cmd.Casc
{
    public class DB3Row
    {
        private byte[] m_data;
        private WDB3Reader m_reader;

        public byte[] Data { get { return m_data; } }

        public DB3Row(WDB3Reader reader, byte[] data)
        {
            m_reader = reader;
            m_data = data;
        }

        // public unsafe T GetField<T>(int offset)
        // {
        //     object retVal;
        //
        //     fixed (byte* ptr = m_data)
        //     {
        //         switch (Type.GetTypeCode(typeof(T)))
        //         {
        //             case TypeCode.String:
        //                 string str;
        //                 int start = BitConverter.ToInt32(m_data, offset);
        //                 if (m_reader.StringTable.TryGetValue(start, out str))
        //                     retVal = str;
        //                 else
        //                     retVal = string.Empty;
        //                 return (T)retVal;
        //             case TypeCode.SByte:
        //                 retVal = ptr[offset];
        //                 return (T)retVal;
        //             case TypeCode.Byte:
        //                 retVal = ptr[offset];
        //                 return (T)retVal;
        //             case TypeCode.Int16:
        //                 retVal = *(short*)(ptr + offset);
        //                 return (T)retVal;
        //             case TypeCode.UInt16:
        //                 retVal = *(ushort*)(ptr + offset);
        //                 return (T)retVal;
        //             case TypeCode.Int32:
        //                 retVal = *(int*)(ptr + offset);
        //                 return (T)retVal;
        //             case TypeCode.UInt32:
        //                 retVal = *(uint*)(ptr + offset);
        //                 return (T)retVal;
        //             case TypeCode.Single:
        //                 retVal = *(float*)(ptr + offset);
        //                 return (T)retVal;
        //             default:
        //                 return default(T);
        //         }
        //     }
        // }
    }
}