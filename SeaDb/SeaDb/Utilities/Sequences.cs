using System.Dynamic;

namespace SeaDb.Utilities
{
    public static class Sequences
    {
        private static ulong _groupKey;
        private static ulong _messageKey;

        public static ulong GetNextGroupKey()
        {
            return Interlocked.Increment(ref _groupKey);
        }

        public static ulong GetNextMessageKey()
        {
            return Interlocked.Increment(ref _messageKey);
        }

    }
}
