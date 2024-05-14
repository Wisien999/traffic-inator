using System;
using System.Linq;
using System.Collections.Generic;
using QuikGraph;
using QuikGraph.Algorithms.ShortestPath;

namespace Trafficinator;

public class CarManager
{
	private IMutableGraph<RoadConnection, Road> map;
	private List<Building> allBuildings;
	FloydWarshallAllShortestPathAlgorithm<RoadConnection, Road> shortestPathAlgorithm;

	CarManager(AdjacencyGraph<RoadConnection, Road> map, List<Building> allBuildings)
	{
		this.map = map;
		this.allBuildings = allBuildings;

		shortestPathAlgorithm = new FloydWarshallAllShortestPathAlgorithm<RoadConnection, Road>(
				map, 
				road => road.Length
			);
		shortestPathAlgorithm.Compute();
	}
	
	public Car randomTargetedCar(RoadConnection start) {
		var CarTarget = allBuildings[new Random().Next(allBuildings.Count)];

		// FIXME: make it work for two way road
		var targetConneciton = CarTarget.AttachedRoad.Source;

		var pathExists = shortestPathAlgorithm.TryGetPath(start, targetConneciton, out var path);
		if (!pathExists) {
			throw new Exception("No path found");
		}

		var car = new IdealTargetedCar(CarTarget, path.ToList());

		return car;
	}

	public Car randomCar(RoadConnection start) {
		return new RandomCar();
	}
}
