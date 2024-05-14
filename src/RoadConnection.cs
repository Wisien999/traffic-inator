using System.Collections.Generic;
using Godot.Collections;
using Godot;

using Trafficinator;

public abstract partial class RoadConnection: Node2D
{
	public abstract Array<Road> OutRoads { get; set; }
	public abstract bool CarEntered(Road from, Car car);
}
