using System.Runtime.InteropServices;

namespace SeaDb.Structures
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public unsafe ref struct Index
    {
        public ulong Key;
        public uint Position;

        public Index(ulong key, uint position)
        {
            Key = key;
            Position = position;
        }

        public Index(Span<byte> data)
        {
            fixed (byte* ptr = data)
            {
                this = *(Index*)ptr;
            }
        }
    }
}
