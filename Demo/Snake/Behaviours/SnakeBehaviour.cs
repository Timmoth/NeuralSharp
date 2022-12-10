using System.Drawing;
using System.Numerics;
using Aptacode.AppFramework;
using Aptacode.AppFramework.Components;
using Aptacode.AppFramework.Plugins;
using Aptacode.Geometry.Primitives;
using Snake.Components;

namespace Snake.Behaviours;

public sealed class SnakeBehaviour : Plugin
{
    public SnakeBehaviour(Scene scene) :
        base(scene)
    {
    }

    public bool EnableTimer { get; set; } = true;

    public override bool Handle(float deltaT)
    {
        //Only handle next frame if entire tick has transpired
        if (EnableTimer && DateTimeOffset.Now - _lastTick < TickSpeed)
        {
            return false;
        }

        _lastTick = DateTimeOffset.Now;

        //Check if game has timed out
        if (HasGameEnded())
        {
            EndGame();
            return true;
        }

        LifeLeft--;
        Moves++;

        MoveSnake();

        if (SnakeHead.CollidesWith(SnakeFood))
        {
            EatFood();
        }

        return true;
    }

    #region Movement

    private void MoveSnake()
    {
        SnakeHead.Translate(SnakeGameConfig.GetMovement(SnakeHead.Direction));
        var lastDirection = SnakeHead.Direction;
        for (var i = 0; i < SnakeBody.Count; i++)
        {
            var bodyComponent = SnakeBody[i];
            bodyComponent.Translate(SnakeGameConfig.GetMovement(bodyComponent.Direction));

            (lastDirection, bodyComponent.Direction) = (bodyComponent.Direction, lastDirection);
        }
    }

    #endregion


    #region Constants

    public static string BehaviourName = "SnakeMovement";
    private static readonly TimeSpan InitialTickSpeed = TimeSpan.FromMilliseconds(40);

    public override string Name()
    {
        return BehaviourName;
    }

    #endregion

    #region Properties

    public int LifeLeft { get; set; }
    public int Moves { get; set; }
    public bool Running { get; set; } = true;
    public TimeSpan TickSpeed { get; set; } = InitialTickSpeed;
    private DateTimeOffset _lastTick = DateTimeOffset.Now;
    public int Score => SnakeBody.Count;

    #endregion

    #region Scene

    public SnakeBodyComponent SnakeHead { get; set; }
    public SnakeFoodComponent SnakeFood { get; set; }
    public List<SnakeBodyComponent> SnakeBody { get; set; } = new();
    public List<Component> Walls { get; set; } = new();

    #endregion

    #region Events

    public EventHandler<SnakeGameResult> GameOver { get; set; }
    public EventHandler<int> ScoreChanged { get; set; }

    #endregion

    #region GameLogic

    private bool HasGameEnded()
    {
        return HasTimedOut() ||
               SnakeBody.Any(b => b.CollidesWith(SnakeHead)) ||
               Walls.Any(b => b.CollidesWith(SnakeHead));
    }

    public SnakeGameResult GetGameResult()
    {
        return new SnakeGameResult(Score, Moves);
    }

    public SnakeGameResult Result { get; set; }

    private void EndGame()
    {
        Result = GetGameResult();
        Running = false;
        GameOver?.Invoke(this, GetGameResult());
    }

    public void Reset()
    {
        LifeLeft = SnakeGameConfig.LifeTime;
        Moves = 0;
        TickSpeed = InitialTickSpeed;
        SnakeHead.SetPosition(SnakeGameConfig.RandomCell(), true);

        foreach (var snakeBodyComponent in SnakeBody)
        {
            Scene.Remove(snakeBodyComponent);
        }

        SnakeBody.Clear();
        ScoreChanged?.Invoke(this, SnakeBody.Count);

        SnakeFood.SetPosition(SnakeGameConfig.RandomCell(), true);
        while (SnakeHead.CollidesWith(SnakeFood))
        {
            SnakeFood.SetPosition(SnakeGameConfig.RandomCell(), true);
        }
    }

    public bool HasTimedOut()
    {
        return LifeLeft == 0;
    }

    private void EatFood()
    {
        ScoreChanged?.Invoke(this, SnakeBody.Count);

        Grow();
        MoveFood();

        LifeLeft += SnakeGameConfig.LifeTime;

        //Increase tick speed
        TickSpeed = TimeSpan.FromMilliseconds(Math.Clamp(InitialTickSpeed.Milliseconds - 20 * SnakeBody.Count, 10,
            InitialTickSpeed.Milliseconds));
    }

    private void Grow()
    {
        var lastSnakeComponent = SnakeBody.Count > 0 ? SnakeBody.Last() : SnakeHead;

        var snakeBodyComponent =
            new SnakeBodyComponent(Polygon.Create(lastSnakeComponent.Polygon.Vertices.Vertices.ToArray()));
        snakeBodyComponent.FillColor = Color.LightSlateGray;
        snakeBodyComponent.BorderColor = Color.DarkSlateGray;
        snakeBodyComponent.Translate(
            SnakeGameConfig.GetMovement(SnakeGameConfig.Reverse(snakeBodyComponent.Direction)));

        SnakeBody.Add(snakeBodyComponent);
        Scene.Add(snakeBodyComponent);
    }

    private void MoveFood()
    {
        var foodPosition = SnakeGameConfig.RandomCell() + new Vector2(2, 2);
        SnakeFood.Primitive.SetPosition(foodPosition);
        SnakeFood.SetPosition(SnakeGameConfig.RandomCell(), true);
        while (SnakeHead.CollidesWith(SnakeFood) || SnakeBody.Any(b => b.CollidesWith(SnakeFood)))
        {
            foodPosition = SnakeGameConfig.RandomCell() + new Vector2(2, 2);
            SnakeFood.Primitive.SetPosition(foodPosition);
        }
    }

    #endregion
}