using Godot;
using System;
using QuikGraph;

namespace Trafficinator;

public partial class TwoWayRoad : Road, IUndirectedEdge<RoadConnection>
{
	SingleWayRoad lane1 = new SingleWayRoad();
	SingleWayRoad lane2 = new SingleWayRoad();

	[Export]
	public override RoadConnection Source { get {return lane1.Source; } set { lane1.Source = value; lane2.Target = value; } } 
	[Export]
	public override RoadConnection Target { get {return lane1.Target; } set { lane1.Target = value; lane2.Source = value; } }

	private static float SpaceDistance = 6;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		lane1.Curve = new Curve2D();
		lane2.Curve = new Curve2D();

		try
		{
			

		lane1.Curve.AddPoint(MovedToRight(Curve.GetPointPosition(1) - Curve.GetPointPosition(0), Curve.GetPointPosition(0)));
		for (int i = 1; i < Curve.PointCount-1; i++)
		{
			var p = CalculateDistancedPointsOnAngle(Curve.GetPointPosition(i-1), Curve.GetPointPosition(i), Curve.GetPointPosition(i + 1));
			lane1.Curve.AddPoint(p);
		}
		lane1.Curve.AddPoint(MovedToRight(Curve.GetPointPosition(Curve.PointCount-1) - Curve.GetPointPosition(Curve.PointCount-2), Curve.GetPointPosition(Curve.PointCount-1)));


		lane2.Curve.AddPoint(MovedToRight(Curve.GetPointPosition(Curve.PointCount-2) - Curve.GetPointPosition(Curve.PointCount-1), Curve.GetPointPosition(Curve.PointCount-1)));
		for (int i = Curve.PointCount-2; i >= 1; i--)
		{
			var p = CalculateDistancedPointsOnAngle(Curve.GetPointPosition(i + 1), Curve.GetPointPosition(i), Curve.GetPointPosition(i - 1));
			lane2.Curve.AddPoint(p);
		}
		lane2.Curve.AddPoint(MovedToRight(Curve.GetPointPosition(0) - Curve.GetPointPosition(1), Curve.GetPointPosition(0)));

		AddChild(lane1);
		AddChild(lane2);
		}
		catch (System.Exception)
		{
			GD.Print("Error in TwoWayRoad");
			
			throw;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _Draw()
	{
		DrawPolyline(Curve.GetBakedPoints(), Colors.White, 20);
		lane1._Draw();
		lane2._Draw();
	}

	private Vector2 CalculateDistancedPointsOnAngle(Vector2 a, Vector2 b, Vector2 c)
	{
		var baDirection = b.DirectionTo(a);
		var bcDirection = b.DirectionTo(c);
		var direction = baDirection + bcDirection;
		var move = direction * SpaceDistance;

		var p0 = b + move;
		var product = leftOrRight(a, b, p0);

		
		move = move * ((float) Math.Pow(0.5, Math.Abs(baDirection.Cross(bcDirection)))) * 1.8f;

		if (product > 0)
		{
			return b + move;
		}

		return b - move;
	}

	private float leftOrRight(Vector2 a, Vector2 b, Vector2 p) {
		var ab = b - a;
		var ap = p - a;

		return ab.Cross(ap);
	}

	private Vector2 MovedToRight(Vector2 roadDirection, Vector2 pinned)
	{
		var moveDirection = -roadDirection.Orthogonal().Normalized();
		var move = moveDirection * SpaceDistance;

		return pinned + move;
	}

	override public bool AddCar(RoadConnection source, Car car)
	{
		if (source == lane1.Source)
		{
			return lane1.AddCar(source, car);
		}
		if (source == lane2.Source)
		{
			return lane2.AddCar(source, car);
		}

		return false;
	}
}
