using System.Collections.Generic;
using Godot.Collections;
using Godot;

using Trafficinator;

public abstract partial class RoadConnection: Node2D
{
	public abstract Array<Road> AttachedRoads { get; set; }
	public abstract void CarEntered(Road from, Car car);
}
