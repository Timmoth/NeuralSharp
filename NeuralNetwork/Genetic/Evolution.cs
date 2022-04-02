using System.Diagnostics;
using Microsoft.Extensions.Logging;
using NeuralSharp.Serialization;

namespace NeuralSharp.Genetic;

public sealed class Evolution
{
    private readonly ILogger<Evolution> _logger;
    private readonly INetworkMutator _mutation;
    private readonly INeuralNetworkIo _neuralNetworkIo;

    public Evolution(ILogger<Evolution> logger, INetworkMutator mutation, INeuralNetworkIo neuralNetworkIo)
    {
        _mutation = mutation;
        _neuralNetworkIo = neuralNetworkIo;
        _logger = logger;
    }

    private Task<List<(NetworkConfig, float)>> RunMutation(NetworkConfig network, EvolutionConfig evolutionConfig,
        Func<NeuralNetwork, float> executor)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var networks = new List<NetworkConfig>
        {
            network
        };

        for (var i = 0; i < evolutionConfig.Mutations - 1; i++)
        {
            networks.Add(_mutation.Mutate(network));
        }

        var results = new List<(NetworkConfig, float)>();
        foreach (var networkConfig in networks)
        {
            var total = 0.0f;
            var iterations = 10;
            var selectedNetwork = new NeuralNetwork(networkConfig);
            for (var j = 0; j < iterations; j++)
            {
                var result = executor(selectedNetwork);
                total += result;
            }

            results.Add((networkConfig, total / iterations));
        }

        stopwatch.Stop();

        _logger.LogInformation("Ran {mutations} mutations in {elapsed}", evolutionConfig.Mutations, stopwatch.Elapsed);
        return Task.FromResult(results);
    }

    public async Task<(NetworkConfig, float)> Run(NetworkConfig networkConfig, EvolutionConfig evolutionConfig,
        Func<NeuralNetwork, float> executor)
    {
        var bestRun = (networkConfig, 0.0f);

        var results = new List<(NetworkConfig, float)>
        {
            bestRun
        };

        for (var j = 0; j < evolutionConfig.Generations; j++)
        {
            var tasks = results.Select(r => Task.Run(() => RunMutation(r.Item1, evolutionConfig, executor)));
            _logger.LogInformation(
                $"Generation {j} starting. Running {tasks.Count() * evolutionConfig.Mutations} games over {tasks.Count()} tasks");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            await Task.WhenAll(tasks);

            stopwatch.Stop();

            _logger.LogInformation($"Generation {j} complete in {stopwatch.Elapsed}");

            results = tasks
                .SelectMany(m => m.Result)
                .OrderByDescending(r => r.Item2)
                .Take(evolutionConfig.Offspring).ToList();


            foreach (var result in results)
            {
                Console.WriteLine($"{result.Item2}");
            }

            bestRun = results.First();

            //Create children
            for (var l = 0; l < evolutionConfig.Offspring; l++)
            {
                var child = bestRun.networkConfig.DeepCopy();
                for (var k = 1; k < results.Count; k++)
                {
                    child.CrossOver(results[k].Item1);
                }

                results.Add((child, 0.0f));
            }

            await _neuralNetworkIo.Save(bestRun.networkConfig);
        }

        return bestRun;
    }
}