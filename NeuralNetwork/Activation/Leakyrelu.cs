namespace NeuralNetwork.Activation;

public sealed class Leakyrelu : IActivationFunction
{
    public float Activate(float x)
    {
        return 0 >= x ? 0.01f * x : x;
    }

    public float Derivative(float x)
    {
        return 0 >= x ? 0.01f : 1;
    }
}