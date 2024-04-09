using Godot;
using System.Collections.Generic;
using System;
using Godot.Collections;

namespace Trafficinator;

public partial class Roundabout: Node2D, RoadConnection
{
	[Export]
	public Array<Road> AttachedRoads { get; set; } = new();
	private Queue<(Car, DateTime)> cars = new();
	private int lastUpdateTime = 0;


	public override void _Ready()
	{
			
	}

	public override void _Draw()
	{
		// DrawCircle(Position, 10, Colors.Black);
		DrawCircle(new Vector2(0, 0), 10, Colors.Black); // this actualy draws on node position

		if (cars.Count > 0)
		{
			var (car, _) = cars.Peek();
			DrawRect(new Rect2(-10, -10, 20, 20), car.Color);
			// DrawRect(new Rect2(-10, -10, 20, 20), Colors.Red);
		}
	}

	public override void _Process(double delta)
	{
		if (cars.Count == 0)
			return;

		var (car, entranceTime) = cars.Peek();

		if (entranceTime.AddSeconds(1) > DateTime.Now)
		{
			cars.Dequeue();
			AttachedRoads[0].AddCar(this, car);
		}

	}

	public void CarEntered(Road from, Car car)
	{
		cars.Enqueue((car, DateTime.Now));
	}
}
