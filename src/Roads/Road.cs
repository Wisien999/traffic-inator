using System.Collections.Generic;
using QuikGraph;
using Godot;

namespace Trafficinator;

public abstract partial class Road : Path2D
{
    abstract public List<Lane> Lanes { get; }
    abstract public bool IsDirected { get; }

    private RoadConnection _source;
    // [Export]
    public virtual RoadConnection Source
    {
        get => _source;
        set
        {
            _source?.OutRoads?.Remove(this);
            _source = value;
            _source?.OutRoads?.Add(this);
        }
    }
    // [Export]
    public virtual RoadConnection Target { get; set; }

    public Road(Curve2D curve, RoadConnection source, RoadConnection target)
    {
        Curve = curve;
        Source = source;
        Target = target;
    }
    public Road(Curve2D curve)
    {
        Curve = curve;
    }

    public Road() {}


    public double Length => Lanes[0].Length;

    public abstract bool AddCar(RoadConnection source, Car car);
    public abstract bool AddCarAt(Building source, Car car);
}
