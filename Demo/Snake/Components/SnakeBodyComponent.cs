using Aptacode.AppFramework.Components.Primitives;
using Aptacode.Geometry.Primitives;
using Demo.Snake;

namespace Snake.Components;

public sealed class SnakeBodyComponent : PolygonComponent
{
    public SnakeBodyComponent(Polygon primitive) : base(primitive)
    {
    }

    public Direction Direction { get; set; }
}