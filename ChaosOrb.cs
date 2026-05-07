using Godot;
using System;

public partial class ChaosOrb : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
{
        // We find the Area3D child and connect its signal manually
        var area = GetNode<Area3D>("Area3D"); 
        area.BodyEntered += OnBodyEntered; // This is the C# way to connect signals
        GD.Print("Orb is ready and listening...");
    }
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnBodyEntered(Node3D body)
	{
    // This will show up in the "Output" console at the bottom of Godot
    GD.Print($"Something entered the orb: {body.Name}");

    if (body is Player player)
    {
        GD.Print("It was the player! Increasing chaos...");
        var global = GetNode<GameManager>("/root/GameManager");
        global.AddChaos(10);
        player.ApplySpeedBuff(2.0f, 15.0f);
        QueueFree(); // This deletes the orb
    }
}
}
