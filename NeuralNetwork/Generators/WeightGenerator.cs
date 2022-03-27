namespace NeuralSharp.Generators;

public class WeightGenerator : IWeightGenerator
{
    public float Generate()
    {
        return Random.Shared.Next(-100, 100) / 100.0f;
    }
}