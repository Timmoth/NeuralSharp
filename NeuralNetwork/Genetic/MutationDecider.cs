namespace NeuralNetwork.Genetic;

public sealed class MutationDecider : IMutationDecider
{
    private readonly float _mutationRate;

    public MutationDecider(float mutationRate)
    {
        _mutationRate = mutationRate;
    }

    public bool ShouldMutate(float v)
    {
        return Random.Shared.NextDouble() < _mutationRate;
    }
}