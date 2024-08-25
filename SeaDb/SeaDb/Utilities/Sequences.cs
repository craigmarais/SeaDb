namespace SeaDb.Utilities
{
    internal static class Sequences
    {
        private static ulong _indexKey;
        private static ulong _groupKey;
        private static ulong _messageKey;

        public static ulong GetNextIndexKey()
        {
            return Interlocked.Increment(ref _indexKey);
        }

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
