using System.Drawing;
using System.Net.Http;
using System.Net.Http.Json;
using System.Numerics;
using System.Threading.Tasks;
using Aptacode.AppFramework;
using Aptacode.AppFramework.Components;
using Aptacode.Geometry.Primitives;
using Microsoft.AspNetCore.Components;
using NeuralSharp;
using NeuralSharp.Activation;
using NeuralSharp.Generators;
using NeuralSharp.Serialization;
using Snake;
using Snake.Behaviours;
using Snake.Components;
using Snake.States;

namespace BlazorDemo.Pages;

public class SnakeBase : ComponentBase
{
    [Inject] public HttpClient Client { get; set; }
    [Inject] public IActivationFunction ActivationFunction { get; set; }
    [Inject] public IBiasGenerator BiasGenerator { get; set; }
    [Inject] public IWeightGenerator WeightGenerator { get; set; }

    public Scene Scene { get; set; }
    public SnakeBehaviour SnakeGame { get; set; }

    public void CreateGame(NeuralNetwork network)
    {
        var snakeHead =
            new SnakeBodyComponent(Polygon.Rectangle.FromTwoPoints(SnakeGameConfig.CenterCell + new Vector2(2, 2),
                SnakeGameConfig.CenterCell + SnakeGameConfig.CellSize - new Vector2(4, 4)));
        snakeHead.FillColor = Color.LightSlateGray;
        snakeHead.BorderColor = Color.DarkSlateGray;
        snakeHead.Direction = Direction.Up;
        Scene.Add(snakeHead);

        var foodPosition = SnakeGameConfig.RandomCell();
        var snakeFood =
            new SnakeFoodComponent(Polygon.Rectangle.FromTwoPoints(foodPosition + new Vector2(2, 2),
                foodPosition + SnakeGameConfig.CellSize - new Vector2(4, 4)));
        snakeFood.FillColor = Color.Red;
        snakeFood.BorderColor = Color.DarkSlateGray;
        Scene.Add(snakeFood);

        var snakeDirection = new SnakeControlBehaviour(Scene);
        Scene.Plugins.Add(snakeDirection);

        SnakeGame = new SnakeBehaviour(ActivationFunction, network, Scene)
        {
            SnakeHead = snakeHead,
            SnakeFood = snakeFood
        };
        SnakeGame.GameOver += GameOver;
        Scene.Plugins.Add(SnakeGame);

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

        Scene.Add(top).Add(right).Add(bottom).Add(left);
        SnakeGame.Walls.Add(top);
        SnakeGame.Walls.Add(right);
        SnakeGame.Walls.Add(bottom);
        SnakeGame.Walls.Add(left);

        SnakeGame.Reset();
    }

    private void GameOver(object? sender, SnakeGameResult e)
    {
        SnakeGame.Reset();
    }

    protected override async Task OnInitializedAsync()
    {
        Scene = new Scene()
        {
            Size = SnakeGameConfig.BoardSize
        };

        var networkConfig = await Client.GetFromJsonAsync<NetworkConfig>("network.json");

        var network = new NeuralNetwork(networkConfig);

        CreateGame(network);

        await base.OnInitializedAsync();
    }
}