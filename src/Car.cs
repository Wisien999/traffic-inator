using Godot;

namespace test_godot_game;

public partial class Road
{
    private partial class Car: PathFollow2D
    {
        public Car()
        {
            Loop = false;
        }
        public override void _Ready()
        {
            
        }

        public override void _Draw()
        {
            DrawRect(new Rect2(-10,-2.5f, 20, 5), Colors.Blue);
        }

        public override void _Process(double delta)
        {
            
        }
    }
}