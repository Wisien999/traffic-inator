using Godot;

namespace Trafficinator;

public partial class example_road : Node2D
{
	public override void _Ready()
	{
		//var road = new SingleWayRoad();
		//road.Position = new Vector2(2, 31);
		//var curve = new Curve2D();
		//curve.AddPoint(new Vector2(0, 0));
	  //curve.AddPoint(new Vector2(100, 0));
		//curve.AddPoint(new Vector2(100, 100));
		//road.Curve = curve;


		//AddChild(road);
    using var mapResource = FileAccess.Open("res://map.osm", FileAccess.ModeFlags.Read);
    var path = mapResource.GetPathAbsolute();
    var root = OsmReader.Parse(new System.IO.FileInfo(path), 50000, 50000);
    root.Position = new Vector2(750,500);
    root.Scale = new Vector2(0.75f,0.75f);
    root.RotationDegrees = 90;
    AddChild(root);
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
		
	public override void _Draw()
	{
		GD.Print("Drawing");
	}
}
