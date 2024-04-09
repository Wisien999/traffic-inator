using Godot;
using System.Collections.Generic;
using System;
using Godot.Collections;

namespace Trafficinator;

public partial class Roundabout: RoadConnection
{
	[Export]
	override public Array<Road> AttachedRoads { get; set; } = new();
	private Queue<(Car, DateTime, Road)> cars = new();
	private int lastUpdateTime = 0;

	private Random random = new();


	public override void _Ready()
	{
			
	}

	public override void _Draw()
	{
		// DrawCircle(Position, 10, Colors.Black);
		DrawCircle(new Vector2(0, 0), 10, Colors.Black); // this actualy draws on node position

		if (cars.Count > 0)
		{
			var (car, _, _) = cars.Peek();
			DrawRect(new Rect2(-10, -10, 20, 20), car.Color);
			// DrawRect(new Rect2(-10, -10, 20, 20), Colors.Red);
		}
	}

	public override void _Process(double delta)
	{
		if (cars.Count == 0) return;


		var (car, entranceTime, source) = cars.Peek();

		if (entranceTime.AddSeconds(1) > DateTime.Now)
		{
			var dispatched = false;
			while (!dispatched) // find a road to attach
			{
				var i = random.Next(0, AttachedRoads.Count);
				GD.Print("Car is dispatching to ", i);
				dispatched = AttachedRoads[i].AddCar(this, car);
				cars.Dequeue();
			}
		}

	}

	override public void CarEntered(Road from, Car car)
	{
		cars.Enqueue((car, DateTime.Now, from));
	}
}
