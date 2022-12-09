using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using NeuralSharp;
using NeuralSharp.Activation;
using NeuralSharp.Generators;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Demo.Commands;

internal sealed class
    Test1Command : Command<Test1Command.Settings> //Got rid of the code in this because it's no longer correct with how the products are structured, need to rewrite this at some point.
{
    private readonly IActivationFunction _activationFunction;
    private readonly IBiasGenerator _biasGenerator;
    private readonly IWeightGenerator _weightGenerator;

    public Test1Command(IWeightGenerator weightGenerator, IBiasGenerator biasGenerator,
        IActivationFunction activationFunction)
    {
        _weightGenerator = weightGenerator;
        _biasGenerator = biasGenerator;
        _activationFunction = activationFunction;
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        Console.WriteLine("#####################################");
        Console.WriteLine($"Running test 1 with {settings.Values} values for {settings.Iterations} iterations.");
        Console.WriteLine("#####################################");

        var neuralNetwork = new NeuralNetwork(_weightGenerator, _biasGenerator, 3, 16, 1);
        var values = new List<(float[], float[])>();
        var rand = new Random();
        for (var i = 0; i < settings.Values; i++)
        {
            var a = rand.Next(0, 100) / 100.0f;
            var b = rand.Next(0, 100) / 100.0f;
            var c = rand.Next(0, 100) / 100.0f;
            values.Add((new[] { a, b, c }, new[] { Calculate(a, b, c) }));
        }

        for (var i = 0; i < settings.Iterations; i++)
        {
            foreach (var value in values)
            {
                neuralNetwork.BackPropagate(_activationFunction, value.Item1, value.Item2);
            }
        }

        foreach (var value in values)
        {
            var actualOutput = neuralNetwork.FeedForward(_activationFunction, value.Item1);
            Helpers.Output(value.Item2, actualOutput);
        }

        return 0;
    }

    private static float Calculate(float a, float b, float c)
    {
        return new[] { a, b, c }.Max();
    }

    public sealed class Settings : CommandSettings
    {
        [Description("Values.")]
        [CommandArgument(0, "[values]")]
        public int Values { get; init; }

        [Description("Iteration.")]
        [CommandArgument(0, "[iterations]")]
        public int Iterations { get; init; }
    }
}