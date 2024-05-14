using System;
using System.Collections.Generic;

namespace Trafficinator;

using Godot;

public partial class SingleWayRoad : Road
{
	private readonly LinkedList<Car> _cars = new();
	public bool DeleteAtEnd = false;
	public override void _Draw()
	{
		DrawPolyline(Curve.GetBakedPoints(), Colors.Black, 7);
	}


	public override void _Ready()
	{
		// GD.Print(
		// 	"SingleWayRoad ready ",
		// 	Curve.PointCount,
		// 	" ",
		// 	Curve.GetClosestPoint(new Vector2(0, 0)),
		// 	Curve.GetClosestOffset(new Vector2(0, 0))
		// 	);
		SpawnCar();
	}

	public override void _Process(double delta)
	{
		var curveLength = Curve.GetBakedLength();
		var lastAvailablePos = curveLength;
		foreach (var car in _cars)
		{
			var lastCarPos = car.Progress;
			car.Progress = Math.Min(lastAvailablePos, lastCarPos + 300 * (float)delta);
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if(lastCarPos != car.Progress)
				car.QueueRedraw();
			lastAvailablePos = car.Progress - 25;

			if (car is IdealTargetedCar targetCar && targetCar.NextEdgeIdx > targetCar.PlannedPath.Count) {
				var designedProgress = Curve.GetClosestOffset(targetCar.Target.Position);

				if (car.Progress >= designedProgress)
				{
					RemoveChild(car);
					_cars.Remove(car);
				}
			}
		}

		var firstCar = _cars.First;
		// ReSharper disable once CompareOfFloatsByEqualityOperator
		if (firstCar != null && firstCar.Value.Progress == curveLength)
		{
			if (DeleteAtEnd)
			{
				_cars.First.Value.QueueFree();
				_cars.RemoveFirst();
			}
			else if (Target != null)
			{
				var res = Target.CarEntered(this, firstCar.Value);
				if (res) {
					_cars.RemoveFirst();
					RemoveChild(firstCar.Value);
				}

			}
		}
		
	}

	public void SpawnCar()
	{
		var car = new RandomCar();
		_cars.AddLast(car);
		AddChild(car); 
	}

	override public bool AddCar(RoadConnection source, Car car)
	{
		GD.Print("Adding car 1 " + source.Name + " " + Source.Name);
		if (source != Source) return false;
		GD.Print("Adding car 2");
		if (_cars.Last?.Value.Progress < 10) return false;
		GD.Print("Adding car 3");
		car.Progress = 0;

		_cars.AddLast(car);
		AddChild(car);

		return true;
	}

	override public bool AddCarAt(Building source, Car car)
	{
		var entryProgress = Curve.GetClosestOffset(source.Position);
		GD.Print("Entry progress ", entryProgress, " / ", Curve.GetBakedLength());

		var nextCar = findFirstCarAfter(entryProgress);

		// no space
		if (nextCar?.Progress - entryProgress < 30) return false;

		if (_cars.Count == 0 || nextCar == null)
		{
			GD.Print("Adding car at the end");
			car.Progress = entryProgress;
			_cars.AddFirst(car);
			AddChild(car);

			return true;
		}

		GD.Print("Adding car before ", nextCar.Progress);
		car.Progress = entryProgress;
		_cars.AddAfter(_cars.Find(nextCar), car);
		AddChild(car);

		return true;
	}

	private Car findFirstCarAfter(float progress)
	{
		Car lastCar = null;
		foreach (var car in _cars)
		{
			if (car.Progress > progress)
			{
				lastCar = car;
			}
		}
		return lastCar;
	}
}
