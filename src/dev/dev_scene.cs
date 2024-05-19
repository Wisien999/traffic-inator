using Godot;
using System;
using System.Collections.Generic;
using QuikGraph;

namespace Trafficinator;

public partial class dev_scene : Node2D
{
    AdjacencyGraph<RoadConnection, Road> graph;
    private GlobalMapData GlobalMapData => GetNode<GlobalMapData>("/root/GlobalMapData");

    public override void _Ready()
    {
        GD.Print("Dev scene ready");
        this.Name = "dev_map";


        this.graph = this.GetGraph();
        var buildings = this.GetBuildings();
        GD.Print("Setting car manager");
        GlobalMapData.CarManager = new CarManager(graph, buildings);
        GD.Print("Car manager set" + GlobalMapData.CarManager);

    }


    public AdjacencyGraph<RoadConnection, Road> GetGraph()
    {
        GD.Print("Getting graph");
        GD.Print("Children: ", GetChildren().Count);
        GD.Print("Filtering connections");
        // get all children of type RoadConnection
        var roadConnections = new List<RoadConnection>();
        foreach (var node in GetChildren())
        {
            if (node is RoadConnection roadConnection)
            {
                roadConnections.Add(roadConnection);
            }
        }

        GD.Print("Filtering roads");
        // get all descendants of type Road
        var roads = new List<Road>();
        foreach (var node in GetChildren())
        {
            if (node is Road road)
            {
                if (!roadConnections.Contains(road.Source) || !roadConnections.Contains(road.Target))
                {
                    GD.Print("Road not connected to road connection");
                    continue;
                }
                roads.Add(road);
            }
        }


        GD.Print("Creating graph");
        var graph = new AdjacencyGraph<RoadConnection, Road>();
        GD.Print("Adding vertices: " + roadConnections.Count);
        graph.AddVertexRange(roadConnections);
        GD.Print("Adding edges: " + roads.Count);
        graph.AddEdgeRange(roads);
        GD.Print("Graph created");

        return graph;
    }

    private List<Building> GetBuildings()
    {
        GD.Print("Getting buildings");
        var buildings = new List<Building>();
        foreach (var node in GetChildren())
        {
            if (node is Building building)
            {
                buildings.Add(building);
            }
        }
        return buildings;
    }
}
