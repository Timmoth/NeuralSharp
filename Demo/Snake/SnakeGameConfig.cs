using System.Numerics;
using Demo.Snake;

namespace Snake;

public static class SnakeGameConfig
{
    public static readonly int LifeTime = 100;

    public static readonly Random _rand = new();
    public static readonly Vector2 BoardSize = new(600, 600);
    public static readonly Vector2 CellSize = new(25, 25);
    public static readonly Vector2 InnerCellSize = new(20, 20);

    public static readonly Vector2 CenterCell = new(CellSize.X * HorizontalCells / 2, CellSize.Y * VerticalCells / 2);

    public static readonly int HorizontalCells = (int)(BoardSize / CellSize).X;
    public static readonly int VerticalCells = (int)(BoardSize / CellSize).X;

    public static Vector2 RandomCell()
    {
        return new Vector2(_rand.Next(1, HorizontalCells - 1) * CellSize.X,
            _rand.Next(1, VerticalCells - 1) * CellSize.Y);
    }

    public static Vector2 CellPosition(int x, int y)
    {
        return new Vector2(x * CellSize.X,
            y * CellSize.Y);
    }

    public static Vector2 GetMovement(Direction direction)
    {
        return direction switch
        {
            Direction.Up => new Vector2(0, CellSize.Y),
            Direction.Down => new Vector2(0, -CellSize.Y),
            Direction.Left => new Vector2(-CellSize.X, 0),
            Direction.Right => new Vector2(CellSize.X, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }

    public static Direction Reverse(Direction direction)
    {
        return direction switch
        {
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            Direction.Left => Direction.Left,
            Direction.Right => Direction.Right,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }
}