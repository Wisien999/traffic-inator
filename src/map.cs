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

        // GD.Print("is edges empty: " , graph.IsEdgesEmpty, "\tvertices: ", graph.IsVerticesEmpty);
        // var enumerator = graph.Edges.GetEnumerator();
        // enumerator.MoveNext();
        // var randomEdge = enumerator.Current;
        // enumerator.MoveNext();
        // var randomEdge2 = enumerator.Current;
        // GD.Print("Random edge prepare");
        // GD.Print("Random edge: ", randomEdge);
        // GD.Print("Random edge end");
        // var invertedEdge = new Lane() { Road = randomEdge.Road, Source = randomEdge.Target, Target = randomEdge.Source };
        //
        // GD.Print("Compare inverted and 2: ", randomEdge2.Source.Position, " ", invertedEdge.Source.Position, " ", randomEdge2.Target.Position, " ", invertedEdge.Target.Position);
        // GD.Print("Inverted edge: ", invertedEdge);
        // GD.Print("is random in graph: ", graph.ContainsEdge(randomEdge));
        // GD.Print("is inverted in graph: ", graph.ContainsEdge(randomEdge.Target, randomEdge.Source));

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
    }
}
