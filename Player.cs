using Godot;
using System;
using System.Threading.Tasks;

public partial class Player : CharacterBody3D
{
    [Export] public float MaxHealth = 100.0f;
    public float CurrentHealth;
    [Export] public float BaseSpeed = 5.0f; 
    [Export] public float Speed = 5.0f;
    [Export] public float RotationSpeed = 10.0f;
    [Export] public float JumpVelocity = 4.5f; 

    [Export] public float Acceleration = 10.0f; 
    
    [ExportGroup("Hazard Settings")]
    [Export] public float KnockbackForce = 15.0f; // Horizontal push
    [Export] public float VerticalForce = 15.0f;  // The "Pop" height
    [Export] public float ImmunityTime = 0.6f;    // Duration of "Mario" jump state

    private bool _isImmune = false;
    [Export] public float MouseSensitivity = 0.002f;
    public bool IsBuffed => Speed > BaseSpeed;
    public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    public override void _Ready()
    {
        CurrentHealth = MaxHealth;
        Input.MouseMode = Input.MouseModeEnum.Captured;
        
        var springArm = GetNode<SpringArm3D>("SpringArm3D");
        springArm.TopLevel = true; 
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;
        var springArm = GetNode<SpringArm3D>("SpringArm3D");

        // 1. Keep the Camera following the Player
        springArm.GlobalPosition = GlobalPosition + new Vector3(0, 1.5f, 0);

        // 2. Gravity
        if (!IsOnFloor()) 
            velocity.Y -= gravity * (float)delta;
        else if (velocity.Y < 0) 
            velocity.Y = 0;

        // 3. Movement Logic
        // THE TRICK: We only allow WASD control if the player is NOT in the knockback/immune state
        if (!_isImmune)
        {
            if (Input.IsActionJustPressed("jump") && IsOnFloor()) 
                velocity.Y = JumpVelocity;

            Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
            Vector3 forward = -springArm.GlobalTransform.Basis.Z;
            Vector3 right = springArm.GlobalTransform.Basis.X;
            forward.Y = 0;
            right.Y = 0;
            forward = forward.Normalized();
            right = right.Normalized();

            Vector3 direction = (forward * -inputDir.Y + right * inputDir.X).Normalized();

            if (direction != Vector3.Zero)
            {
                velocity.X = Mathf.MoveToward(velocity.X, direction.X * Speed, Acceleration * (float)delta);
                velocity.Z = Mathf.MoveToward(velocity.Z, direction.Z * Speed, Acceleration * (float)delta);

                float targetAngle = Mathf.Atan2(direction.X, direction.Z);
                Basis targetBasis = Basis.Identity.Rotated(Vector3.Up, targetAngle);
                Basis = Basis.Slerp(targetBasis, RotationSpeed * (float)delta);
            }
            else
            {
                velocity.X = Mathf.MoveToward(velocity.X, 0, Acceleration * (float)delta);
                velocity.Z = Mathf.MoveToward(velocity.Z, 0, Acceleration * (float)delta);
            }
        }
        else
        {
            // While immune (flying from lava), we allow physics to carry the velocity 
            // but we add a tiny bit of air friction so it's not a rigid slide
           	float airResistance = 12.0f; 
			velocity.X = Mathf.MoveToward(velocity.X, 0, airResistance * (float)delta);
			velocity.Z = Mathf.MoveToward(velocity.Z, 0, airResistance * (float)delta);
		}        

        Velocity = velocity;
        MoveAndSlide();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            var springArm = GetNode<SpringArm3D>("SpringArm3D");
            springArm.RotateY(-mouseMotion.Relative.X * MouseSensitivity);
            
            float newRotationX = springArm.Rotation.X - mouseMotion.Relative.Y * MouseSensitivity;
            newRotationX = Mathf.Clamp(newRotationX, Mathf.DegToRad(-75), Mathf.DegToRad(75));
            springArm.Rotation = new Vector3(newRotationX, springArm.Rotation.Y, 0);
        }

        if (@event.IsActionPressed("ui_cancel"))
            Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    public async Task ApplySpeedBuff(float amount, float duration)
    {
        Speed += amount;
        await ToSignal(GetTree().CreateTimer(duration), SceneTreeTimer.SignalName.Timeout);
        Speed -= amount;
    }

    private void Die()
    {
        CurrentHealth = MaxHealth;
        GetTree().CallDeferred(SceneTree.MethodName.ReloadCurrentScene);
    }

    public async Task TakeDamage(float amount, Vector3 hazardPosition)
    {
        if (_isImmune) return;

        _isImmune = true;
        CurrentHealth -= amount;
        GD.Print($"Ouch! Health: {CurrentHealth}");

        // 1. Calculate Horizontal Launch Direction (Away from lava center)
        Vector3 flatHazardPos = new Vector3(hazardPosition.X, GlobalPosition.Y, hazardPosition.Z);
       	Vector3 pushDir = -Velocity.Normalized(); // Pushes exactly opposite of your current movement
		if (pushDir == Vector3.Zero) pushDir = -Basis.Z; // Default to 'backwards' if standing still
        // 2. Set the Launch Velocity 
        Vector3 launchVelocity = pushDir * KnockbackForce;
        launchVelocity.Y = VerticalForce; 
        Velocity = launchVelocity;

        if (CurrentHealth <= 0)
        {
            Die();
            return; // Exit early if dead
        }

        // 3. Visual Feedback (Red Flash)
        var mesh = GetNode<MeshInstance3D>("MeshInstance3D");
        var material = mesh.GetActiveMaterial(0) as StandardMaterial3D;

        if (material != null)
        {
            Color originalColor = material.AlbedoColor;
            material.AlbedoColor = Colors.Red;
            
            // Wait for immunity to wear off
            await ToSignal(GetTree().CreateTimer(ImmunityTime), SceneTreeTimer.SignalName.Timeout);
            
            material.AlbedoColor = originalColor;
        }
        else
        {
            await ToSignal(GetTree().CreateTimer(ImmunityTime), SceneTreeTimer.SignalName.Timeout);
        }

        _isImmune = false; 
    }
}