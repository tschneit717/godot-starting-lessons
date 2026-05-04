using Godot;
using System; 

public partial class Player : CharacterBody3D
{
    [Export] public float Speed = 5.0f;
    [Export] public float RotationSpeed = 10.0f;
    [Export] public float JumpVelocity = 4.5f; // Added this back for your jump!

	[Export] public float Acceleration = 10.0f; // Added missing Acceleration

    public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    public override void _Ready()
    {
    }
	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// 1. Add Gravity
		if (!IsOnFloor())
		{
			velocity.Y -= gravity * (float)delta;
		}
		else if (velocity.Y < 0)
		{
			velocity.Y = 0; // Reset downward velocity when on floor
		}

		// 2. Handle Jump
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		// 3. Read input and create a relative direction
		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
		
		// We explicitly calculate direction relative to the camera or the global view, 
		// avoiding basis transformations inside the moving body itself.
		Vector3 direction = new Vector3(inputDir.X, 0, inputDir.Y).Normalized();

		if (direction != Vector3.Zero)
		{
			// Smooth out the velocity buildup rather than instantly snapping
			Vector3 targetVelocity = direction * Speed;
			velocity.X = Mathf.MoveToward(velocity.X, targetVelocity.X, Acceleration * (float)delta);
			velocity.Z = Mathf.MoveToward(velocity.Z, targetVelocity.Z, Acceleration * (float)delta);

			// Calculate rotation
			float targetAngle = Mathf.Atan2(direction.X, direction.Z);
			Basis targetBasis = Basis.Identity.Rotated(Vector3.Up, targetAngle);
			Basis currentBasis = Basis;
			Basis newBasis = currentBasis.Slerp(targetBasis, RotationSpeed * (float)delta);
			Basis = newBasis;
		}
		else
		{
			// Friction / stop
			velocity.X = Mathf.MoveToward(velocity.X, 0, Acceleration * (float)delta);
			velocity.Z = Mathf.MoveToward(velocity.Z, 0, Acceleration * (float)delta);
		}

		Velocity = velocity;
		MoveAndSlide(); // Godot 4: MoveAndSlide() uses the internal Velocity property
	}
}