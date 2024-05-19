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

public static (Godot.Node2D, AdjacencyGraph<RoadConnection, Road>, List<Building>) Parse(FileInfo file, float scaleX, float scaleY)
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
        .SelectMany(way => new[] { way.Nodes[0], way.Nodes[^1] })
        .DistinctBy(node => node.Id)
        .ToImmutableDictionary(node => node.Id, node => new Roundabout() { Position = Node2Pos(node, centerX, centerY, scaleX, scaleY) });
    var ways = filtered
        .Where(geo => geo.Type == OsmGeoType.Way)
        .Cast<CompleteWay>()
        .Where(way => way.Nodes[0].Id != null && way.Nodes[^1].Id != null);
    var roads = new List<Road>();
    var roadsByName = new Dictionary<String, List<Road>>();
    foreach (var way in ways)
    {
        var start = nodes[way.Nodes[0].Id];
        var end = nodes[way.Nodes[^1].Id];
        var curve = new Godot.Curve2D();
        foreach (var node in way.Nodes)
        {
            curve.AddPoint(Node2Pos(node, centerX, centerY, scaleX, scaleY));
        }

        var singleWay = way.Tags.Contains("oneway", "yes");
        Road road;
        if (singleWay)
        {
            road = new SingleWayRoad() { Source = start, Target = end, Curve = curve };
            roads.Add(road);
        }
        else
        {
            road = new TwoWayRoad() { Source = start, Target = end, Curve = curve };
            roads.Add(road);
        }
        var roadName = way.Tags.GetValue("name");
        if (roadName != null)
        {
            if (!roadsByName.ContainsKey(roadName))
            {
                roadsByName.Add(roadName, new List<Road>());
            }
            roadsByName[roadName].Append(road);
        }
    }
    var buildingFiltered = src.Where(BuildingFilter).ToComplete().ToList();
    var buildings = buildingFiltered
      .Where(geo => geo.Type == OsmGeoType.Way)
      .Cast<CompleteWay>()
      .ToList();

    Godot.GD.Print("Parsed file ", file.FullName, " created ", nodes.Count, " Points ", roads.Count, " roads and ", buildings.Count, " buildings");
    var buildingTypes = buildings.Select(way => way.Tags.GetValue("building")).ToHashSet();

    foreach (var buildingType in buildingTypes)
    {
        Godot.GD.Print(buildingType);
    }

    var root = new Godot.Node2D();
    foreach (var road in roads)
    {
        root.AddChild(road);
    }
    foreach (var node in nodes.Values)
    {
        root.AddChild(node);
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
            AttachedRoad = road,
            OutlineColor =  BuildingColors[building.Tags.GetValue("building")]
        };
        root.AddChild(buildingObject);
        buildingObjects.Append(buildingObject);
    }
    var graph = new AdjacencyGraph<RoadConnection, Road>();
    graph.AddVertexRange(nodes.Values);
    graph.AddEdgeRange(roads);

    return (root, graph, buildingObjects);

}
private static Road ClosestRoad(List<Road> roads, Godot.Vector2 point) =>
  roads.MaxBy<Road, float>(road => (road.Curve.GetClosestPoint(point) - point).Length());



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
