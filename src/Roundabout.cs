using Godot;
using System.Collections.Generic;
using System;
using Godot.Collections;

namespace Trafficinator;

public partial class Roundabout : RoadConnection
{
    [Export]
    override public Array<Road> OutRoads { get; set; } = new();
    private Queue<(Car, DateTime, Road)> cars = new();
    private int lastUpdateTime = 0;
    [Export]
    int MaxCars = 2;

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
            DrawRect(new Rect2(-5, -5, 10, 10), car.Color);
            // DrawRect(new Rect2(-10, -10, 20, 20), Colors.Red);
        }
    }

    public override void _Process(double delta)
    {
        if (OutRoads.Count == 0) return;
        if (cars.Count == 0) return;


        var (car, entranceTime, source) = cars.Peek();

        if (entranceTime.AddSeconds(3) > DateTime.Now)
        {
            return;
        }


        var dispatched = DispatchCar(car);


        if (dispatched)
        {
            cars.Dequeue();
            QueueRedraw();
        }

    }

    private bool DispatchCar(Car car)
    {
        return car switch
        {
            null => throw new ArgumentNullException(),
            IdealTargetedCar targetedCar =>
                DispatchTargetedCar(targetedCar),
            RandomCar randomCar =>
                DispatchRandomCar(randomCar),
            _ => throw new ArgumentException("Invalid car type")
        };
    }

    private bool DispatchRandomCar(RandomCar car)
    {
        var i = random.Next(0, OutRoads.Count);
        var dispatched = OutRoads[i].AddCar(this, car);
        return dispatched;
    }


    private bool DispatchTargetedCar(IdealTargetedCar car)
    {
        if (car.NextEdgeIdx < car.PlannedPath.Count)
        {
            GD.Print("Dispatching car to next road");
            var nextRoad = car.PlannedPath[car.NextEdgeIdx];

            var res = nextRoad.AddCar(this, car);
            if (res)
            {
                car.NextEdgeIdx++;
            }
            return res;
        }

        GD.Print("Dispatching car to target");
        car.NextEdgeIdx++;
        var res2 = car.Target.AttachedRoad.AddCar(this, car);

        return res2;
    }

    // private Road findRoadTo(RoadConnection target) {
    // 	foreach (var road in OutRoads)
    // 	{
    // 		if (road.Target == target)
    // 		{
    // 			return road;
    // 		}
    // 	}
    // 	return null;
    // }


    override public bool CarEntered(Road from, Car car)
    {
        if (cars.Count >= MaxCars) return false;

        cars.Enqueue((car, DateTime.Now, from));
        QueueRedraw();

        return true;
    }
}
