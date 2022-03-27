namespace NeuralSharp.Activation;

public sealed class Sigmoid : IActivationFunction
{
    public float Activate(float x)
    {
        var k = (float)Math.Exp(x);
        return k / (1.0f + k);
    }

    public float Derivative(float x)
    {
        return x * (1 - x);
    }
}