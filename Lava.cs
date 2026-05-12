using Godot;
using System;

public partial class Lava : Area3D
{

	[Export] public float lavaDamage;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node3D body)
	{

		if (!IsInsideTree() || !body.IsInsideTree()) return;

		if (body is Player player)
		{
			GD.Print("Player is in lava! Resetting them....");
			
			player.TakeDamage(lavaDamage, GlobalPosition);

			Vector3 bounceDir = (player.GlobalPosition - GlobalPosition).Normalized();

			player.Velocity += bounceDir * 10.0f;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		if (!IsInsideTree()) return;

		// check all bodies currently overlapping the lava
		foreach (Node3D body in GetOverlappingBodies())
		{
			if (body is Player player)
			{
				player.TakeDamage(lavaDamage, GlobalPosition);
			}
		}
	}
}
