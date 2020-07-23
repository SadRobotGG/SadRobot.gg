using System.Runtime.CompilerServices;

namespace SadRobot.Cmd.DBCD.DBCDReader.Common
{
    struct Value64
    {
        unsafe fixed byte Value[8];

        public T GetValue<T>() where T : struct
        {
            unsafe
            {
                fixed (byte* ptr = Value)
                    return Unsafe.ReadUnaligned<T>(ptr);
            }
        }
    }
}