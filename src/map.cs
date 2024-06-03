using System.Collections.Generic;
using Godot;
using QuikGraph;
using QuikGraph.Algorithms.ShortestPath;

namespace Trafficinator;

public partial class map : Node2D
{
	Node2D _root;
	AdjacencyGraph<RoadConnection, Lane> graph;
	private GlobalMapData GlobalMapData => GetNode<GlobalMapData>("/root/GlobalMapData");
	public override void _Ready()
	{
		this.Name = "Map";
		using var mapResource = FileAccess.Open("res://map.osm", FileAccess.ModeFlags.Read);
		var path = mapResource.GetPathAbsolute();
		var (root, graph, buildings) = OsmReader.Parse(new System.IO.FileInfo(path), 50000, 50000);
		// root.Position = new Vector2(750,500);
		// root.Scale = new Vector2(0.75f,0.75f);
		root.RotationDegrees = 90;
		AddChild(root);

		_root = root;
		this.graph = graph;
		GlobalMapData.CarManager = new CarManager(graph, buildings);

	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		// var tmp = (float) (0.1 * delta);
		// _root.Scale -= new Vector2(tmp, tmp);

	}

	public override void _Draw()
	{
		GD.Print("Drawing");
	}
}
