namespace NeuralNetwork.Genetic;

public sealed class FloatMutator : IFloatMutator
{
    private readonly float _minMutation;
    private readonly float _mutationRange;

    public FloatMutator(float minMutation, float maxMutation)
    {
        _mutationRange = maxMutation - _minMutation;
        _minMutation = minMutation;
    }

    public float Mutate(float v)
    {
        var mutationAmount = _minMutation + Random.Shared.NextDouble() * _mutationRange;
        return v + (float)mutationAmount;
    }
}