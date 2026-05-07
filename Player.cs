using Godot;
using System;
using System.Threading.Tasks;


public partial class Player : CharacterBody3D
{
	[Export] public float BaseSpeed = 5.0f; // Rename your initial speed
    [Export] public float Speed = 5.0f;
    [Export] public float RotationSpeed = 10.0f;
    [Export] public float JumpVelocity = 4.5f; // Added this back for your jump!

	[Export] public float Acceleration = 10.0f; // Added missing Acceleration

	[Export] public float MouseSensitivity = 0.002f;
	public bool IsBuffed => Speed > BaseSpeed;
    public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

  	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		
		// THE SECRET SAUCE: 
		// This tells the SpringArm "Follow my position, but don't spin just because I spin."
		var springArm = GetNode<SpringArm3D>("SpringArm3D");
		springArm.TopLevel = true; 
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;
		var springArm = GetNode<SpringArm3D>("SpringArm3D");

		// 1. Keep the Camera following the Player's position
		springArm.GlobalPosition = GlobalPosition + new Vector3(0, 1.5f, 0);

		// 2. Gravity & Jump (Your existing logic)
		if (!IsOnFloor()) velocity.Y -= gravity * (float)delta;
		else if (velocity.Y < 0) velocity.Y = 0;
		if (Input.IsActionJustPressed("jump") && IsOnFloor()) velocity.Y = JumpVelocity;

		// 3. STABLE Camera-Relative Movement
		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
		
		// We use the SpringArm's rotation, which is now independent of the Player's spin
		Vector3 forward = -springArm.GlobalTransform.Basis.Z;
		Vector3 right = springArm.GlobalTransform.Basis.X;
		forward.Y = 0;
		right.Y = 0;
		forward = forward.Normalized();
		right = right.Normalized();

		// W is -inputDir.Y, so we multiply by -inputDir.Y to move "Forward"
		Vector3 direction = (forward * -inputDir.Y + right * inputDir.X).Normalized();

		if (direction != Vector3.Zero)
		{
			velocity.X = Mathf.MoveToward(velocity.X, direction.X * Speed, Acceleration * (float)delta);
			velocity.Z = Mathf.MoveToward(velocity.Z, direction.Z * Speed, Acceleration * (float)delta);

			// 4. Rotate the character to face movement
			float targetAngle = Mathf.Atan2(direction.X, direction.Z);
			Basis targetBasis = Basis.Identity.Rotated(Vector3.Up, targetAngle);
			Basis = Basis.Slerp(targetBasis, RotationSpeed * (float)delta);
		}
		else
		{
			velocity.X = Mathf.MoveToward(velocity.X, 0, Acceleration * (float)delta);
			velocity.Z = Mathf.MoveToward(velocity.Z, 0, Acceleration * (float)delta);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			var springArm = GetNode<SpringArm3D>("SpringArm3D");

			// Rotate the SpringArm independently
			springArm.RotateY(-mouseMotion.Relative.X * MouseSensitivity);
			
			// Vertical Tilt
			float newRotationX = springArm.Rotation.X - mouseMotion.Relative.Y * MouseSensitivity;
			newRotationX = Mathf.Clamp(newRotationX, Mathf.DegToRad(-75), Mathf.DegToRad(75));
			
			springArm.Rotation = new Vector3(newRotationX, springArm.Rotation.Y, 0);
		}

		
		// Press Escape to get your mouse back
		if (@event.IsActionPressed("ui_cancel"))
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
	}

	public async Task ApplySpeedBuff(float amount, float duration)
	{
		 
		// apply speed
		Speed += amount;
		GD.Print($"Buffed! Speed is now: {Speed}");

		// wait logic
		await ToSignal(GetTree().CreateTimer(duration), SceneTreeTimer.SignalName.Timeout);

		Speed -= amount;
		GD.Print($"Buff expired. Speed returned to: {Speed}");

	}
}