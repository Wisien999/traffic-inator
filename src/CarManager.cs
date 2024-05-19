using System;
using System.Linq;
using System.Collections.Generic;
using Godot;
using QuikGraph;
using QuikGraph.Algorithms.ShortestPath;

namespace Trafficinator;

public class CarManager
{
    private IMutableGraph<RoadConnection, Road> map;
    private List<Building> allBuildings;
    FloydWarshallAllShortestPathAlgorithm<RoadConnection, Road> shortestPathAlgorithm;

    public CarManager(AdjacencyGraph<RoadConnection, Road> map, List<Building> allBuildings)
    {
        this.map = map;
        this.allBuildings = allBuildings;

        shortestPathAlgorithm = new FloydWarshallAllShortestPathAlgorithm<RoadConnection, Road>(
                map,
                road => road.Length
            );
        shortestPathAlgorithm.Compute();
    }

    public Car randomTargetedCar(RoadConnection start)
    {
        // GD.Print("Random targeted car");
        var CarTarget = allBuildings[new Random().Next(allBuildings.Count)];

        // FIXME: make it work for two way road
        var targetConneciton = CarTarget.AttachedRoad.Source;
        // GD.Print("Target connection: ", targetConneciton.Name);

        var pathExists = shortestPathAlgorithm.TryGetPath(start, targetConneciton, out var path);
        if (!pathExists)
        {
            GD.Print("No path found");
            throw new Exception("No path found");
        }

        // GD.Print("Path found: ");
        // foreach (var road in path) {
        // GD.Print("Road: ", road.Name);
        // }
        // GD.Print("");

        var car = new IdealTargetedCar(CarTarget, path.ToList());
        car.NextEdgeIdx = 0;

        return car;
    }

    public Car randomCar(RoadConnection start)
    {
        return new RandomCar();
    }
}
