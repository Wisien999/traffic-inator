using Godot;
using System;
using System.Collections.Generic;
using QuikGraph;

namespace Trafficinator;

public partial class TwoWayRoad : Road
{
	private Lane lane1 { get; init; }
	private Lane lane2 { get; init; }

	[Export]
	public override RoadConnection Source { get { return lane1.Source; } set { lane1.Source = value; lane2.Target = value; } }
	[Export]
	public override RoadConnection Target { get { return lane1.Target; } set { lane1.Target = value; lane2.Source = value; } }

	override public List<Lane> Lanes => new List<Lane> { lane1, lane2 };

	private static float SpaceDistance = 3;

	public override bool IsDirected => false;

	public TwoWayRoad(Curve2D curve, RoadConnection source, RoadConnection target) : base(curve)
	{
		var (curve1, curve2) = GetCurves();

		lane1 = new Lane()
		{
			Road = this,
			Curve = curve1,
		};
		lane2 = new Lane()
		{
			Road = this,
			Curve = curve2,
		};

		Source = source;
		Target = target;
	}

	public TwoWayRoad() : base()
	{
		lane1 = new Lane()
		{
			Road = this,
		};
		lane2 = new Lane()
		{
			Road = this,
		};
	}

	public override void _Ready()
	{
		var (curve1, curve2) = GetCurves();
		lane1.Curve = curve1;
		lane2.Curve = curve2;

		// for (int i = 0; i < lane1.Curve.PointCount; i++) {
		// 	var p = lane1.Curve.GetPointPosition(i);
		//
		// 	GD.Print("p", i, ": ", p);
		// }

		AddChild(lane1);
		AddChild(lane2);
	}

	private (Curve2D, Curve2D) GetCurves()
	{
		var curve1 = new Curve2D();
		var curve2 = new Curve2D();

		try
		{
			curve1.AddPoint(MovedToRight(Curve.GetPointPosition(1) - Curve.GetPointPosition(0), Curve.GetPointPosition(0)));
			for (int i = 1; i < Curve.PointCount - 1; i++)
			{
				var p = CalculateDistancedPointsOnAngle(Curve.GetPointPosition(i - 1), Curve.GetPointPosition(i), Curve.GetPointPosition(i + 1));
				curve1.AddPoint(p);
			}
			curve1.AddPoint(MovedToRight(Curve.GetPointPosition(Curve.PointCount - 1) - Curve.GetPointPosition(Curve.PointCount - 2), Curve.GetPointPosition(Curve.PointCount - 1)));


			curve2.AddPoint(MovedToRight(Curve.GetPointPosition(Curve.PointCount - 2) - Curve.GetPointPosition(Curve.PointCount - 1), Curve.GetPointPosition(Curve.PointCount - 1)));
			for (int i = Curve.PointCount - 2; i >= 1; i--)
			{
				var p = CalculateDistancedPointsOnAngle(Curve.GetPointPosition(i + 1), Curve.GetPointPosition(i), Curve.GetPointPosition(i - 1));
				curve2.AddPoint(p);
			}
			curve2.AddPoint(MovedToRight(Curve.GetPointPosition(0) - Curve.GetPointPosition(1), Curve.GetPointPosition(0)));

		}
		catch (System.Exception e)
		{
			GD.Print("Error in TwoWayRoad", e);

			throw;
		}

		return (curve1, curve2);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _Draw()
	{
		DrawPolyline(Curve.GetBakedPoints(), Colors.White, 20);
	}

	private Vector2 CalculateDistancedPointsOnAngle(Vector2 a, Vector2 b, Vector2 c)
	{
		var baDirection = b.DirectionTo(a);
		var bcDirection = b.DirectionTo(c);
		var direction = (baDirection + bcDirection).Normalized();
		var move = direction * SpaceDistance;

		var p0 = b + move;
		var product = leftOrRight(a, b, p0);


		move = move * ((float)Math.Pow(0.5, Math.Abs(baDirection.Cross(bcDirection)))) * 1.8f;

		if (product > 0)
		{
			return b + move;
		}

		return b - move;
	}

	private float leftOrRight(Vector2 a, Vector2 b, Vector2 p)
	{
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
			// GD.Print("Adding car to lane1");
			return lane1.AddCar(source, car);
		}
		if (source == lane2.Source)
		{
			// GD.Print("Adding car to lane2");
			return lane2.AddCar(source, car);
		}

		// GD.Print("Source is not in the road");
		return false;
	}

	override public bool AddCarAt(Building source, Car car)
	{
		return lane1.AddCarAt(source, car);

	}
}
