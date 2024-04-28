using System.Runtime.InteropServices;

namespace SeaDbTestApp.Structures
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal unsafe struct NewTransport
    {
        public int Id;
        public int ShipmentId;
        public int SourceId;
        public int DestinationId;
        public int LoadSize;

        public static int Length => sizeof(NewTransport);

        public NewTransport()
        { }

        public NewTransport(Span<byte> data)
        {
            fixed (byte* ptr = data)
            {
                this = *(NewTransport*)ptr;
            }
        }
    }
}
