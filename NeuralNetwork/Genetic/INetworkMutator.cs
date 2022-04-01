using NeuralSharp.Serialization;

namespace NeuralSharp.Genetic;

public interface INetworkMutator
{
    NeuralNetwork Mutate(NeuralNetwork network);
    NeuralNetwork Mutate(NeuralNetwork[] networks);
    NetworkConfig Mutate(NetworkConfig network);
}