using Aptacode.AppFramework;
using Aptacode.AppFramework.Events;
using Aptacode.AppFramework.Plugins;
using Demo.Snake;

namespace Snake.Behaviours;

public class SnakeManualControl : Plugin
{
    public static string BehaviourName = "SnakeControl";

    public SnakeManualControl(Scene scene) : base(scene)
    {
    }

    public override bool Handle(UiEvent uiEvent)
    {
        if (!Enabled)
        {
            return false;
        }

        if (uiEvent is not KeyDownEvent keyEvent)
        {
            return false;
        }

        var state = Scene.Plugins.Get<SnakeBehaviour>(SnakeBehaviour.BehaviourName);
        if (state == null)
        {
            return false;
        }

        state.SnakeHead.Direction = keyEvent.Key switch
        {
            "ArrowUp" => Direction.Up,
            "ArrowDown" => Direction.Down,
            "ArrowLeft" => Direction.Left,
            "ArrowRight" => Direction.Right,
            _ => state.SnakeHead.Direction
        };

        return false;
    }

    public override string Name()
    {
        return BehaviourName;
    }
}