using SeaDbTestApp;
using SeaDbTestApp.Structures;
using System.Runtime.InteropServices;

var cts = new CancellationTokenSource();
int insertCount = 50_000_000;
var tableName = "Transport";

//RawRead();

Runner runner = new Runner(1, insertCount, tableName, cts.Token);
runner.OnCompletion = Count;

runner.Run();

Utilities.Run(cts);

void Count()
{
    var messages = runner.GetAll(tableName);
    Console.WriteLine(messages.Count != insertCount
        ? $"Runner 1 failed to insert all records: expected {insertCount:##,###} got {messages.Count:##,###}"
        : "Runner 1 Successfully inserted all records.");
}

void RawRead()
{
    List<NewTransport> list = new List<NewTransport>();
    var buffer = new byte[2_000_000];
    using var reader = File.OpenRead(Path.Join(tableName, "1.cdat"));
    var readCount = reader.Read(buffer);
    buffer = buffer[8..]; // skip the mmf offset

    while (readCount != 0)
    {
        var readStructs = MemoryMarshal.Cast<byte, NewTransport>(buffer.AsSpan()).ToArray();
        foreach (var item in readStructs)
        {
            if (item.Id == 0)
                break;
            list.Add(item);
        }
        buffer = new byte[2_000_000];
        readCount = reader.Read(buffer);
    }
    
    Console.WriteLine(list.Count != insertCount
        ? $"Runner 1 failed to insert all records: expected {insertCount:##,###} got {list.Count:##,###}"
        : "Runner 1 Successfully inserted all records.");
}

