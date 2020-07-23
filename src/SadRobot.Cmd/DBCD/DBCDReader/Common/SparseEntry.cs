using System.Runtime.InteropServices;

namespace SadRobot.Cmd.DBCD.DBCDReader.Common
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    struct SparseEntry
    {
        public uint Offset;
        public ushort Size;
    }
}