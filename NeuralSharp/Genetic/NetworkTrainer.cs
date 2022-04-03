using System.Diagnostics;
using Microsoft.Extensions.Logging;
using NeuralSharp.Serialization;

namespace NeuralSharp.Genetic;

public sealed class NetworkTrainer
{
    private readonly ILogger<NetworkTrainer> _logger;
    private readonly INetworkMutator _mutation;
    private readonly INeuralNetworkIo _neuralNetworkIo;

    public NetworkTrainer(ILogger<NetworkTrainer> logger, INetworkMutator mutation, INeuralNetworkIo neuralNetworkIo)
    {
        _mutation = mutation;
        _neuralNetworkIo = neuralNetworkIo;
        _logger = logger;
    }

    public async Task<(NeuralNetwork, float)> Run(NeuralNetwork networkConfig, NetworkTrainerConfig trainingConfig,
        Func<NeuralNetwork, float> executor)
    {
        var bestNetwork = (networkConfig, 0.0f);

        var results = new List<(NeuralNetwork, float)>
        {
            bestNetwork
        };

        _logger.LogInformation("Starting training: {config}", trainingConfig);

        for (var j = 0; j < trainingConfig.Generations; j++)
        {
            var tasks = results.Select((r, i) => Task.Run(() => RunNetworkMutations(j, i, r.Item1, trainingConfig, executor)));

            _logger.LogInformation(
                "Starting generation {generation}. Running {mutations} mutations over {tasks} parallel tasks.", j,
                tasks.Count() * trainingConfig.Mutations, tasks.Count());

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            await Task.WhenAll(tasks);

            stopwatch.Stop();

            results = tasks
                    .SelectMany(m => m.Result)
                .OrderByDescending(r => r.Item2)
                .Take(trainingConfig.Offspring).ToList();

            _logger.LogInformation("Completed generation {generation} in {duration}. Results '{results}'", j, stopwatch.Elapsed, string.Join(", ", results.Select(r => r.Item2)));

            bestNetwork = results.First();

            //Create children
            var networks = results.Select(c => c.Item1).CrossOver(trainingConfig.Offspring);
            results.AddRange(networks.Select(n => (n, 0.0f)));

            await _neuralNetworkIo.Save(NetworkConfig.From(bestNetwork.networkConfig));
        }

        _logger.LogInformation("Finished training: {config}", trainingConfig);

        return bestNetwork;
    }

    private async Task<List<(NeuralNetwork, float)>> RunNetworkMutations(int generationIndex, int networkIndex,
        NeuralNetwork network,
        NetworkTrainerConfig trainingConfig,
        Func<NeuralNetwork, float> executor)
    {
        _logger.LogTrace("{generationIndex},{mutationIndex} - Started ", generationIndex, networkIndex);

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var results = new List<(NeuralNetwork, float)>();
        var bestResult = 0.0f;
        for (var i = 0; i < trainingConfig.Mutations; i++)
        {
            var mutatedNetwork = _mutation.Mutate(network);
            var result = executor(mutatedNetwork);
            results.Add((mutatedNetwork, result));
            bestResult = result > bestResult ? result : bestResult;
        }

        stopwatch.Stop();

        _logger.LogTrace("{generationIndex},{mutationIndex} - Completed in {elapsed}, best result: '{bestResult}'", generationIndex, networkIndex, stopwatch.Elapsed, bestResult);

        return results;
    }
}