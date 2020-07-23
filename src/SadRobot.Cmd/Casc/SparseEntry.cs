using System.Runtime.InteropServices;

namespace SadRobot.Cmd.Casc
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SparseEntry
    {
        public int Offset;
        public ushort Size;
    }
}