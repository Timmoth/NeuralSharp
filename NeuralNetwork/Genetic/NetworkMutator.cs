namespace NeuralSharp.Genetic;

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