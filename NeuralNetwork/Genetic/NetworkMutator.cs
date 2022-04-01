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
                if (m.ShouldMutate(0.5f))
                {
                    neurons.Add(alayerConfig.Neurons[j]);
                }
                else
                {
                    neurons.Add(blayerConfig.Neurons[j]);
                }
            }

            layers.Add(new LayerConfig(neurons));
        }

        return new NetworkConfig(layers);
    }
}

public sealed class NetworkMutator : INetworkMutator
{
    #region Construction

    public NetworkMutator(Func<NeuralNetwork, float> fitnessFunction,
        IMutationDecider mutationDecider,
        IFloatMutator floatMutator)
    {
        _fitnessFunction = fitnessFunction;
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

    public NeuralNetwork Mutate(NeuralNetwork[] networks)
    {
        var fittest = networks.OrderByDescending(m => _fitnessFunction(m)).First();
        Mutate(fittest);
        return fittest;
    }

    private float Mutate(float value)
    {
        return !_mutationDecider.ShouldMutate(value) ? value : _floatMutator.Mutate(value);
    }

    #region Properties

    private readonly Func<NeuralNetwork, float> _fitnessFunction;
    private readonly IMutationDecider _mutationDecider;
    private readonly IFloatMutator _floatMutator;

    #endregion
}