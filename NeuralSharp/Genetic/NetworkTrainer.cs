using System.Diagnostics;
using Microsoft.Extensions.Logging;
using NeuralSharp.Serialization;

namespace NeuralSharp.Genetic;

public sealed class NetworkTrainer
{
    private readonly ILogger<NetworkTrainer> _logger;
    private readonly INetworkMutator _mutation;
    public NetworkTrainer(ILogger<NetworkTrainer> logger, INetworkMutator mutation)
    {
        _mutation = mutation;
        _logger = logger;
    }

    public async Task<NeuralNetwork> Run(NeuralNetwork networkConfig, NetworkTrainerConfig trainingConfig,
        Func<NeuralNetwork, Task<float>> executor, INeuralNetworkIo neuralNetworkIo)
    {
        var offspring = new NeuralNetwork[trainingConfig.Offspring];
        var tasks = new Task<(NeuralNetwork, float)>[trainingConfig.Offspring];

        // Generate initial generation from given network
        var bestNetwork = offspring[0] = networkConfig;
        for (int i = 1; i < offspring.Length; i++)
        {
            offspring[i] = _mutation.Mutate(new NeuralNetwork(NetworkConfig.From(bestNetwork)));
        }

        _logger.LogInformation("Starting training: {config}", trainingConfig);

        for (var j = 0; j < trainingConfig.Generations; j++)
        {
            // Time iteration
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // Get the average score for each network
            for(var k = 0; k < tasks.Length; k++)
            {
                // capture k
                var index = k;

                tasks[index] = (Task.Run(async () => {
                    var network = offspring[index];
                    var averageScore = await GetAverageNetworkScore(network, executor, trainingConfig.Runs);
                    return (network, averageScore);
                    }
                ));
            }

            // Wait for all tasks to finish executing
            var results = await Task.WhenAll(tasks);

            stopwatch.Stop();

            // The most successful reproduce
            var mostSuccessfulOffspring = results.OrderByDescending(x => x.Item2);
            bestNetwork = offspring[0] = mostSuccessfulOffspring.ElementAt(0).Item1;
            for (int i = 1; i < offspring.Length; i++)
            {
                offspring[i] = _mutation.Mutate(new NeuralNetwork(NetworkConfig.From(bestNetwork)));
            }

            _logger.LogInformation("Completed generation {generation} in {duration}. Average: '{results}', Best: '{best}'", j, stopwatch.Elapsed, string.Join(", ", mostSuccessfulOffspring.Select(r => r.Item2).Average()), mostSuccessfulOffspring.First().Item2);

            //Save file
            await neuralNetworkIo.Save(NetworkConfig.From(bestNetwork));
        }

        _logger.LogInformation("Finished training: {config}", trainingConfig);

        return bestNetwork;
    }

    /// <summary>
    /// Executes the given network task a specified number of times and returns the average score
    /// </summary>
    /// <param name="network"></param>
    /// <param name="networkTask"></param>
    /// <param name="runs"></param>
    /// <returns></returns>
    private async Task<float> GetAverageNetworkScore(
        NeuralNetwork network,
        Func<NeuralNetwork, Task<float>> networkTask,
        int runs)
    {
        float totalScore = 0.0f;
        for (var i = 0; i < runs; i++)
        {
            totalScore += await networkTask(network);
        }

        return totalScore / runs;
    }
}