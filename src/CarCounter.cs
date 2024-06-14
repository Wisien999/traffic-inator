using Godot;
using System;

namespace Trafficinator;

public partial class CarCounter : Label
{
	private GlobalMapData GlobalMapData => GetNode<GlobalMapData>("/root/GlobalMapData");

	private Timer _redrawTimer;
	private int Value { get => GlobalMapData.CarManager.CarCount; }

	public override void _Ready()
	{
		_redrawTimer = new Timer() { 
			WaitTime = 1,
			OneShot = false
		};
		_redrawTimer.Connect("timeout", Callable.From(QueueRedraw));
		AddChild(_redrawTimer);
		_redrawTimer.Start();
	}

	public override void _Draw()
	{
		this.Text = Value.ToString();
			
		base._Draw();
	}
}
