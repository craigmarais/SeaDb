namespace SeaDbTestApp
{
    internal static class Utilities
    {
        public static void Run(CancellationTokenSource cts)
        {
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };
        }

        private static ulong _messageSequence;
        public static ulong NextMessageSeq()
        {
            return Interlocked.Increment(ref _messageSequence);
        }
    }
}
