using System.Runtime.CompilerServices;
using System.Text;

namespace SadRobot.Cmd.Casc
{
    public class BitReader
    {
        private byte[] m_data;
        private int m_bitPosition;
        private int m_offset;

        public int Position { get => m_bitPosition; set => m_bitPosition = value; }
        public int Offset { get => m_offset; set => m_offset = value; }
        public byte[] Data { get => m_data; set => m_data = value; }

        public BitReader(byte[] data)
        {
            m_data = data;
        }

        public BitReader(byte[] data, int offset)
        {
            m_data = data;
            m_offset = offset;
        }

        public T Read<T>(int numBits) where T : unmanaged
        {
            ulong result = Unsafe.As<byte, ulong>(ref m_data[m_offset + (m_bitPosition >> 3)]) << (64 - numBits - (m_bitPosition & 7)) >> (64 - numBits);
            m_bitPosition += numBits;
            return Unsafe.As<ulong, T>(ref result);
        }

        public T ReadSigned<T>(int numBits) where T : unmanaged
        {
            ulong result = Unsafe.As<byte, ulong>(ref m_data[m_offset + (m_bitPosition >> 3)]) << (64 - numBits - (m_bitPosition & 7)) >> (64 - numBits);
            m_bitPosition += numBits;
            ulong signedShift = (1UL << (numBits - 1));
            result = (signedShift ^ result) - signedShift;
            return Unsafe.As<ulong, T>(ref result);
        }

        public string ReadCString()
        {
            int start = m_bitPosition;

            while (m_data[m_offset + (m_bitPosition >> 3)] != 0)
                m_bitPosition += 8;

            string result = Encoding.UTF8.GetString(m_data, m_offset + (start >> 3), (m_bitPosition - start) >> 3);
            m_bitPosition += 8;
            return result;
        }
    }
}