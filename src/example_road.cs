using Godot;
using System;

namespace Trafficinator;

public partial class example_road : Node2D
{
	public override void _Ready()
	{
		var road = new SingleWayRoad();
		road.Position = new Vector2(2, 31);
		var curve = new Curve2D();
		curve.AddPoint(new Vector2(0, 0));
		curve.AddPoint(new Vector2(100, 0));
		curve.AddPoint(new Vector2(100, 100));
		road.Curve = curve;


		AddChild(road);
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
		
	public override void _Draw()
	{
		GD.Print("Drawing");
	}

}
