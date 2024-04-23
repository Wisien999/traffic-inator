using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using OsmSharp;
using OsmSharp.Complete;
using OsmSharp.Streams;

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

	public static Godot.Node2D Parse(FileInfo file, float scaleX, float scaleY)
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
			.ToImmutableDictionary(node => node.Id, node => new Roundabout() { Position =  Node2Pos(node, centerX, centerY, scaleX, scaleY)});
		var ways = filtered
			.Where(geo => geo.Type == OsmGeoType.Way)
			.Cast<CompleteWay>()
			.Where(way => way.Nodes[0].Id != null && way.Nodes[^1].Id != null);
		var roads = new List<Road>();
		foreach (var way in ways)
		{
			var start = nodes[way.Nodes[0].Id];
			var end = nodes[way.Nodes[^1].Id];
			var curve = new Godot.Curve2D();
			foreach(var node in way.Nodes) {
				curve.AddPoint(Node2Pos(node, centerX, centerY, scaleX, scaleY));
			}

			var singleWay = way.Tags.Contains("oneway", "yes");
			if (singleWay) {
			  var road = new SingleWayRoad() { Source = start, Target = end, Curve = curve};
			  roads.Add(road);
			} else {
			  var road = new TwoWayRoad() {Source = start, Target = end, Curve = curve};  
			  roads.Add(road);
			}
		}
		Godot.GD.Print("Parsed file ", file.FullName, " created ", nodes.Count, " Points and ", roads.Count, " roads");
		var root = new Godot.Node2D();
		foreach(var road in roads) {
		  root.AddChild(road);
		}
		foreach(var node in nodes.Values) {
		  root.AddChild(node);
		}
		return root;

	}

	private static Godot.Vector2 Node2Pos(Node node, double centerX, double centerY, float scaleX, float scaleY) => 
	  new Godot.Vector2{X = (float)(node.Latitude - centerX) * scaleX, Y = (float)(node.Longitude - centerY) * scaleY };

	private static bool WayFilter(OsmGeo osmGeo) =>
		osmGeo.Type == OsmGeoType.Node ||
		osmGeo.Type == OsmGeoType.Way && osmGeo.Tags != null &&
		WayAllowedTypes.Contains(osmGeo.Tags.GetValue("highway"));

}
