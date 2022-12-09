namespace NeuralSharp.Generators;

public class BiasGenerator : IBiasGenerator
{
    private static readonly Random _random = new();
    public float Generate()
    {
        return _random.Next(-100, 100) / 100.0f;
    }
}