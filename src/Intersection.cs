using Godot;
using System.Collections.Generic;

namespace Trafficinator;

public partial class Intersection : Polygon2D//, RoadConnection
{
    public List<Road> AttachedRoads = new();

    public override void _Ready()
    {
        foreach (Vector2 point in this.Polygon)
        {
            GD.Print(point);
        }

    }

    public override void _Draw()
    {
    }

    public override void _Process(double delta)
    {

    }
}
