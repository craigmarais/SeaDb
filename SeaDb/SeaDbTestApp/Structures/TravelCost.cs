using System.Runtime.InteropServices;

namespace SeaDbTestApp.Structures
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal unsafe struct TravelCost
    {
        public int ShipmentId;
        public int TravelKilometers;
        public int CostPerKilometer;
        public int TotalCost;

        public static int Length => sizeof(TravelCost);

        public TravelCost()
        { }

        public TravelCost(Span<byte> data)
        {
            fixed (byte* ptr = data)
            {
                this = *(TravelCost*)ptr;
            }
        }
    }
}
