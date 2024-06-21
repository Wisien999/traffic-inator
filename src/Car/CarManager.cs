using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Godot;
using QuikGraph;
using QuikGraph.Algorithms.ShortestPath;
using Microsoft.Extensions.Caching.Memory;

namespace Trafficinator;

public class CarManager
{
	private IMutableGraph<RoadConnection, Lane> map;
	private List<Building> allBuildings;
	FloydWarshallAllShortestPathAlgorithm<RoadConnection, Lane> shortestPathAlgorithm;
	private MemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
	private Dictionary<Road, List<Building>> buildingsOnRoad = new Dictionary<Road, List<Building>>();
	private int _carCount = 0;
	public int CarCount { get => _carCount; }

	public CarManager(AdjacencyGraph<RoadConnection, Lane> map, List<Building> allBuildings)
	{
		var cacheOptions = new MemoryCacheOptions();
		cacheOptions.SizeLimit = 1000;
		memoryCache = new MemoryCache(cacheOptions);
		this.map = map;
		this.allBuildings = allBuildings;

		shortestPathAlgorithm = new FloydWarshallAllShortestPathAlgorithm<RoadConnection, Lane>(
				map,
				lane => lane.Length
			);
		shortestPathAlgorithm.Compute();
	}

	private List<Building> getAccessibleBuildingsFor(RoadConnection point)
	{
		return memoryCache.GetOrCreate<List<Building>>(
			point,
			entry =>
			{
				entry.Size = 1;
				return allBuildings.Where(b => shortestPathAlgorithm.TryGetDistance(point, b.AttachedRoad.Source, out _)).ToList();
			}
		);
	}

	public bool TryRandomTargetedCar(RoadConnection start, out Car car)
	{
		car = null;
		var possibleBuildings = getAccessibleBuildingsFor(start);
		// GD.Print("All possible buildings: ", possibleBuildings.Count, " from ", allBuildings.Count);
		if (possibleBuildings.Count == 0) return false;

		var CarTarget = possibleBuildings[new Random().Next(possibleBuildings.Count)];
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
			return false;
		}

		car = new IdealTargetedCar(CarTarget, path.Select(lane => lane.Road).ToList())
		{
			NextEdgeIdx = 0
		};
		SetupCar(car);

		return true;
	}

	public Car randomCar(RoadConnection start)
	{

		var car = new RandomCar();
		SetupCar(car);

		return car;
	}

	private void SetupCar(Car car)
	{
		car.TreeEntered += () => Interlocked.Increment(ref _carCount);
		car.TreeExited += () => Interlocked.Decrement(ref _carCount);
		car.Speed += (new Random().Next() % 30) - 15;
	}
}
