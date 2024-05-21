using System.Diagnostics;
using System.Runtime.InteropServices;
using SeaDb;
using SeaDbTestApp.Structures;

namespace SeaDbTestApp
{
    internal class Runner : IDisposable
    {
        public Action OnCompletion { get; set; }

        readonly Thread _thread;
        readonly CancellationToken _token;
        readonly SeaDatabase _database;
        readonly ulong _insertCount;
        int _count;
        int _runnerId;

        public unsafe Runner(int runnerId, int insertCount, string table, CancellationToken token)
        {
            _token = token;
            _insertCount = (ulong)insertCount;
            _runnerId = runnerId;

            _database = new SeaDatabase(table);
            new Thread(LogPerformance).Start();

            _thread = new Thread(RunThread);
        }

        private unsafe void RunThread()
        {
            ulong messageId = 0;
            var sw = Stopwatch.StartNew();

            while (!_token.IsCancellationRequested)
            {
                if (messageId == _insertCount) break;

                var transport = new NewTransport
                {
                    DestinationId = 1,
                    ShipmentId = 2,
                    LoadSize = 24_900,
                    Id = (int)(++messageId)
                };
                Span<byte> data = new Span<byte>(&transport, NewTransport.Length);
                _database.Write(messageId, data);
                _count++;
            }

            _database.Flush();
            sw.Stop();
            Console.WriteLine($"{_runnerId} finished writing {_insertCount:##,###} messages in {sw.ElapsedMilliseconds}ms :)");
            OnCompletion?.Invoke();
        }

        private void LogPerformance(object? obj)
        {
            while (!_token.IsCancellationRequested)
            {
                if (_count > 0)
                    Console.WriteLine($"{_runnerId} = {_count:##,###} p/s");

                _count = 0;
                Thread.Sleep(1000);
            }
        }

        public void Run()
        {
            _thread.Start();
        }

        public Span<NewTransport> GetAll(string tableName)
        {
            var data = _database.ReadFrom(0);
            return MemoryMarshal.Cast<byte, NewTransport>(data.Span);
        }

        public void Dispose()
        {
            _database.Dispose();
        }
    }
}
