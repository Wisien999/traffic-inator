using System;
using System.Collections.Generic;

namespace test_godot_game;

using Godot;

public partial class Road : Path2D
{
    private readonly LinkedList<Car> _cars = new();
    public bool DeleteAtEnd = false;
    public override void _Draw()
    {
        DrawPolyline(Curve.GetBakedPoints(), Colors.White, 7);
    }

    public override void _Ready()
    {
        SpawnCar();
    }

    public override void _Process(double delta)
    {
        var curveLength = Curve.GetBakedLength();
        var lastAvailablePos = curveLength;
        foreach (var car in _cars)
        {
            var lastCarPos = car.Progress;
            car.Progress = Math.Min(lastAvailablePos, lastCarPos + 200 * (float)delta);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if(lastCarPos != car.Progress)
                car.QueueRedraw();
            lastAvailablePos = car.Progress - 25;
        }
        
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (DeleteAtEnd && _cars.First != null && _cars.First.Value.Progress == curveLength)
        {
            _cars.First.Value.QueueFree();
            _cars.RemoveFirst();
        }    
    }

    public void SpawnCar()
    {
        var car = new Car();
        _cars.AddLast(car);
       AddChild(car); 
    }
}