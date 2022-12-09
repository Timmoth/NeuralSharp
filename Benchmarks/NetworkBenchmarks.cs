// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Attributes;
using NeuralSharp;
using NeuralSharp.Activation;
using NeuralSharp.Generators;
using NeuralSharp.Serialization;
using Newtonsoft.Json.Linq;

[MemoryDiagnoser]
public class NetworkBenchmarks
{
    private NeuralNetwork network = default!;
    private float[] input;
    private float[] expected;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var networkData = JObject.FromObject(new
        {
            Layers = new[]
           {
                new
                {
                    Neurons = new[]
                    {
                        new
                        {
                            Bias = 1.0f,
                            Weights = new[] { 0.4f, 0.3f, 0.1f }
                        },
                        new
                        {
                            Bias = 1.0f,
                            Weights = new[] { 0.2f, 0.9f, 0.3f }
                        }
                    }
                },
                new
                {
                    Neurons = new[]
                    {
                        new
                        {
                            Bias = 0.7f,
                            Weights = new[] { 0.4f, 0.1f }
                        },
                        new
                        {
                            Bias = 0.6f,
                            Weights = new[] { 0.5f, 0.3f }
                        },
                        new
                        {
                            Bias = 0.3f,
                            Weights = new[] { 0.2f, 0.8f }
                        }
                    }
                },
                new
                {
                    Neurons = new[]
                    {
                        new
                        {
                            Bias = 0.2f,
                            Weights = new float[] { }
                        },
                        new
                        {
                            Bias = 0.8f,
                            Weights = new float[] { }
                        }
                    }
                }
            }
        }).ToObject<NetworkConfig>();

        network = new NeuralNetwork(networkData);
        input = new[] { 0.1f, 0.8f };
        expected = new[] { 0.45f, 0.82f };
    }

    [Benchmark]
    public void CreateNetwork()
    {
        var output = new NeuralNetwork(new WeightGenerator(), new BiasGenerator(), 2, 3, 4);
    }

    [Benchmark]
    public void BackPropagate()
    {
        var output = network.BackPropagate(new Sigmoid(), input, expected);
    }

    [Benchmark]
    public void Feedforward()
    {
        var output = network.FeedForward(new Sigmoid(), input);
    }
}