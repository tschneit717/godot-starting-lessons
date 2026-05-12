using Godot;
using System;

public partial class Hud : CanvasLayer
{

	private Label _chaosLabel;
	private Label _speedLabel;
	private ProgressBar _healthBar;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_chaosLabel = GetNode<Label>("Control/MarginContainer/VBoxContainer/ChaosLabel");
		_speedLabel = GetNode<Label>("Control/MarginContainer/VBoxContainer/SpeedLabel");
		_healthBar = GetNode<ProgressBar>("Control/MarginContainer/VBoxContainer/HealthBar");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
		// 1. Get Chaos from GameManager
		var global = GetNode<GameManager>("/root/GameManager");
		_chaosLabel.Text = $"Chaos: {global.ChaosPoints}";

		// Get the first node in the "player" group
		var player = GetTree().GetFirstNodeInGroup("Player") as Player;

		if (player != null)
		{
			_speedLabel.Text = $"Speed: {player.Speed:F1}";
			
			// Use the new property we just made
			_speedLabel.Modulate = player.IsBuffed ? Colors.Gold : Colors.White;

			// ProgressBar uses a 0-100 scale by default
	        _healthBar.Value = player.CurrentHealth;
		}
		else 
		{
			// Debugging: If the speed still isn't showing, this will tell us why
			_speedLabel.Text = "Player not found";
		}
	}
}
