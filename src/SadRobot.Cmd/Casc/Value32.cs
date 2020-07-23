using System.Runtime.CompilerServices;

namespace SadRobot.Cmd.Casc
{
    public struct Value32
    {
        private uint Value;

        public T As<T>() where T : unmanaged
        {
            return Unsafe.As<uint, T>(ref Value);
        }
    }
}