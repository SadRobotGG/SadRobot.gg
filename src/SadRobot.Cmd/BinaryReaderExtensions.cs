using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SadRobot.Cmd
{
    public static class BinaryReaderExtensions
    {
        public static string ReadCString(this BinaryReader reader)
        {
            using var stream = new MemoryStream();
            var value = reader.ReadByte();
            while (value != '\0')
            {
                stream.WriteByte(value);
                value = reader.ReadByte();
            }

            return Encoding.UTF8.GetString(stream.ToArray());
        }

        public static string ReadFixedLengthString(this BinaryReader br, uint bytes)
        {
            var holder = new ArrayList();
            uint lastByte = bytes - 1;
            for (uint i = 0; i < bytes; i++)
            {
                byte value = br.ReadByte();
                if (!(i == lastByte && value == 0))
                {
                    holder.Add(value);
                }
            }
            byte[] arr = holder.ToArray(typeof(byte)) as byte[];
            return Encoding.UTF8.GetString(arr);
        }

        public static uint GetBits(this BitArray ba, int start, uint bits, bool rightToLeft = false)
        {
            uint ret = 0;
            for (byte i = 0; i < bits; i++)
            {
                if (ba.Get(start + i))
                {
                    ret += Hardcoded.BitValues[rightToLeft ? bits - i - 1 : i];
                }
            }
            return ret;
        }

        public static uint GetBits(this BitArray ba, int start, int bits, bool rightToLeft = false)
        {
            return ba.GetBits(start, (uint)bits, rightToLeft);
        }

        public static class Hardcoded
        {
            public static Dictionary<ushort, ushort> FieldSizeMap = new Dictionary<ushort, ushort>
            {
                // (32 - n) / 8
                { 0, 4 },
                { 8, 3 },
                { 16, 2 },
                { 24, 1 },
                { 32, 0 }, // this is a special signifier for something involving FieldStorageInfo
                { 0xFFE0, 8 }, // -32 for 64-bit, ha ha ha
            };

            public static uint[] BitValues = Enumerable.Range(0, 64).Select(x => (uint)1 << x).ToArray();
        }
    }
}