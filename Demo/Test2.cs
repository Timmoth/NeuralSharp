using NeuralNetwork.Activation;
using NeuralNetwork.Generators;

namespace Demo;

public class Test2
{
    public void Run()
    {
        var activationFunction = new Tanh();
        var neuralNetwork1 =
            new NeuralNetwork.NeuralNetwork(new WeightGenerator(),
                new BiasGenerator(),
                1, 16, 1);

        var values = new List<(float[], float[])>();
        var rand = new Random();
        for (var i = 0; i < 100; i++)
        {
            var a = rand.Next(0, 100) / 100.0f;
            values.Add((new[] { a }, new[] { (float)Math.Sin(a) }));
        }

        for (var i = 0; i < 10; i++)
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