using System.Diagnostics;
using Microsoft.Extensions.Logging;
using NeuralSharp.Serialization;

namespace NeuralSharp.Genetic;

public sealed class Trainer
{
    private readonly ILogger<Trainer> _logger;
    private readonly INetworkMutator _mutation;
    private readonly INeuralNetworkIo _neuralNetworkIo;

    public Trainer(ILogger<Trainer> logger, INetworkMutator mutation, INeuralNetworkIo neuralNetworkIo)
    {
        _mutation = mutation;
        _neuralNetworkIo = neuralNetworkIo;
        _logger = logger;
    }

    public async Task<(NetworkConfig, float)> Run(NetworkConfig networkConfig, TrainingConfig trainingConfig,
        Func<NeuralNetwork, float> executor)
    {
        var bestNetwork = (networkConfig, 0.0f);

        var results = new List<(NetworkConfig, float)>
        {
            bestNetwork
        };

        _logger.LogInformation("Starting training: {config}", trainingConfig);

        for (var j = 0; j < trainingConfig.Generations; j++)
        {
            var tasks = results.Select(r => Task.Run(() => RunNetworkMutations(r.Item1, trainingConfig, executor)));

            _logger.LogInformation(
                "Generation {generation} starting. Running {mutations} mutations over {tasks} parallel tasks.", j,
                tasks.Count() * trainingConfig.Mutations, tasks.Count());

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            await Task.WhenAll(tasks);

            stopwatch.Stop();

            _logger.LogInformation("Generation {generation} complete in {duration}", j, stopwatch.Elapsed);

            results = results.Concat(tasks
                    .SelectMany(m => m.Result))
                .OrderByDescending(r => r.Item2)
                .Take(trainingConfig.Offspring).ToList();

            _logger.LogInformation("Results '{results}'", string.Join(", ", results));

            bestNetwork = results.First();

            //Create children
            var networks = results.Select(c => c.Item1).CrossOver(trainingConfig.Offspring);
            results.AddRange(networks.Select(n => (n, 0.0f)));

            await _neuralNetworkIo.Save(bestNetwork.networkConfig);
        }

        return bestNetwork;
    }

    private async Task<List<(NetworkConfig, float)>> RunNetworkMutations(NetworkConfig network,
        TrainingConfig trainingConfig,
        Func<NeuralNetwork, float> executor)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var results = new List<(NetworkConfig, float)>();
        for (var i = 0; i < trainingConfig.Mutations; i++)
        {
            var mutatedNetwork = _mutation.Mutate(network);
            results.Add((mutatedNetwork, executor(new NeuralNetwork(mutatedNetwork))));
        }

        stopwatch.Stop();

        _logger.LogInformation("Ran {mutations} mutations in {elapsed}", trainingConfig.Mutations, stopwatch.Elapsed);

        return results;
    }
}