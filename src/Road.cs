using QuikGraph;
using Godot;

using Trafficinator;

public abstract partial class Road: Path2D, IEdge<RoadConnection> 
{
	[Export]
	public RoadConnection Source { get; set; }
	[Export]
	public RoadConnection Target { get; set; }

	public abstract bool AddCar(RoadConnection source, Car car);
}
