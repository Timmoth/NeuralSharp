namespace NeuralSharp.Generators;

public class BiasGenerator : IBiasGenerator
{
    public float Generate()
    {
        return Random.Shared.Next(-100, 100) / 100.0f;
    }
}