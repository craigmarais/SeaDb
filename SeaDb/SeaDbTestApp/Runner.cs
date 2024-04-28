using System.Diagnostics;
using SeaDb;
using SeaDbTestApp.Structures;

namespace SeaDbTestApp
{
    internal class Runner : IDisposable
    {
        public Action OnCompletion { get; set; }

        private readonly Thread _thread;
        readonly SeaDatabase _database;

        public unsafe Runner(int runnerId, int insertCount, string table, CancellationToken token)
        {
            _database = new SeaDatabase(table);
            int count = 0;
            new Thread(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    if (count > 0)
                        Console.WriteLine($"{runnerId} = {count:##,###} p/s");

                    count = 0;
                    Thread.Sleep(1000);
                }
            }).Start();


            _thread = new Thread(() =>
            {
                int messageId = 0;
                var sw = Stopwatch.StartNew();

                while (!token.IsCancellationRequested)
                {
                    if (messageId == insertCount) break;
                    
                    var transport = new NewTransport
                    {
                        DestinationId = 1,
                        ShipmentId = 2,
                        LoadSize = 24_900,
                        Id = ++messageId
                    };
                    Span<byte> data = new Span<byte>(&transport, NewTransport.Length);
                    _database.Write(Utilities.NextMessageSeq(), data);
                    count++;
                }

                _database.Flush();
                sw.Stop();
                Console.WriteLine($"{runnerId} finished writing {insertCount:##,###} messages in {sw.ElapsedMilliseconds}ms :)");
                OnCompletion?.Invoke();
            });
        }

        public void Run()
        {
            _thread.Start();
        }

        public List<NewTransport> GetAll(string tableName)
        {
            var result = new List<NewTransport>();
            var data = _database.ReadFrom(0);
            int offset = 0;

            while (offset < data.Length)
            {
                result.Add(new NewTransport(data[offset..].Span));
                offset += NewTransport.Length;
            }

            return result;
        }

        public void Dispose()
        {
            _database.Dispose();
        }
    }
}
