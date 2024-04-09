using Godot;

namespace Trafficinator;

public partial class Car: PathFollow2D
{
	public Color Color = Colors.Red;

	public Car()
	{
		Loop = false;
	}
	public override void _Ready()
	{
		
	}

	public override void _Draw()
	{
		DrawRect(new Rect2(-10,-2.5f, 20, 5), Colors.Blue);
	}

	public override void _Process(double delta)
	{
		
	}
}
