namespace NeuralSharp.Activation;

public interface IActivationFunction
{
    float Activate(float x);
    float Derivative(float x);
}