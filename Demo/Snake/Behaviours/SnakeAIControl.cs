using System.Numerics;
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

        var vision = GetVision();
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

    private Direction GetNewDirection(float[] output)
    {
        var maxOutput = 0.0f;
        var direction = _snakeBehaviour.SnakeHead.Direction;
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

    private bool BodyCollide(Vector2 v)
    {
        return _snakeBehaviour.SnakeBody.Any(b => b.CollidesWith(v));
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
        return _snakeBehaviour.SnakeFood.CollidesWith(v);
    }

    private float[] Look(Vector2 direction)
    {
        var look = new float[3];
        var pos = _snakeBehaviour.SnakeHead.Primitive.BoundingRectangle.Center;
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
}