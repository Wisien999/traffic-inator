using Godot;
using System;

namespace Trafficinator;

public partial class Building : Node2D {
	[Export]
	public int SpawnRate = 5;
	[Export]
	private Road AttachedRoad;

	private Timer spawnTimer = new();


	public override void _Ready() {
		AddChild(spawnTimer);
		spawnTimer.Connect("timeout", Callable.From(SpawnCar));
		spawnTimer.Start(SpawnRate);
		GD.Print(spawnTimer);
	}


	public override void _Draw() {
		DrawRect(new Rect2(-10, -10, 40, 20), Colors.Tan);
	}


	public void SpawnCar() {
		GD.Print("Spawning car");
		var res = AttachedRoad?.AddCarAt(this, new Car());
		GD.Print("Spawned car ", res);
	}
}
