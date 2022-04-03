using NeuralSharp.Serialization;

namespace NeuralSharp.Genetic;

public interface INetworkMutator
{
    NeuralNetwork Mutate(NeuralNetwork network);
    NetworkConfig Mutate(NetworkConfig network);
    NeuralNetwork MutateFromConfig(NetworkConfig network);
}