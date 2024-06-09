using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using OsmSharp;
using OsmSharp.Complete;
using OsmSharp.Streams;
using QuikGraph;

namespace Trafficinator;

public class OsmReader
{
    public static readonly HashSet<string> WayAllowedTypes = new()
    {
        "residential",
        "tertiary",
        "secondary",
        "primary",
        "tertiary_link",
        "secondary_link",
        "primary_link",
        "motorway",
        "trunk",
        "unclassified",
        "motorway_link",
        "trunk_link",
        "living_street",
        "road"
    };
    public static readonly HashSet<string> BuildingAllowedTypes = new()
    {
        "yes",
        "university",
        "school",
        "residential",
        "apartments",
        "dormitory",
        "commercial",
        "industrial",
        "civic",
		// "roof",
		"retail",
        "sports_centre",
        "service",
        "library",
        "construction",
        "college",
        "warehouse",
        "detached",
        "house",
        "semidetached_house",
		// "terrace",
		"hotel",
        "garage",
        "hut",
		// "shed",
		"office",
        "government",
        "garages",
        "outbuilding",
		// "trash_shed",
		// "toilets",
		// "bridge",
		// "guardhouse",
		"police",
        "kiosk",
        "hospital",
        "parking_entrance",
		// "passage",
		// "exhibition",
		// "garbage_shed"
	};
    public static readonly Dictionary<String, Godot.Color> BuildingColors = new()
    {
    { "yes", Godot.Colors.Olive },
      { "university", Godot.Colors.Beige },
    { "school", Godot.Colors.Beige },
    { "residential", Godot.Colors.Aqua },
    { "apartments", Godot.Colors.Aqua },
    { "dormitory", Godot.Colors.Aqua },
    { "commercial", Godot.Colors.SkyBlue },
    { "industrial", Godot.Colors.Brown },
    { "civic", Godot.Colors.SeaGreen },
    { "retail", Godot.Colors.SkyBlue },
    { "sports_centre", Godot.Colors.Lime },
    { "service", Godot.Colors.SkyBlue },
    { "library", Godot.Colors.Aqua },
    { "construction", Godot.Colors.Brown },
    { "college", Godot.Colors.Aqua },
    { "warehouse", Godot.Colors.Brown },
    { "detached", Godot.Colors.Yellow },
    { "house", Godot.Colors.Yellow },
    { "semidetached_house", Godot.Colors.Yellow },
    { "hotel", Godot.Colors.SkyBlue },
    { "garage", Godot.Colors.Yellow },
    { "hut", Godot.Colors.Yellow },
    { "office", Godot.Colors.SkyBlue },
    { "government", Godot.Colors.SeaGreen },
    { "garages", Godot.Colors.Olive },
    { "outbuilding", Godot.Colors.Olive },
    { "police", Godot.Colors.SeaGreen },
    { "kiosk", Godot.Colors.SkyBlue },
    { "hospital", Godot.Colors.SeaGreen },
    { "parking_entrance", Godot.Colors.Olive }
    };

