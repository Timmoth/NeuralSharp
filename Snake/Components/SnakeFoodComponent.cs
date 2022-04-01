using Aptacode.AppFramework.Components.Primitives;
using Aptacode.Geometry.Primitives;

namespace Snake.Components;

public sealed class SnakeFoodComponent : PolygonComponent
{
    public SnakeFoodComponent(Polygon primitive) : base(primitive)
    {
    }
}