using Godot;
using System;

namespace test_godot_game;

public partial class TwoWayRoad : Line2D
{

	SingleWayRoad lane1 = new SingleWayRoad();
	SingleWayRoad lane2 = new SingleWayRoad();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var curve1 = new Curve2D();
		var curve2 = new Curve2D();

		foreach (var point in this.Points)
		{
			GD.Print(point);
			curve1.AddPoint(point);
		}
		// add to lane2 in revese order
		Array.Reverse(this.Points);
		GD.Print("Reversed");
		for (int i = this.Points.Length -1; i >= 0 ; i--)
		{
			curve2.AddPoint(Points[i] + new Vector2(10, 10));
		}

		lane1.Curve = curve1;
		lane2.Curve = curve2;

		AddChild(lane1);
		AddChild(lane2);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _Draw()
	{
		lane1._Draw();
		lane2._Draw();
	}

	
	private Godot.Collections.Array ReverseArray(Godot.Collections.Array array)
	{
		Godot.Collections.Array newArray = new Godot.Collections.Array();
		for (int i = array.Count - 1; i >= 0; i--)
		{
			newArray.Add(array[i]);
		}
		return newArray;
	}

}
