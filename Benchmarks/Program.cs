using BenchmarkDotNet.Running;

Console.Write("Select benchmarks to run: ");
if (!int.TryParse(Console.ReadLine(), out var selection))
{
    Console.Write("Could not parse given input.");
}

switch (selection)
{
    case 1:
        BenchmarkRunner.Run<NetworkBenchmarks>();
        break;
    case -1:
        BenchmarkRunner.Run<NetworkBenchmarks>();
        break;
}

Console.ReadLine();