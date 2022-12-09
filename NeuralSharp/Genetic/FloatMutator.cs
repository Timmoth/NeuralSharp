namespace NeuralSharp.Genetic;

public sealed class FloatMutator : IFloatMutator
{
    private static readonly Random _random = new();
    public float Mutate(float v)
    {
        return v + Gauss();
    }

    public float Gauss()
    {
        var u1 = 1.0 - _random.NextDouble(); //uniform(0,1] random doubles
        var u2 = 1.0 - _random.NextDouble();
        var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                            Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
        return (float)(0 + 1 * randStdNormal); //random normal(mean,stdDev^2)
    }
}