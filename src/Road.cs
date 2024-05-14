using QuikGraph;
using Godot;

using Trafficinator;

public abstract partial class Road: Path2D, IEdge<RoadConnection> 
{
	private RoadConnection _source;
	[Export]
	public virtual RoadConnection Source { 
		get => _source; 
		set { 
			_source?.OutRoads?.Remove(this); 
			_source = value;
			_source?.OutRoads?.Add(this);
		} 
	}
	[Export]
	public virtual RoadConnection Target { get; set; }

	public double Length { get => Curve.GetBakedLength(); }

	public abstract bool AddCar(RoadConnection source, Car car);
	public abstract bool AddCarAt(Building source, Car car);
}