    public static (Godot.Node2D, AdjacencyGraph<RoadConnection, Lane>, List<Building>) Parse(FileInfo file, float scaleX, float scaleY)
    {
        using var fileStream = file.OpenRead();
        OsmStreamSource src = file.Extension switch
        {
            ".pbf" => new PBFOsmStreamSource(fileStream),
            ".osm" => new XmlOsmStreamSource(fileStream),
            _ => throw new ArgumentException("the file has unknown extension"),
        };
        var filtered = src.Where(WayFilter).ToComplete().ToList();

        var centerX = filtered.Where(geo => geo.Type == OsmGeoType.Node)
            .Cast<Node>()
            .Select(node => node.Latitude)
            .Average()
            .GetValueOrDefault(0);
        var centerY = filtered.Where(geo => geo.Type == OsmGeoType.Node)
            .Cast<Node>()
            .Select(node => node.Longitude)
            .Average()
            .GetValueOrDefault(0);
        var nodes = filtered.Where(geo => geo.Type == OsmGeoType.Way)
            .Cast<CompleteWay>()
            .SelectMany(way => way.Nodes)
            .DistinctBy(node => node.Id)
            .ToDictionary(node => node.Id, node => (
              Position: Node2Pos(node, centerX, centerY, scaleX, scaleY),
              OnewayInRoads: new HashSet<int>(),
              OnewayOutRoads: new HashSet<int>(),
              TwoWayRoads: new HashSet<int>()
              ));
        var ways = filtered
             .Where(geo => geo.Type == OsmGeoType.Way)
             .Cast<CompleteWay>()
             .Where(way => way.Nodes[0].Id != null && way.Nodes[^1].Id != null)
             .SelectMany(SplitWay)
             .Select((arg, index) => (
               Oneway: arg.way.Tags.Contains("oneway", "yes"),
               StartId: arg.start.Id,
               EndId: arg.end.Id,
               Points: (new []{arg.start, arg.end}).Select(node => Node2Pos(node, centerX, centerY, scaleX, scaleY)).ToList(),
               Names: new HashSet<string> { arg.way.Tags.GetValue("name") },
               Ids: new HashSet<int> { index },
               MinId: index)
         )
         .ToList();
        foreach (var way in ways)
        {
            var index = way.Ids.First();
            if (way.Oneway)
            {
                nodes[way.StartId].OnewayOutRoads.Add(index);
                nodes[way.EndId].OnewayInRoads.Add(index);
            }
            else
            {
                nodes[way.StartId].TwoWayRoads.Add(index);
                nodes[way.EndId].TwoWayRoads.Add(index);
            }
        }
        var nodeIdsToRemove = new List<long?>();
        foreach (var node in nodes)
        {
            var onewayInCount = node.Value.OnewayInRoads.Count;
            var onewayOutCount = node.Value.OnewayOutRoads.Count;
            var twowayCount = node.Value.TwoWayRoads.Count;
            if (twowayCount == 0 && onewayInCount == 1 && onewayOutCount == 1)
            {
                var roadIn = ways[node.Value.OnewayInRoads.First()];
                var roadOut = ways[node.Value.OnewayOutRoads.First()];

                roadIn.Names.UnionWith(roadOut.Names);
                roadIn.Ids.UnionWith(roadOut.Ids);
                roadIn.EndId = roadOut.EndId;
                roadIn.Points.AddRange(roadOut.Points.Skip(1));
                roadIn.MinId = Math.Min(roadIn.MinId, roadOut.MinId);
                foreach (var id in roadIn.Ids)
                {
                    ways[id] = roadIn;
                }

                nodeIdsToRemove.Add(node.Key);
            }
            else if (twowayCount == 2 && onewayOutCount == 0 && onewayInCount == 0)
            {
                var roadIds = node.Value.TwoWayRoads.ToList();
                var roadDst = ways[roadIds[0]];
                var roadSrc = ways[roadIds[1]];

                roadDst.Names.UnionWith(roadSrc.Names);
                roadDst.Ids.UnionWith(roadSrc.Ids);
                if (roadDst.EndId != node.Key)
                {
                    roadDst.Points.Reverse();
                    (roadDst.StartId, roadDst.EndId) = (roadDst.EndId, roadDst.StartId);
                }
                if (roadSrc.StartId != node.Key)
                {
                    roadSrc.Points.Reverse();
                    (roadSrc.StartId, roadSrc.EndId) = (roadSrc.EndId, roadSrc.StartId);
                }
                roadDst.Points.AddRange(roadSrc.Points.Skip(1));
                roadDst.EndId = roadSrc.EndId;
                roadDst.MinId = Math.Min(roadDst.MinId, roadSrc.MinId);

                foreach (var id in roadDst.Ids)
                {
                    ways[id] = roadDst;
                }

                nodeIdsToRemove.Add(node.Key);
            }
        }
        foreach (var id in nodeIdsToRemove)
        {
            nodes.Remove(id);
        }

        var deduplicatedWays = ways
          .Where((way, idx) => way.MinId == idx)
          .Where(way => nodes.ContainsKey(way.StartId) && nodes.ContainsKey(way.EndId))
          .Where(way => way.StartId != way.EndId)
          .ToList();

        var intersections = nodes.ToImmutableDictionary(kv => kv.Key, kv => new Roundabout { Position = kv.Value.Position });

        var roads = new List<Road>();
        var roadsByName = new Dictionary<String, List<Road>>();
        foreach (var way in deduplicatedWays)
        {
            var start = intersections[way.StartId];
            var end = intersections[way.EndId];
            var curve = new Godot.Curve2D();
            foreach (var point in way.Points)
            {
                curve.AddPoint(point);
            }
            Road road;
            if (way.Oneway)
            {
                road = new SingleWayRoad(curve, start, end);
            }
            else
            {
                road = new TwoWayRoad(curve, start, end);
            }
            roads.Add(road);
            foreach (var roadName in way.Names)
            {
                if (roadName != null)
                {
                    road.Name = "Street" + roadName;
                    if (!roadsByName.ContainsKey(roadName))
                    {
                        roadsByName[roadName] = new List<Road>();
                    }
                    roadsByName[roadName].Add(road);
                }
            }

        }
        var buildingFiltered = src.Where(BuildingFilter).ToComplete().ToList();
        var buildings = buildingFiltered
          .Where(geo => geo.Type == OsmGeoType.Way)
          .Cast<CompleteWay>()
          .ToList();

        Godot.GD.Print("Parsed file ", file.FullName, " created ", intersections.Count, " Points ", roads.Count, " roads and ", buildings.Count, " buildings, removed ", nodeIdsToRemove.Count, " passthrough nodes");
        //    var buildingTypes = buildings.Select(way => way.Tags.GetValue("building")).ToHashSet();

        //    foreach (var buildingType in buildingTypes)
        //    {
        //        Godot.GD.Print(buildingType);
        //    }

        var root = new Godot.Node2D();
        foreach (var road in roads)
        {
            root.AddChild(road);
        }
        foreach (var intersection in intersections.Values)
        {
            root.AddChild(intersection);
        }
        var buildingObjects = new List<Building>();
        foreach (var building in buildings)
        {
            var points = building.Nodes.Select(node => Node2Pos(node, centerX, centerY, scaleX, scaleY)).ToList();
            var buildingCenterX = points.Select(point => point.X).Average();
            var buildingCenterY = points.Select(point => point.Y).Average();
            var buildingPosition = new Godot.Vector2 { X = buildingCenterX, Y = buildingCenterY };
            var translatedPoints = points
              .Select(point => point - buildingPosition)
              .ToArray();
            var buildingAddress = building.Tags.GetValue("addr:street");
            List<Road> potentialRoads;
            if (buildingAddress != null && roadsByName.ContainsKey(buildingAddress))
            {
                potentialRoads = roadsByName[buildingAddress];
            }
            else
            {
                potentialRoads = roads;
            }
            var road = ClosestRoad(potentialRoads, buildingPosition);
            var buildingObject = new Building
            {
                Position = buildingPosition,
                Outline = translatedPoints,
                OutlineColor = BuildingColors[building.Tags.GetValue("building")],

                AttachedRoad = road,
            };
            root.AddChild(buildingObject);
            buildingObjects.Add(buildingObject);
        }
        var graph = new AdjacencyGraph<RoadConnection, Lane>();

        graph.AddVertexRange(intersections.Values);
        graph.AddEdgeRange(roads.SelectMany(road => road.Lanes));

        return (root, graph, buildingObjects);

    }

