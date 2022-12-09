using NeuralSharp.Helpers;
using NeuralSharp.Serialization;
using System.Linq;

namespace NeuralSharp.Genetic;

public static class NetworkCrossover
{
    public static NetworkConfig CrossOver(this NetworkConfig ac, NetworkConfig bc)
    {
        var m = new MutationDecider(0.5f);

        var layers = new LayerConfig[ac.Layers.Length];
        for (var i = 0; i < ac.Layers.Length; i++)
        {
            var alayerConfig = ac.Layers[i];
            var blayerConfig = bc.Layers[i];

            var neurons = new NeuronData[alayerConfig.Neurons.Length];
            for (var j = 0; j < alayerConfig.Neurons.Length; j++)
            {
                neurons[j] = m.ShouldMutate(0.5f) ? alayerConfig.Neurons[j] : blayerConfig.Neurons[j];
            }

            layers[i] = new LayerConfig(neurons);
        }

        return new NetworkConfig(layers);
    }

    public static NetworkConfig CrossOver(this IEnumerable<NetworkConfig> networks)
    {
        var child = networks.First();
        return networks.Skip(1).Aggregate(child, (current, networkConfig) => current.CrossOver(networkConfig));
    }

    public static IEnumerable<NetworkConfig> CrossOver(this IEnumerable<NetworkConfig> results, int childrenCount)
    {
        var children = new List<NetworkConfig>();
        for (var l = 0; l < childrenCount; l++)
        {
            children.Add(results.CrossOver());
        }

        return children;
    }

    private static NeuralNetwork SelectRandom(this IEnumerable<NeuralNetwork> n)
    {
        return n.ElementAt(Random.Shared.Next(0, n.Count()));
    }
    private static float SelectBias(this IEnumerable<NeuralNetwork> n, int i, int j)
    {
        return SelectRandom(n).Layers[i].Neurons[j].Bias;
    }
    private static float SelectWeight(this IEnumerable<NeuralNetwork> n, int i, int j, int k)
    {
        return SelectRandom(n).Layers[i].Neurons[j].Out[k].Weight;
    }

    public static NeuralNetwork CrossOver(this IEnumerable<NeuralNetwork> networks)
    {
        var firstNetwork = networks.First();

        var layers = new Layer[firstNetwork.Layers.Length];
        for (var i = 0; i < layers.Length; i++)
        {
            var neurons = new Neuron[firstNetwork.Layers[i].Neurons.Length];

            for (var j = 0; j < neurons.Length; j++)
            {
                //Create the new neuron
                neurons[j] = new Neuron()
                {
                    Bias = networks.SelectBias(i, j)
                };
            }


            //Create the layer from the layer data
            layers[i] = new Layer(neurons);

            if (i > 0)
            {
                var layer = layers[i];
                //Connect the last layer to the current layer
                var previousLayer = layers[i - 1];

                for (var k = 0; k < previousLayer.Neurons.Length; k++)
                {
                    var n = previousLayer.Neurons[k];
                    for (var l = 0; l < layer.Neurons.Length; l++)
                    {
                       n.Connect(layer.Neurons[l], networks.SelectWeight(i - 1, k, l));
                    }
                }
            }
        }

        return new NeuralNetwork(layers);
    }

    public static IEnumerable<NeuralNetwork> CrossOver(this IEnumerable<NeuralNetwork> results, int childrenCount)
    {
        var children = new List<NeuralNetwork>();
        for (var l = 0; l < childrenCount; l++)
        {
            children.Add(results.CrossOver());
        }

        return children;
    }
}

public sealed class NetworkMutator : INetworkMutator
{
    #region Construction

    public NetworkMutator(
        IMutationDecider mutationDecider,
        IFloatMutator floatMutator)
    {
        _mutationDecider = mutationDecider;
        _floatMutator = floatMutator;
    }

    #endregion

    public NeuralNetwork Mutate(NeuralNetwork network)
    {
        foreach (var layer in network.Layers)
        {
            foreach (var neuron in layer.Neurons)
            {
                neuron.Bias = Mutate(neuron.Bias);

                foreach (var connection in neuron.Out)
                {
                    connection.Weight = Mutate(connection.Weight);
                }
            }
        }

        return network;
    }

    public NetworkConfig Mutate(NetworkConfig network)
    {
        var layers = network.Layers
            .Select(layer => layer.Neurons
                .Select(neuron => new NeuronData(Mutate(neuron.Bias), neuron.Weights.Select(Mutate).ToArray())).ToArray())
            .Select(neurons => new LayerConfig(neurons)).ToArray();

        return new NetworkConfig(layers);
    }

    public NeuralNetwork MutateFromConfig(NetworkConfig network)
    {
        var layers = new Layer[network.Layers.Length];
        for (var i = 0; i < layers.Length; i++)
        {
            var layerData = network.Layers[i];

            //Create the layer from the layer data
            layers[i] = Layer.Create(layerData);
            if (i > 0)
            {
                //Connect the last layer to the current layer
                var previousLayer = layers[i - 1];
                previousLayer.Connect(layers[i], network.Layers[i - 1]);
            }
        }

        return new NeuralNetwork(layers);
    }

    private float Mutate(float value)
    {
        return !_mutationDecider.ShouldMutate(value) ? value : _floatMutator.Mutate(value);
    }

    #region Properties

    private readonly IMutationDecider _mutationDecider;
    private readonly IFloatMutator _floatMutator;

    #endregion
}