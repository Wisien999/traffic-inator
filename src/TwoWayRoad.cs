using Godot;
using System;

namespace test_godot_game;

public partial class TwoWayRoad : Line2D
{
	SingleWayRoad lane1 = new SingleWayRoad();
	SingleWayRoad lane2 = new SingleWayRoad();

	private static float SpaceDistance = 6;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.Width = 24;
		lane1.Curve = new Curve2D();
		lane2.Curve = new Curve2D();


		// var (starting2, ending1) = MovedStarting(Points[Points.Length-1], Points[Points.Length-2]);
		lane1.Curve.AddPoint(MovedToRight(Points[1] - Points[0], Points[0]));
		for (int i = 1; i < Points.Length-1; i++)
		{
			var p = CalculateDistancedPointsOnAngle(Points[i-1], Points[i], Points[i + 1]);
			lane1.Curve.AddPoint(p);
		}
		lane1.Curve.AddPoint(MovedToRight(Points[Points.Length-1] - Points[Points.Length-2], Points[Points.Length-1]));


		lane2.Curve.AddPoint(MovedToRight(Points[Points.Length-2] - Points[Points.Length-1], Points[Points.Length-1]));
		for (int i = Points.Length-2; i >= 1; i--)
		{
			var p = CalculateDistancedPointsOnAngle(Points[i + 1], Points[i], Points[i - 1]);
			lane2.Curve.AddPoint(p);
		}
		lane2.Curve.AddPoint(MovedToRight(Points[0] - Points[1], Points[0]));

		AddChild(lane1);
		AddChild(lane2);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _Draw()
	{
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

		
		GD.Print("cos", Math.Abs(baDirection.Cross(bcDirection)));
		move = move * ((float) Math.Pow(0.5, Math.Abs(baDirection.Cross(bcDirection)))) * 2f;

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

}
