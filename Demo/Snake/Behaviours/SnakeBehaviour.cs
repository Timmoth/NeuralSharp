using System.Numerics;
using Aptacode.AppFramework;
using Aptacode.AppFramework.Components.Primitives;
using Aptacode.AppFramework.Plugins;
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
        SnakeHead.AddTranslation(SnakeGameConfig.GetMovement(SnakeHead.Direction));
        var lastDirection = SnakeHead.Direction;
        for (var i = 0; i < SnakeBody.Count; i++)
        {
            var bodyComponent = SnakeBody[i];
            bodyComponent.AddTranslation(SnakeGameConfig.GetMovement(bodyComponent.Direction));

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

    public int LifeLeft { get; set; } = SnakeGameConfig.LifeTime;
    public int Moves { get; set; } = 0;
    public bool Running { get; set; } = true;
    public TimeSpan TickSpeed { get; set; } = InitialTickSpeed;
    private DateTimeOffset _lastTick = DateTimeOffset.Now;
    public int Score => SnakeBody.Count;

    #endregion

    #region Scene

    public SnakeBodyComponent SnakeHead { get; set; }
    public SnakeFoodComponent SnakeFood { get; set; }
    public List<SnakeBodyComponent> SnakeBody { get; set; } = new();
    public List<PrimitiveComponent> Walls { get; set; } = new();

    #endregion

    #region Events

    public EventHandler<float> GameOver { get; set; }
    public EventHandler<int> ScoreChanged { get; set; }

    #endregion

    #region GameLogic

    private bool HasGameEnded()
    {
        return HasTimedOut() ||
               SnakeBody.Any(b => b.CollidesWith(SnakeHead)) ||
               Walls.Any(b => b.CollidesWith(SnakeHead));
    }

    public float Fitness => (Score + 1) * (Score + 1) + (float)Moves / SnakeGameConfig.LifeTime;

    private void EndGame()
    {
        Running = false;
        GameOver?.Invoke(this, Fitness);
    }

    public void Reset()
    {
        LifeLeft = SnakeGameConfig.LifeTime;
        Moves = 0;
        TickSpeed = InitialTickSpeed;
        SnakeHead.SetTranslation(SnakeGameConfig.RandomCell());

        foreach (var snakeBodyComponent in SnakeBody)
        {
            Scene.Remove(snakeBodyComponent);
        }

        SnakeBody.Clear();
        ScoreChanged?.Invoke(this, SnakeBody.Count);

        SnakeFood.SetTranslation(SnakeGameConfig.RandomCell());
        while (SnakeHead.CollidesWith(SnakeFood))
        {
            SnakeFood.SetTranslation(SnakeGameConfig.RandomCell());
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
            new SnakeBodyComponent(lastSnakeComponent.Polygon.Copy());

        var cell = SnakeGameConfig.GetMovement(SnakeGameConfig.Reverse(snakeBodyComponent.Direction));
        snakeBodyComponent.TranslationMatrix = lastSnakeComponent.TranslationMatrix * Matrix3x2.CreateTranslation(cell);

        SnakeBody.Add(snakeBodyComponent);
        Scene.Add(snakeBodyComponent);
    }

    private void MoveFood()
    {
        SnakeFood.SetTranslation(SnakeGameConfig.RandomCell());
        while (SnakeHead.CollidesWith(SnakeFood) || SnakeBody.Any(b => b.CollidesWith(SnakeFood)))
        {
            SnakeFood.SetTranslation(SnakeGameConfig.RandomCell());
        }
    }

    #endregion
}