    private static IEnumerable<(CompleteWay way,Node start,Node end)> SplitWay(CompleteWay way) {
                    int n_subways = way.Nodes.Length - 1;
                    for(int i = 0; i < n_subways; i++) {
                      yield return (way: way, start: way.Nodes[i], end: way.Nodes[i+1]);
                    }
                 }
    private static Road ClosestRoad(List<Road> roads, Godot.Vector2 point) =>
      roads.MinBy<Road, float>(road => (road.Curve.GetClosestPoint(point) - point).Length());



    private static Godot.Vector2 Node2Pos(Node node, double centerX, double centerY, float scaleX, float scaleY) =>
      new Godot.Vector2 { X = (float)(node.Latitude - centerX) * scaleX, Y = (float)(node.Longitude - centerY) * scaleY };

    private static bool WayFilter(OsmGeo osmGeo) =>
        osmGeo.Type == OsmGeoType.Node ||
        osmGeo.Type == OsmGeoType.Way && osmGeo.Tags != null &&
        WayAllowedTypes.Contains(osmGeo.Tags.GetValue("highway"));
    private static bool BuildingFilter(OsmGeo osmGeo) =>
          osmGeo.Type == OsmGeoType.Node ||
          osmGeo.Type == OsmGeoType.Way && osmGeo.Tags != null &&
          BuildingAllowedTypes.Contains(osmGeo.Tags.GetValue("building"));


}
