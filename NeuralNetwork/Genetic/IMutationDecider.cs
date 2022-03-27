namespace NeuralSharp.Genetic;

public interface IMutationDecider
{
    bool ShouldMutate(float v);
}