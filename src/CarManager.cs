using System;
using System.Linq;
using System.Collections.Generic;
using Godot;
using QuikGraph;
using QuikGraph.Algorithms.ShortestPath;

namespace Trafficinator;

public class CarManager
{
	private IMutableGraph<RoadConnection, Lane> map;
	private List<Building> allBuildings;
	FloydWarshallAllShortestPathAlgorithm<RoadConnection, Lane> shortestPathAlgorithm;

	public CarManager(AdjacencyGraph<RoadConnection, Lane> map, List<Building> allBuildings)
	{
		this.map = map;
		this.allBuildings = allBuildings;

		shortestPathAlgorithm = new FloydWarshallAllShortestPathAlgorithm<RoadConnection, Lane>(
				map,
				road => road.Length
			);
		shortestPathAlgorithm.Compute();
	}

	public bool TryRandomTargetedCar(RoadConnection start, out Car car)
	{
		// // get all buildings with connection from start
		// var allPossibleBuildings = this.allBuildings
		// 	.Where(building => shortestPathAlgorithm.TryGetPath(start, building.AttachedRoad.Source, out _))
		// 	.ToList();
		//
		// GD.Print("All possible buildings: ", allPossibleBuildings.Count, " from ", allBuildings.Count);

		// GD.Print("Random targeted car");
		var CarTarget = allBuildings[new Random().Next(allBuildings.Count)];


		var targetConneciton = CarTarget.AttachedRoad.Source;
		
		var pathExists = shortestPathAlgorithm.TryGetPath(start, targetConneciton, out var path);

		// fallback for two way road
		if (!pathExists && !CarTarget.AttachedRoad.IsDirected)
		{
			targetConneciton = CarTarget.AttachedRoad.Target;
			pathExists = shortestPathAlgorithm.TryGetPath(
					start,
					targetConneciton,
					out path
			);
		}

		if (!pathExists)
		{
			car = null;
			return false;
		}

		car = new IdealTargetedCar(CarTarget, path.Select(lane => lane.Road).ToList()) {
			NextEdgeIdx = 0
		};

		return true;
	}

	public Car randomCar(RoadConnection start)
	{
		return new RandomCar();
	}
}
