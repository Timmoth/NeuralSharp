namespace NeuralNetwork.Genetic;

public interface IMutationDecider
{
    bool ShouldMutate(float v);
}