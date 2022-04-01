namespace NeuralSharp.Genetic;

public sealed class FloatMutator : IFloatMutator
{
    private readonly float _mutationRange;

    public FloatMutator(float mutationRange)
    {
        _mutationRange = mutationRange;
    }

    public float Mutate(float v)
    {
        return v + Gauss();
    }

    public float Gauss()
    {
        var rand = new Random(); //reuse this if you are generating many
        var u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
        var u2 = 1.0 - rand.NextDouble();
        var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                            Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
        return (float)(0 + 1 * randStdNormal); //random normal(mean,stdDev^2)
    }
}