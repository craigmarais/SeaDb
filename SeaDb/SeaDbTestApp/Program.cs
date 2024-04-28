using SeaDbTestApp;

var cts = new CancellationTokenSource();
int insertCount = 1_000_000;

var tableName = "Transport";
Runner runner = new Runner(1, insertCount, tableName, cts.Token);
runner.OnCompletion = () =>
{
    var messages = runner.GetAll(tableName);
    Console.WriteLine(messages.Count != insertCount
        ? $"Runner 1 failed to insert all records: expected {insertCount} got {messages.Count}"
        : "Runner 1 Successfully inserted all records.");
};

runner.Run();

Utilities.Run(cts);