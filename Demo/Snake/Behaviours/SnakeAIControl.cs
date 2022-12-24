using System.Numerics;
using System.Runtime.CompilerServices;
using Aptacode.AppFramework;
using Aptacode.AppFramework.Plugins;
using NeuralSharp;
using NeuralSharp.Activation;
using Snake.States;

namespace Snake.Behaviours;

public class SnakeAIControl : Plugin
{
    public static string BehaviourName = "SnakeAIControl";

    private SnakeBehaviour _snakeBehaviour;
    private readonly float[] vision = new float[24];

    public SnakeAIControl(Scene scene, IActivationFunction activationFunction, NeuralNetwork neuralNetwork) :
        base(scene)
    {
        _activationFunction = activationFunction;
        _neuralNetwork = neuralNetwork;
    }

    public override bool Handle(float deltaT)
    {
        if (!Enabled)
        {
            return false;
        }

        _snakeBehaviour ??= Scene.Plugins.Get<SnakeBehaviour>(SnakeBehaviour.BehaviourName);

        UpdateVision(); // update input layer
        var output = _neuralNetwork.FeedForward(_activationFunction, vision);

        _snakeBehaviour.SnakeHead.Direction = GetNewDirection(output);

        return true;
    }

    public override string Name()
    {
        return BehaviourName;
    }

    #region Dependencies

    private readonly IActivationFunction _activationFunction;

    private readonly NeuralNetwork _neuralNetwork;

    #endregion

    #region Vision

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Direction GetNewDirection(float[] output)
    {
        var maxOutput = 0.0f;
        int direction = (int)_snakeBehaviour.SnakeHead.Direction;
        for (var i = 0; i < output.Length; i++)
        {
            if (output[i] > maxOutput)
            {
                maxOutput = output[i];
                direction = i;
            }
        }

        return (Direction)direction;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool BodyCollide(Vector2 v)
    {
        for(int i = 0; i < _snakeBehaviour.SnakeBody.Count; i++)
        {
            if (_snakeBehaviour.SnakeBody[i].CollidesWith(v))
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool WallCollide(Vector2 v)
    {
        return v.X - SnakeGameConfig.CellSize.X / 2.0f <= 0 ||
               v.Y - SnakeGameConfig.CellSize.Y / 2.0f <= 0 ||
               v.X + SnakeGameConfig.CellSize.X / 2.0f >= SnakeGameConfig.BoardSize.X ||
               v.Y + SnakeGameConfig.CellSize.Y / 2.0f >= SnakeGameConfig.BoardSize.Y;
    }

    private bool FoodCollide(Vector2 v)
    {
        return _snakeBehaviour.SnakeFood.CollidesWith(v);
    }

    private void Look(Vector2 direction, int index)
    {
        var foodFound = false;
        var bodyFound = false;
        // Food / body cannot be ontop of current position
        var pos = _snakeBehaviour.SnakeHead.Primitive.BoundingRectangle.Center + direction;
        var distance = 1; // min distance = 1

        while (!WallCollide(pos))
        {
            if (!foodFound && FoodCollide(pos))
            {
                // Food found in given direction
                foodFound = true;
            }

            if (!bodyFound && BodyCollide(pos))
            {
                // own body found in given direction
                bodyFound = true;
            }

            pos += direction;
            distance += 1;
        }

        vision[index] = foodFound ? 1 : 0; // food found in given direction
        vision[index + 1] = bodyFound ? 1 : 0;// No body found in given direction
        vision[index + 2] = 1.0f / distance;// Set distance to wall
    }

    private void UpdateVision()
    {
        Look(new Vector2(SnakeGameConfig.CellSize.X, 0), 0);
        Look(new Vector2(SnakeGameConfig.CellSize.X, SnakeGameConfig.CellSize.Y), 3);
        Look(new Vector2(SnakeGameConfig.CellSize.X, -SnakeGameConfig.CellSize.Y), 6);

        Look(new Vector2(-SnakeGameConfig.CellSize.X, 0), 9);
        Look(new Vector2(-SnakeGameConfig.CellSize.X, -SnakeGameConfig.CellSize.Y), 12);
        Look(new Vector2(-SnakeGameConfig.CellSize.X, SnakeGameConfig.CellSize.Y), 15);

        Look(new Vector2(0, -SnakeGameConfig.CellSize.Y), 18);
        Look(new Vector2(0, SnakeGameConfig.CellSize.Y), 21);
    }

    #endregion
}