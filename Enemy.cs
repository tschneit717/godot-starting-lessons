using Godot;
using System;

public partial class Enemy : CharacterBody3D
{
    [Export] public float Speed = 3.5f;
    [Export] public float Damage = 20.0f;
    
    [Export] public float EnemyKnockback = 8.0f;
    [Export] public float EnemyVeriticalPop = 3.0f;
    
    // New: Adjustable stun time
    [Export] public float StunDuration = 0.8f; 

    private Player _target;
    private Area3D _hitbox;
    private bool _isStunned = false; // Prevents movement after a hit
    private float _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    public override void _Ready()
    {
        _target = GetTree().GetFirstNodeInGroup("Player") as Player;
        _hitbox = GetNode<Area3D>("Hitbox");
        
        // We only need the overlapping check in PhysicsProcess for continuous damage,
        // so we can remove the BodyEntered signal connection to keep it clean.
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_target == null) return;

        Vector3 velocity = Velocity;

        // 1. Always apply Gravity
        if (!IsOnFloor())
        {
            velocity.Y -= _gravity * (float)delta;
        }

        // 2. Chase & Attack Logic (Skipped if stunned)
        if (!_isStunned)
        {
            // Calculate direction to player
            Vector3 direction = (_target.GlobalPosition - GlobalPosition).Normalized();

			Vector3 separationForce = Vector3.Zero;
			var overlappingBodies = _hitbox.GetOverlappingBodies();

			foreach (Node3D body in overlappingBodies)
			{
				// If it's an enemy, we PUSH AWAY
				if (body is Enemy otherEnemy && otherEnemy != this)
				{
					Vector3 diff = GlobalPosition - otherEnemy.GlobalPosition;
					float dist = diff.Length();
					// The closer they are, the harder they push (Inverse Square Law-ish)
					separationForce += diff.Normalized() * (5.0f / Mathf.Max(dist, 0.5f));
				}

				// If it's the player, we ATTACK
				if (body is Player player)
				{
					AttackPlayer(player);
				}
			}

			// Merge the chase direction with the separation force
			// We weight the separation higher to keep them apart
			direction = (direction + separationForce).Normalized();

			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
     
            // Rotation
            Vector3 lookTarget = new Vector3(_target.GlobalPosition.X, GlobalPosition.Y, _target.GlobalPosition.Z);
            if (GlobalPosition.DistanceTo(lookTarget) > 0.1f)
            {
                LookAt(lookTarget, Vector3.Up);
            }
        }
        else
        {
            // If stunned, quickly slide to a halt
            velocity.X = Mathf.MoveToward(velocity.X, 0, Speed * (float)delta * 5.0f);
            velocity.Z = Mathf.MoveToward(velocity.Z, 0, Speed * (float)delta * 5.0f);
        }

        Velocity = velocity;
        MoveAndSlide();
    }

    private async void AttackPlayer(Player player)
    {
        // Only trigger attack if we aren't already recovering from one
       if (_isStunned) return;

		player.TakeDamage(Damage, GlobalPosition, EnemyKnockback, EnemyVeriticalPop);

		// RECOIL: Move the enemy backward slightly when they hit the player
		Vector3 recoilDir = (GlobalPosition - player.GlobalPosition).Normalized();
		recoilDir.Y = 0;
		Velocity = recoilDir * 5.0f; // Bounce the enemy back
		
		_isStunned = true;
		
		// Play with this value: 0.5s is a quick flinch, 1.0s is a heavy stun
		await ToSignal(GetTree().CreateTimer(StunDuration), SceneTreeTimer.SignalName.Timeout);
		
		_isStunned = false;
    }
}