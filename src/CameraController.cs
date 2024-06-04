using Godot;
using System;

public partial class CameraController : Camera2D
{
    private float _zoomStep = 0.1f;
    private float _minZoom = 0.5f;
    private float _maxZoom = 3f;
    private Vector2 _dragOrigin;
    private bool _isDragging = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        // float floatDelta = (float)delta;
        if (Input.IsActionJustPressed("ui_zoom_in"))
        {
            Zoom -= new Vector2(_zoomStep, _zoomStep);
        }
        else if (Input.IsActionJustPressed("ui_zoom_out"))
        {
            Zoom += new Vector2(_zoomStep, _zoomStep);
        }

        // Clamp the zoom value
        Zoom = new Vector2(Mathf.Clamp(Zoom.X, _minZoom, _maxZoom), Mathf.Clamp(Zoom.Y, _minZoom, _maxZoom));

        // Check if the middle mouse button is pressed for panning
        if (Input.IsActionPressed("ui_pan"))
        {
            if (!_isDragging)
            {
                _dragOrigin = GetLocalMousePosition();
                _isDragging = true;
            }
            else
            {
                Vector2 mousePos = GetLocalMousePosition();
                Vector2 offset = (_dragOrigin - mousePos);
                Position += offset;
                _dragOrigin = mousePos;
            }
        }
        else
        {
            _isDragging = false;
        }
        if (Input.IsActionJustPressed("ui_home"))
        {
            Position = Vector2.Zero;
        }
    }
}
