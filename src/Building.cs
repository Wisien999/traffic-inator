using Godot;
using System;

namespace Trafficinator;

public partial class Building : Node2D
{
    [Export]
    public int SpawnRate = 5;
    [Export]
    public Road AttachedRoad;

    public Vector2[] Outline { get; set; }

    private GlobalMapData GlobalMapData => GetNode<GlobalMapData>("/root/GlobalMapData");

    private Timer spawnTimer = new();


    public override void _Ready()
    {
        AddChild(spawnTimer);
        spawnTimer.Connect("timeout", Callable.From(SpawnCar));
        spawnTimer.Start(SpawnRate);
        // GD.Print("Building ready");
        // GD.Print(GlobalMapData);
    }


    public override void _Draw()
    {
        if (Outline != null)
        {
            DrawPolygon(Outline, new Color[] { Colors.Olive });
        }
        else
        {
            DrawRect(new Rect2(-20, -10, 40, 20), Colors.Tan);
        }
    }


    public void SpawnCar()
    {
        // GD.Print("Spawning car");
        var newCar = GlobalMapData?.CarManager?.randomTargetedCar(AttachedRoad.Target);
        newCar.Color = Colors.Gold;
        var res = AttachedRoad?.AddCarAt(this, newCar);
        // var res = AttachedRoad?.AddCarAt(this, new RandomCar() { Color = Colors.Gold});
        // GD.Print("Spawned car ", res);
    }
}
