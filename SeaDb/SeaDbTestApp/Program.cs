using SeaDbTestApp;

var cts = new CancellationTokenSource();
int insertCount = 100_000_000;

var tableName = "Transport";
Runner runner = new Runner(1, insertCount, tableName, cts.Token);
runner.OnCompletion = () =>
{
    var messages = runner.GetAll(tableName);
    Console.WriteLine(messages.Length != insertCount
        ? $"Runner 1 failed to insert all records: expected {insertCount} got {messages.Length}"
        : "Runner 1 Successfully inserted all records.");
};

runner.Run();

Utilities.Run(cts);