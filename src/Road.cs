using QuikGraph;
using Godot;

using Trafficinator;

public abstract partial class Road: Path2D, IEdge<RoadConnection> 
{
	public RoadConnection Source { get; set; }
	public RoadConnection Target { get; set; }

	public void AddCar(RoadConnection source, Car car)
	{
	}
}
