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
      _source?.AttachedRoads?.Remove(this); 
      _source = value;
      _source?.AttachedRoads?.Add(this);
    } 
  }
	[Export]
	public virtual RoadConnection Target { get; set; }

	public abstract bool AddCar(RoadConnection source, Car car);
}
