namespace NeuralNetwork.Activation;

public sealed class Relu : IActivationFunction
{
    public float Activate(float x)
    {
        return 0 >= x ? 0 : x;
    }

    public float Derivative(float x)
    {
        return 0 >= x ? 0 : 1;
    }
}