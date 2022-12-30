using Aptacode.AppFramework;
using Aptacode.AppFramework.Components;
using Aptacode.Geometry.Primitives;
using Demo.Snake;
using NeuralSharp;
using NeuralSharp.Activation;
using Snake.Behaviours;
using Snake.Components;

namespace Snake;

public class SnakeScene : Scene
{
    public SnakeScene(IActivationFunction activationFunction, NeuralNetwork network)
    {
        ActivationFunction = activationFunction;
        Network = network;
    }

    public SnakeBehaviour SnakeGame { get; set; }
    public IActivationFunction ActivationFunction { get; set; }
    public NeuralNetwork Network { get; set; }

    public override Task Setup()
    {
        Plugins.Add(new SnakeAIControl(this, ActivationFunction, Network));
        SnakeGame = new SnakeBehaviour(this);
        Plugins.Add(SnakeGame);

        SnakeGame.SnakeHead =
           new SnakeBodyComponent(Polygon.Rectangle.FromTwoPoints(-SnakeGameConfig.InnerCellSize / 2, SnakeGameConfig.InnerCellSize / 2))
           {
               Direction = Direction.Up
           };
        Add(SnakeGame.SnakeHead);

        SnakeGame.SnakeFood =
            new SnakeFoodComponent(Polygon.Rectangle.FromTwoPoints(-SnakeGameConfig.InnerCellSize / 2, SnakeGameConfig.InnerCellSize / 2));
        SnakeGame.SnakeFood.SetTranslation(SnakeGameConfig.RandomCell());
        Add(SnakeGame.SnakeFood);

        var thickness = 10.0f;
        var bottom = new Polygon(thickness, thickness, SnakeGameConfig.BoardSize.X - thickness, thickness,
            SnakeGameConfig.BoardSize.X, 0, 0, 0).ToComponent();

        var right = new Polygon(SnakeGameConfig.BoardSize.X - thickness, thickness,
            SnakeGameConfig.BoardSize.X - thickness, SnakeGameConfig.BoardSize.Y - thickness,
            SnakeGameConfig.BoardSize.X, SnakeGameConfig.BoardSize.Y, SnakeGameConfig.BoardSize.X, 0).ToComponent();

        var top = new Polygon(SnakeGameConfig.BoardSize.X - thickness, SnakeGameConfig.BoardSize.X - thickness,
            thickness, SnakeGameConfig.BoardSize.X - thickness, 0, SnakeGameConfig.BoardSize.Y,
            SnakeGameConfig.BoardSize.X, SnakeGameConfig.BoardSize.Y).ToComponent();

        var left = new Polygon(thickness, SnakeGameConfig.BoardSize.X - thickness, thickness, thickness, 0, 0, 0,
            SnakeGameConfig.BoardSize.Y).ToComponent();

        Add(top).Add(right).Add(bottom).Add(left);
        SnakeGame.Walls.Add(top);
        SnakeGame.Walls.Add(right);
        SnakeGame.Walls.Add(bottom);
        SnakeGame.Walls.Add(left);

        return Task.CompletedTask;
    }
}
