using NeuralSharp;
using NeuralSharp.Activation;
using NeuralSharp.Generators;

namespace Demo;

public class Test1
{
    private static float Calculate(float a, float b, float c)
    {
        return new[] { a, b, c }.Max();
    }

    public void Run()
    {
        var activationFunction = new Tanh();

        var neuralNetwork1 =
            new NeuralNetwork(new WeightGenerator(), new BiasGenerator(), 3, 16, 1);
        var values = new List<(float[], float[])>();
        var rand = new Random();
        for (var i = 0; i < 10; i++)
        {
            var a = rand.Next(0, 100) / 100.0f;
            var b = rand.Next(0, 100) / 100.0f;
            var c = rand.Next(0, 100) / 100.0f;
            values.Add((new[] { a, b, c }, new[] { Calculate(a, b, c) }));
        }

        for (var i = 0; i < 1000; i++)
        {
            foreach (var value in values)
            {
                neuralNetwork1.BackPropagate(activationFunction, value.Item1, value.Item2);
            }
        }

        foreach (var value in values)
        {
            var actualOutput = neuralNetwork1.FeedForward(activationFunction, value.Item1);
            Helpers.Output(value.Item2, actualOutput);
        }
    }
}