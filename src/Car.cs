using Godot;
using System.Collections.Generic;

namespace Trafficinator;

public abstract partial class Car : PathFollow2D
{
	public enum Type
	{
		Random,
		IdealTargeted
	}

	public Color Color = Colors.Blue;
	public abstract Type CarType { get; }

	public Car()
	{
		Loop = false;
	}

	public override void _Ready()
	{
	}

	public override void _Draw()
	{
		DrawRect(new Rect2(-10, -2.5f, 20, 5), Color);
	}

	public override void _Process(double delta)
	{

	}
}
public partial class RandomCar : Car
{
	override public Type CarType { get => Type.Random; }
}
public partial class IdealTargetedCar : Car
{
	override public Type CarType { get => Type.IdealTargeted; }

	public Building Target { get; private set; }
	public List<Road> PlannedPath { get; private set; }
	public int NextEdgeIdx = 0;

	public IdealTargetedCar(Building target, List<Road> path)
	{
		Target = target;
		PlannedPath = path;
		NextEdgeIdx = 0;
		Loop = false;
	}
}
