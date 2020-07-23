using System.Runtime.CompilerServices;

namespace SadRobot.Cmd.DBCD.DBCDReader.Common
{
    struct Value32
    {
        unsafe fixed byte Value[4];

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