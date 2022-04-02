//using System.ComponentModel;
//using System.Diagnostics.CodeAnalysis;
//using System.Drawing;
//using System.Numerics;
//using Aptacode.AppFramework;
//using Aptacode.AppFramework.Components;
//using Aptacode.Geometry.Primitives;
//using NeuralSharp;
//using NeuralSharp.Activation;
//using NeuralSharp.Genetic;
//using Spectre.Console.Cli;

//namespace Demo.Commands;

//internal sealed class
//    Test3Command : Command<Test3Command.Settings>
//{
//    private readonly IActivationFunction _activationFunction;
//    private readonly Evolution _evolution;
//    private readonly INeuralNetworkIo _networkIo;

//    public Test3Command(IActivationFunction activationFunction, INeuralNetworkIo networkIo, Evolution evolution)
//    {
//        _activationFunction = activationFunction;
//        _networkIo = networkIo;
//        _evolution = evolution;
//    }

//    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
//    {
//        Console.WriteLine("#####################################");
//        Console.WriteLine($"Running test 3 with {settings.Values} values for {settings.Iterations} iterations.");
//        Console.WriteLine("#####################################");

//        Run().GetAwaiter().GetResult();

//        return 0;
//    }

//    public async Task Run()
//    {
//        var network = await _networkIo.Load();
//        var evolutionConfig = new EvolutionConfig(1000, 300, 10);
//        await _evolution.Run(network, evolutionConfig, Run);
//    }

//    public float Run(NeuralNetwork network)
//    {
//        var game = CreateGame(network);
//        var behaviour = game.Plugins.Get<SnakeBehaviour>(SnakeBehaviour.BehaviourName);
//        behaviour.EnableTimer = false;
//        while (behaviour.Running)
//        {
//            game.Handle(100);
//        }

//        return behaviour.Result.Fitness;
//    }

//    public Scene CreateGame(NeuralNetwork network)
//    {
//        var scene = new Scene
//        {
//            Size = SnakeGameConfig.BoardSize
//        };

//        var snakeHead =
//            new SnakeBodyComponent(Polygon.Rectangle.FromTwoPoints(SnakeGameConfig.CenterCell + new Vector2(2, 2),
//                SnakeGameConfig.CenterCell + SnakeGameConfig.CellSize - new Vector2(4, 4)));
//        snakeHead.FillColor = Color.LightSlateGray;
//        snakeHead.BorderColor = Color.DarkSlateGray;
//        snakeHead.Direction = Direction.Up;
//        scene.Add(snakeHead);

//        var foodPosition = SnakeGameConfig.RandomCell();
//        var snakeFood =
//            new SnakeFoodComponent(Polygon.Rectangle.FromTwoPoints(foodPosition + new Vector2(2, 2),
//                foodPosition + SnakeGameConfig.CellSize - new Vector2(4, 4)));
//        snakeFood.FillColor = Color.Red;
//        snakeFood.BorderColor = Color.DarkSlateGray;
//        scene.Add(snakeFood);

//        var snakeDirection = new SnakeControlBehaviour(scene);
//        scene.Plugins.Add(snakeDirection);

//        var snakeBehavioru = new SnakeBehaviour(_activationFunction, network, scene)
//        {
//            SnakeHead = snakeHead,
//            SnakeFood = snakeFood
//        };
//        scene.Plugins.Add(snakeBehavioru);

//        var thickness = 10.0f;
//        var bottom = Polygon.Create(thickness, thickness, SnakeGameConfig.BoardSize.X - thickness, thickness,
//            SnakeGameConfig.BoardSize.X, 0, 0, 0).ToComponent();
//        bottom.FillColor = Color.SlateGray;
//        bottom.BorderColor = Color.SlateGray;

//        var right = Polygon.Create(SnakeGameConfig.BoardSize.X - thickness, thickness,
//            SnakeGameConfig.BoardSize.X - thickness, SnakeGameConfig.BoardSize.Y - thickness,
//            SnakeGameConfig.BoardSize.X, SnakeGameConfig.BoardSize.Y, SnakeGameConfig.BoardSize.X, 0).ToComponent();
//        right.FillColor = Color.SlateGray;
//        right.BorderColor = Color.SlateGray;

//        var top = Polygon.Create(SnakeGameConfig.BoardSize.X - thickness, SnakeGameConfig.BoardSize.X - thickness,
//            thickness, SnakeGameConfig.BoardSize.X - thickness, 0, SnakeGameConfig.BoardSize.Y,
//            SnakeGameConfig.BoardSize.X, SnakeGameConfig.BoardSize.Y).ToComponent();
//        top.FillColor = Color.SlateGray;
//        top.BorderColor = Color.SlateGray;

//        var left = Polygon.Create(thickness, SnakeGameConfig.BoardSize.X - thickness, thickness, thickness, 0, 0, 0,
//            SnakeGameConfig.BoardSize.Y).ToComponent();
//        left.FillColor = Color.SlateGray;
//        left.BorderColor = Color.SlateGray;

//        scene.Add(top).Add(right).Add(bottom).Add(left);
//        snakeBehavioru.Walls.Add(top);
//        snakeBehavioru.Walls.Add(right);
//        snakeBehavioru.Walls.Add(bottom);
//        snakeBehavioru.Walls.Add(left);

//        snakeBehavioru.Reset();

//        return scene;
//    }

//    public sealed class Settings : CommandSettings
//    {
//        [Description("Values.")]
//        [CommandArgument(0, "[values]")]
//        public int Values { get; init; }

//        [Description("Iteration.")]
//        [CommandArgument(0, "[iterations]")]
//        public int Iterations { get; init; }
//    }
//}