namespace NeuralSharp.Activation;

public sealed class Tanh : IActivationFunction
{
    public float Activate(float x)
    {
        return (float)Math.Tanh(x);
    }

    public float Derivative(float x)
    {
        return 1 - x * x;
    }
}