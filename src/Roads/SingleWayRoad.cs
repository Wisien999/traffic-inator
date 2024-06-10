using System;
using System.Collections.Generic;

namespace Trafficinator;

using Godot;

public partial class SingleWayRoad : Road
{
	private Lane lane = new Lane();

	public override bool IsDirected => true;

	override public List<Lane> Lanes { get; }

	[Export]
	override public RoadConnection Source
	{
		get => lane.Source;
		set
		{
			base.Source = value;
			lane.Source = value;
		}
	}
	[Export]
	override public RoadConnection Target { get => lane.Target; set => lane.Target = value; }

	public SingleWayRoad(Curve2D curve, RoadConnection source, RoadConnection target) : base(curve)
	{
		lane = new Lane()
		{
			Road = this,
			Curve = curve,
		};
		Source = source;
		Target = target;

		Lanes = new List<Lane> { lane };
	}

	public SingleWayRoad() : base()
	{
		lane = new Lane()
		{
			Road = this,
		};

		Lanes = new List<Lane> { lane };
	}

	override public void _Ready()
	{
		lane.Curve = Curve;
		AddChild(lane);
	}

	override public bool AddCarAt(Building source, Car car)
	{
		return lane.AddCarAt(source, car);
	}

	override public bool AddCar(RoadConnection source, Car car)
	{
		return lane.AddCar(source, car);
	}
}
