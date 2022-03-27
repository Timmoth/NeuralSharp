using NeuralSharp.Helpers;

namespace NeuralSharp.Serialization;

public sealed record NeuronData(float Bias, List<float> Weights)
{
    public bool Equals(NeuronData other)
    {
        return other != null && Math.Abs(Bias - other.Bias) < Constants.Tolerance &&
               Weights.SequenceEqual(other.Weights);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Bias, Weights);
    }
}