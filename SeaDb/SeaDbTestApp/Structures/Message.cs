using System.Runtime.InteropServices;

namespace SeaDbTestApp.Structures
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal unsafe struct Message<T> where T : unmanaged
    {
        public int Sequence;
        public int SenderId;
        public T NestedStruct;

        public static int Length => sizeof(Message<T>);

        public Message(Span<byte> data)
        {
            fixed (byte* ptr = data)
            {
                this = *(Message<T>*)ptr;
            }
        }
    }
}
