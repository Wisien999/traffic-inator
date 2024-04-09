using System.Collections.Generic;
using Godot.Collections;

using Trafficinator;

public interface RoadConnection
{
	public Array<Road> AttachedRoads { get; }
	public void CarEntered(Road from, Car car);
}
