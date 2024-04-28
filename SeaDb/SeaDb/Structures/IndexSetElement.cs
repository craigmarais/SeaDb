using System.Runtime.InteropServices;

namespace SeaDb.Structures
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public unsafe struct IndexSetElement
    {
        public ulong FirstKey;
        public ulong LastKey;
        public ulong GroupKey;

        public IndexSetElement(ulong firstKey, ulong lastKey, ulong groupKey)
        {
            FirstKey = firstKey;
            LastKey = lastKey;
            GroupKey = groupKey;
        }

        public IndexSetElement(Span<byte> data)
        {
            fixed (byte* ptr = data)
            {
                this = *(IndexSetElement*)ptr;
            }
        }
    }
}
