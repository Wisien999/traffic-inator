using Godot;
using QuikGraph;

namespace Trafficinator;

public partial class map : Node2D
{
	Node2D _root;
	AdjacencyGraph<RoadConnection, Lane> graph;
	private GlobalMapData GlobalMapData => GetNode<GlobalMapData>("/root/GlobalMapData");
	private FileDialog fileDialog;
	public override void _Ready()
	{
		this.Name = "Map";
    fileDialog = new FileDialog() {
      FileMode = FileDialog.FileModeEnum.OpenFile,
      Filters = new string[] {
        "*.osm ; Osm XML Map file",
        "*.pbf ; Osm Protobuf Map file"
      },
      Access = FileDialog.AccessEnum.Filesystem,
      UseNativeDialog = true
    };
    fileDialog.FileSelected += init;
    AddChild(fileDialog);
    fileDialog.Popup();
	}

	public void init(string path) {
		var (root, graph, buildings) = OsmReader.Parse(new System.IO.FileInfo(path), 50000, 50000);

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
