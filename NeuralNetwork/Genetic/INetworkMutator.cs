namespace NeuralSharp.Genetic;

public interface INetworkMutator
{
    NeuralNetwork Mutate(NeuralNetwork network);
    NeuralNetwork Mutate(NeuralNetwork[] networks);
}