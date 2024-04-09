using Godot;
using System;
using Trafficinator;

public partial class IsAtEndButton : CheckButton
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Toggled += on =>
		{
			var road = GetParent() as SingleWayRoad;
			if (road != null) road.DeleteAtEnd = on;
		};
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
