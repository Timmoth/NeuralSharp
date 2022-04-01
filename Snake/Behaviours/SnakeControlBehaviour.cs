using Aptacode.AppFramework.Plugins.Behaviours;
using Aptacode.AppFramework.Scene;
using Aptacode.AppFramework.Scene.Events;
using Snake.States;

namespace Snake.Behaviours;

public class SnakeControlBehaviour : BehaviourPlugin<UiEvent>
{
    public static string BehaviourName = "SnakeControl";

    public SnakeControlBehaviour(Scene scene) : base(scene)
    {
    }

    public override bool Handle(UiEvent uiEvent)
    {
        if (uiEvent is not KeyDownEvent keyEvent)
        {
            return false;
        }

        var state = Scene.Plugins.Tick.Get<SnakeBehaviour>(SnakeBehaviour.BehaviourName);
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