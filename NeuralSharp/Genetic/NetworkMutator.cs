using NeuralSharp.Helpers;
using NeuralSharp.Serialization;

namespace NeuralSharp.Genetic;

public static class NetworkCrossover
{
    public static NetworkConfig CrossOver(this NetworkConfig ac, NetworkConfig bc)
    {
        var m = new MutationDecider(0.5f);

        var layers = new List<LayerConfig>();
        for (var i = 0; i < ac.Layers.Count; i++)
        {
            var alayerConfig = ac.Layers[i];
            var blayerConfig = bc.Layers[i];

            var neurons = new List<NeuronData>();
            for (var j = 0; j < alayerConfig.Neurons.Count; j++)
            {
                neurons.Add(m.ShouldMutate(0.5f) ? alayerConfig.Neurons[j] : blayerConfig.Neurons[j]);
            }

            layers.Add(new LayerConfig(neurons));
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

    public static NeuralNetwork CrossOver(this IEnumerable<NeuralNetwork> networks)
    {
        var firstNetwork = networks.First();

        var layers = new Layer[firstNetwork.Layers.Length];
        for (var i = 0; i < layers.Length; i++)
        {
            var layerA = firstNetwork.Layers[i];

            var neurons = new Neuron[layerA.Neurons.Length];

            for (var j = 0; j < neurons.Length; j++)
            {
                //Select the neuron to copy
                var selectedNeuron = networks.SelectRandom().Layers[i].Neurons[j];

                //Create the new neuron
                var neuron = neurons[j] = new Neuron()
                {
                    Bias = selectedNeuron.Bias
                };

                if (i > 0)
                {
                    //Connect the last layer to the current layer
                    var previousLayer = layers[i - 1];
                    var connections = selectedNeuron.In;
                    for (var k = 0; k < connections.Count; k++)
                    {
                        previousLayer.Neurons[k].Connect(neuron, connections[k].Weight);
                    }
                }

            }
            
            //Create the layer from the layer data
            layers[i] = new Layer(neurons);
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
                .Select(neuron => new NeuronData(Mutate(neuron.Bias), neuron.Weights.Select(Mutate).ToList())).ToList())
            .Select(neurons => new LayerConfig(neurons)).ToList();

        return new NetworkConfig(layers);
    }

    public NeuralNetwork MutateFromConfig(NetworkConfig network)
    {
        var layers = new Layer[network.Layers.Count];
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