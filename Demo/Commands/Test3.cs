using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;
using System.Text.Json;
using Aptacode.AppFramework.Components;
using Aptacode.AppFramework.Scene;
using Aptacode.Geometry.Primitives;
using NeuralSharp;
using NeuralSharp.Activation;
using NeuralSharp.Generators;
using NeuralSharp.Genetic;
using NeuralSharp.Serialization;
using Snake;
using Snake.Behaviours;
using Snake.Components;
using Snake.States;
using Spectre.Console.Cli;

namespace Demo.Commands;

internal sealed class
    Test3Command : Command<Test3Command.Settings> //Got rid of the code in this because it's no longer correct with how the products are structured, need to rewrite this at some point.
{
    private readonly IActivationFunction _activationFunction;
    private readonly IBiasGenerator _biasGenerator;
    private readonly INetworkMutator _mutation;
    private readonly IWeightGenerator _weightGenerator;

    private readonly int GenerationCount = 5000; //How many generations to run
    private readonly int MutationCount = 400; //How many times should one network be mutated in a single run
    private readonly int OffspringCount = 10; //How many offspring to produce per generation
    private readonly int SuccessfulMutationCount = 10; //How many successful mutations should be selected per generation

    public Test3Command(IWeightGenerator weightGenerator, IBiasGenerator biasGenerator,
        IActivationFunction activationFunction)
    {
        _weightGenerator = weightGenerator;
        _biasGenerator = biasGenerator;
        _activationFunction = activationFunction;
        _mutation = new NetworkMutator(n => 0.1f, new MutationDecider(0.01f), new FloatMutator(0.5f));
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        Console.WriteLine("#####################################");
        Console.WriteLine($"Running test 3 with {settings.Values} values for {settings.Iterations} iterations.");
        Console.WriteLine("#####################################");

        RunAll().GetAwaiter().GetResult();

        return 0;
    }

    public Scene CreateGame(NeuralNetwork network)
    {
        var scene = new Scene(SnakeGameConfig.BoardSize);

        var snakeHead =
            new SnakeBodyComponent(Polygon.Rectangle.FromTwoPoints(SnakeGameConfig.CenterCell + new Vector2(2, 2),
                SnakeGameConfig.CenterCell + SnakeGameConfig.CellSize - new Vector2(4, 4)));
        snakeHead.FillColor = Color.LightSlateGray;
        snakeHead.BorderColor = Color.DarkSlateGray;
        snakeHead.Direction = Direction.Up;
        scene.Add(snakeHead);

        var foodPosition = SnakeGameConfig.RandomCell();
        var snakeFood =
            new SnakeFoodComponent(Polygon.Rectangle.FromTwoPoints(foodPosition + new Vector2(2, 2),
                foodPosition + SnakeGameConfig.CellSize - new Vector2(4, 4)));
        snakeFood.FillColor = Color.Red;
        snakeFood.BorderColor = Color.DarkSlateGray;
        scene.Add(snakeFood);

        var snakeDirection = new SnakeControlBehaviour(scene);
        scene.Plugins.Ui.Add(snakeDirection);

        var snakeBehavioru = new SnakeBehaviour(_activationFunction, network, scene)
        {
            SnakeHead = snakeHead,
            SnakeFood = snakeFood
        };
        scene.Plugins.Tick.Add(snakeBehavioru);

        var thickness = 10.0f;
        var bottom = Polygon.Create(thickness, thickness, SnakeGameConfig.BoardSize.X - thickness, thickness,
            SnakeGameConfig.BoardSize.X, 0, 0, 0).ToComponent();
        bottom.FillColor = Color.SlateGray;
        bottom.BorderColor = Color.SlateGray;

        var right = Polygon.Create(SnakeGameConfig.BoardSize.X - thickness, thickness,
            SnakeGameConfig.BoardSize.X - thickness, SnakeGameConfig.BoardSize.Y - thickness,
            SnakeGameConfig.BoardSize.X, SnakeGameConfig.BoardSize.Y, SnakeGameConfig.BoardSize.X, 0).ToComponent();
        right.FillColor = Color.SlateGray;
        right.BorderColor = Color.SlateGray;

        var top = Polygon.Create(SnakeGameConfig.BoardSize.X - thickness, SnakeGameConfig.BoardSize.X - thickness,
            thickness, SnakeGameConfig.BoardSize.X - thickness, 0, SnakeGameConfig.BoardSize.Y,
            SnakeGameConfig.BoardSize.X, SnakeGameConfig.BoardSize.Y).ToComponent();
        top.FillColor = Color.SlateGray;
        top.BorderColor = Color.SlateGray;

        var left = Polygon.Create(thickness, SnakeGameConfig.BoardSize.X - thickness, thickness, thickness, 0, 0, 0,
            SnakeGameConfig.BoardSize.Y).ToComponent();
        left.FillColor = Color.SlateGray;
        left.BorderColor = Color.SlateGray;

        scene.Add(top).Add(right).Add(bottom).Add(left);
        snakeBehavioru.Walls.Add(top);
        snakeBehavioru.Walls.Add(right);
        snakeBehavioru.Walls.Add(bottom);
        snakeBehavioru.Walls.Add(left);

        snakeBehavioru.Reset();

        return scene;
    }

    public SnakeGameResult Run(NeuralNetwork network)
    {
        var game = CreateGame(network);
        var behaviour = game.Plugins.Tick.Get<SnakeBehaviour>(SnakeBehaviour.BehaviourName);
        behaviour.EnableTimer = false;
        while (behaviour.Running)
        {
            game.Handle(100);
        }

        return behaviour.Result;
    }

    public async Task<List<(NetworkConfig, SnakeGameResult)>> RunMutation(NetworkConfig network)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var networks = new List<NetworkConfig>
        {
            network
        };

        for (var i = 0; i < MutationCount - 1; i++)
        {
            networks.Add(_mutation.Mutate(network));
        }

        var results = new List<(NetworkConfig, SnakeGameResult)>();
        foreach (var networkConfig in networks)
        {
            var totalMovement = 0;
            var totalScore = 0;
            var iterations = 10;
            var selectedNetwork = new NeuralNetwork(networkConfig);
            for (var j = 0; j < iterations; j++)
            {
                var gameResult = Run(selectedNetwork);
                totalMovement += gameResult.Moves;
                totalScore += gameResult.Score;
            }

            results.Add((networkConfig, new SnakeGameResult(totalScore / iterations, totalMovement / iterations)));
        }

        stopwatch.Stop();

        Console.WriteLine($"Ran {MutationCount} mutations in {stopwatch.Elapsed}");
        return results;
    }

    public async Task<(NetworkConfig, SnakeGameResult)> RunAll()
    {
        var networkConfig =
            JsonSerializer.Deserialize<NetworkConfig>(await File.ReadAllTextAsync("./network_output.json"));
        //var networkConfig = NetworkConfig.From(new NeuralNetwork(_weightGenerator, _biasGenerator, 24, 32, 32, 4));
        var bestRun = (networkConfig, new SnakeGameResult(0, 0));

        var results = new List<(NetworkConfig, SnakeGameResult)>
        {
            bestRun
        };

        for (var j = 0; j < GenerationCount; j++)
        {
            var tasks = results.Select(r => Task.Run(() => RunMutation(r.Item1)));
            Console.WriteLine(
                $"Generation {j} starting. Running {tasks.Count() * MutationCount} games over {tasks.Count()} tasks");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            await Task.WhenAll(tasks);

            stopwatch.Stop();

            Console.WriteLine($"Generation {j} complete in {stopwatch.Elapsed}");

            results = tasks
                .SelectMany(m => m.Result)
                .OrderByDescending(r => r.Item2.Fitness)
                .Take(SuccessfulMutationCount).ToList();


            foreach (var result in results)
            {
                Console.WriteLine($"{result.Item2}");
            }

            bestRun = results.First();

            //Create children
            for (var l = 0; l < OffspringCount; l++)
            {
                var child = bestRun.networkConfig.DeepCopy();
                for (var k = 1; k < results.Count; k++)
                {
                    child.CrossOver(results[k].Item1);
                }

                results.Add((child, new SnakeGameResult(0, 0)));
            }

            await File.WriteAllTextAsync("./network_output.json", JsonSerializer.Serialize(bestRun.networkConfig));
        }


        return bestRun;
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