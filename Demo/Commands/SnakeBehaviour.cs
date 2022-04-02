using System.Drawing;
using System.Numerics;
using Aptacode.AppFramework.Components;
using Aptacode.AppFramework.Plugins.Behaviours;
using Aptacode.AppFramework.Scene;
using Aptacode.Geometry.Primitives;
using NeuralSharp;
using NeuralSharp.Activation;
using Snake.Components;
using Snake.States;

namespace Snake.Behaviours;

public sealed class SnakeBehaviour : BehaviourPlugin<float>
{
    public SnakeBehaviour(IActivationFunction activationFunction, NeuralNetwork network, Scene scene) :
        base(scene)
    {
        _activationFunction = activationFunction;
        _neuralNetwork = network;
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

        var vision = GetVision();
        var output = _neuralNetwork.FeedForward(_activationFunction, vision);

        var newDirection = GetNewDirection(output);
        MoveSnake(newDirection);

        if (SnakeHead.CollidesWith(SnakeFood))
        {
            EatFood();
        }

        return true;
    }

    #region Dependencies

    private readonly IActivationFunction _activationFunction;

    private readonly NeuralNetwork _neuralNetwork;

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

    #region Vision

    private bool BodyCollide(Vector2 v)
    {
        return SnakeBody.Any(b => b.CollidesWith(v));
    }

    private bool WallCollide(Vector2 v)
    {
        return v.X - SnakeGameConfig.CellSize.X / 2.0f <= 0 ||
               v.Y - SnakeGameConfig.CellSize.Y / 2.0f <= 0 ||
               v.X + SnakeGameConfig.CellSize.X / 2.0f >= SnakeGameConfig.BoardSize.X ||
               v.Y + SnakeGameConfig.CellSize.Y / 2.0f >= SnakeGameConfig.BoardSize.Y;
    }

    private bool FoodCollide(Vector2 v)
    {
        return SnakeFood.CollidesWith(v);
    }

    private float[] Look(Vector2 direction)
    {
        var look = new float[3];
        var pos = SnakeHead.Primitive.BoundingRectangle.Center;
        var distance = 0.0f;
        var foodFound = false;
        var bodyFound = false;
        pos += direction;
        distance += 1;
        while (!WallCollide(pos))
        {
            if (!foodFound && FoodCollide(pos))
            {
                foodFound = true;
                look[0] = 1;
            }

            if (!bodyFound && BodyCollide(pos))
            {
                bodyFound = true;
                look[1] = 1;
            }

            pos += direction;
            distance += 1;
        }

        look[2] = 1 / distance;
        return look;
    }

    private float[] GetVision()
    {
        var vision = new float[24];
        var temp = Look(new Vector2(-SnakeGameConfig.CellSize.X, 0));
        vision[0] = temp[0];
        vision[1] = temp[1];
        vision[2] = temp[2];
        temp = Look(new Vector2(-SnakeGameConfig.CellSize.X, -SnakeGameConfig.CellSize.Y));
        vision[3] = temp[0];
        vision[4] = temp[1];
        vision[5] = temp[2];
        temp = Look(new Vector2(0, -SnakeGameConfig.CellSize.Y));
        vision[6] = temp[0];
        vision[7] = temp[1];
        vision[8] = temp[2];
        temp = Look(new Vector2(SnakeGameConfig.CellSize.X, -SnakeGameConfig.CellSize.Y));
        vision[9] = temp[0];
        vision[10] = temp[1];
        vision[11] = temp[2];
        temp = Look(new Vector2(SnakeGameConfig.CellSize.X, 0));
        vision[12] = temp[0];
        vision[13] = temp[1];
        vision[14] = temp[2];
        temp = Look(new Vector2(SnakeGameConfig.CellSize.X, SnakeGameConfig.CellSize.Y));
        vision[15] = temp[0];
        vision[16] = temp[1];
        vision[17] = temp[2];
        temp = Look(new Vector2(0, SnakeGameConfig.CellSize.Y));
        vision[18] = temp[0];
        vision[19] = temp[1];
        vision[20] = temp[2];
        temp = Look(new Vector2(-SnakeGameConfig.CellSize.X, SnakeGameConfig.CellSize.Y));
        vision[21] = temp[0];
        vision[22] = temp[1];
        vision[23] = temp[2];
        return vision;
    }

    #endregion

    #region Movement

    private Direction GetNewDirection(float[] output)
    {
        var maxOutput = 0.0f;
        var direction = SnakeHead.Direction;
        for (var i = 0; i < output.Length; i++)
        {
            var outputValue = output[i];
            if (outputValue > maxOutput)
            {
                maxOutput = outputValue;
                direction = (Direction)i;
            }
        }

        return direction;
    }

    private void MoveSnake(Direction direction)
    {
        SnakeHead.Direction = direction;
        SnakeHead.Translate(SnakeGameConfig.GetMovement(direction));
        var lastDirection = direction;
        for (var i = 0; i < SnakeBody.Count; i++)
        {
            var bodyComponent = SnakeBody[i];
            bodyComponent.Translate(SnakeGameConfig.GetMovement(bodyComponent.Direction));

            (lastDirection, bodyComponent.Direction) = (bodyComponent.Direction, lastDirection);
        }
    }

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