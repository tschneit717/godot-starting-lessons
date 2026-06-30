using Godot;
using System;
using System.ComponentModel;

public partial class WaveSpawner : Node3D
{
[Export] public PackedScene EnemyScene;
[Export] public float SpawnRadiusMin = 15.0f;
[Export] public float SpawnRadiusMax = 25.0f;
[Export] public float TimeBetweenWaves = 5.0f;
[Export] public int EnemiesPerWave = 3;

private Player _player;
private float _spawnTimer = 0.0f; 

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// fiund the pllayer using your established group ID
		_player = GetTree().GetFirstNodeInGroup("Player") as Player;
		_spawnTimer = TimeBetweenWaves; // Start ready to spawn
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_player == null || EnemyScene == null) return;

		_spawnTimer -= (float)delta;
		if (_spawnTimer <= 0.0f)
		{
			SpawnWave();
			_spawnTimer = TimeBetweenWaves;
		}
	}

	private void SpawnWave()
	{
		for (int i = 0; i < EnemiesPerWave; i++)
		{
			SpawnEnemy();
		}
	}

	private void SpawnEnemy()
	{
		float randomAngle = (float)GD.RandRange(0.0, Mathf.Tau);

		float randomDistance = (float)GD.RandRange(SpawnRadiusMin, SpawnRadiusMax);

		Vector3 offset = new Vector3(
			Mathf.Cos(randomAngle) * randomDistance,
			0,
			Mathf.Sin(randomAngle) * randomDistance
		);
		
		Vector3 spawnPos = _player.GlobalPosition + offset;

		Enemy enemyInstance = EnemyScene.Instantiate<Enemy>();
		GetTree().CurrentScene.AddChild(enemyInstance);

		enemyInstance.GlobalPosition = spawnPos;
	}
}
