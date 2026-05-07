using Godot;
using System;

public partial class GameManager : Node
{

	public int ChaosPoints = 0;
	public float GlobalMultiplier = 1.0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void AddChaos(int amount)
	{
		ChaosPoints += amount;
		GD.Print($"Global Chaos: {ChaosPoints}");
	}
}
