using System.Diagnostics;
using System.Runtime.InteropServices;
using SeaDb;
using SeaDbTestApp.Structures;

namespace SeaDbTestApp
{
    internal class Runner : IDisposable
    {
        public Action OnCompletion { get; set; }

        //readonly Thread _thread;
        SeaDatabase _db;
        readonly CancellationToken _token;
        readonly ulong _insertCount;
        int _count;
        int _runnerId;

        public unsafe Runner(int runnerId, int insertCount, string table, CancellationToken token)
        {
            _token = token;
            _insertCount = (ulong)insertCount;
            _runnerId = runnerId;
            _db = new SeaDatabase(table);

            new Thread(LogPerformance).Start();
            //_thread = new Thread(RunThread);
        }

        public unsafe void RunThread()
        {
            ulong messageId = 0;
            var sw = Stopwatch.StartNew();

            var transport = new NewTransport
            {
                DestinationId = 1,
                ShipmentId = 2,
                LoadSize = 24_900,
                Id = 0
            };
            while (!_token.IsCancellationRequested)
            {
                if (messageId == _insertCount) break;

                transport.Id = (int)(++messageId);
                Span<byte> data = new Span<byte>(&transport, NewTransport.Length);
                _db.Write(Utilities.NextMessageSeq(), data);

                _count++;
            }
            throw new Exception("An expected exception has occurred, please dont panic.");
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
            //_thread.Start();
            RunThread();
        }

        public List<NewTransport> GetAll(string tableName)
        {
            var result = new List<NewTransport>();
            var data = _db.ReadFrom(0);
            foreach(var item in data)
            {
                result.AddRange(MemoryMarshal.Cast<byte, NewTransport>(item.Span).ToArray());
            }
            return result;
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
