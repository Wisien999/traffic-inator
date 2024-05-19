using Godot;
using System;
using Trafficinator;

public partial class SpawnButton : Button
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Pressed += () =>
        {
            var parent = GetParent() as SingleWayRoad;
            parent?.SpawnCar();
        };
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

}
