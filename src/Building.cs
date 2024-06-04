using Godot;
using System;

namespace Trafficinator;

public partial class Building : Node2D
{

    private static Random random = new Random();

    [Export]
    public int SpawnRate = 20;
    [Export]
    public float SpawnChance = 0.1f;
    [Export]
    public Road AttachedRoad;
    [Export]
    public Vector2[] Outline { get; set; }
    [Export]
    public Color OutlineColor { get; set; } = Colors.Olive;

    private GlobalMapData GlobalMapData => GetNode<GlobalMapData>("/root/GlobalMapData");

    private Timer spawnTimer = new();


    public override void _Ready()
    {
        AddChild(spawnTimer);
        spawnTimer.Connect("timeout", Callable.From(TimerCallback));
        spawnTimer.OneShot = true;
        spawnTimer.Start(SpawnRate * random.NextSingle());
        // GD.Print("Building ready");
        // GD.Print(GlobalMapData);
    }


    public override void _Draw()
    {
        if (Outline != null)
        {
            DrawPolygon(Outline, new Color[] { OutlineColor });
        }
        else
        {
            DrawRect(new Rect2(-20, -10, 40, 20), Colors.Tan);
        }
    }

    public void TimerCallback()
    {
        SpawnCarWithChance();
        spawnTimer.Start(SpawnRate * random.NextSingle());
    }

    public void SpawnCarWithChance()
    {
        if (random.NextSingle() < SpawnChance)
        {
            SpawnCar();
        }
    }

    public void SpawnCar()
    {
        // GD.Print("Spawning car");
        Car newCar = null;
        var carCreated = GlobalMapData?.CarManager?.TryRandomTargetedCar(AttachedRoad.Target, out newCar);
        if (carCreated.GetValueOrDefault(false) && newCar != null)
        {
            newCar.Color = Colors.Gold;
            var res = AttachedRoad?.AddCarAt(this, newCar);
        }
    }
}
