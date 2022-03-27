namespace NeuralNetwork.Activation;

public interface IActivationFunction
{
    float Activate(float x);
    float Derivative(float x);
}