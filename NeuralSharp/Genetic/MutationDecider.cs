using System.Runtime.CompilerServices;

namespace NeuralSharp.Genetic;

public sealed class MutationDecider : IMutationDecider
{
    private readonly float _mutationRate;
    private static readonly Random _random = new();

    public MutationDecider(float mutationRate)
    {
        _mutationRate = mutationRate;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ShouldMutate(float v)
    {
        return _random.NextDouble() < _mutationRate;
    }
}