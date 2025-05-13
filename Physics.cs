using OpenTK.Mathematics;

namespace minecraft;

public class Physics
{
    private static readonly List<Entity> Entities = [];
    public static float Gravity { get; private set; } = -12.8f;

    public Physics(Entity entity)
    {
        if(!Entities.Contains(entity))
            Entities.Add(entity);
    }

    public static void ApplyForceToEntities(float deltaTime)
    {
        for(var i = 0; i < Entities.Count; i++)
        {
            var entity = Entities[i];
            
            // gravity
            entity.IsGrounded = false;
            entity.Velocity.Y += Gravity * deltaTime;
            
            entity.Physics.MoveAxis(1, entity.Velocity.Y * deltaTime, ref entity.Position, ref entity.Velocity, entity.Collider, ref entity.IsGrounded);
            
        }
    }

    public void MoveAxis(int axis, float amount, ref Vector3 position, ref Vector3 velocity, BoxCollider collider, ref bool isGrounded)
    {
        var stepSize = 0.05f;
        var remaining = amount;

        while (MathF.Abs(remaining) > 0.0001f)
        {
            var step = Math.Clamp(remaining, -stepSize, stepSize);
            var tryOffset = axis switch
            {
                0 => new Vector3(step, 0, 0),
                1 => new Vector3(0, step, 0),
                2 => new Vector3(0, 0, step),
                _ => Vector3.Zero
            };

            var newPos = position + tryOffset;
            var testBox = collider.GetAABB(newPos);

            if (!ChunkPhysics.CheckCollision(testBox))
            {
                position = newPos;
                remaining -= step;
            }
            else
            {
                switch (axis)
                {
                    case 0:
                        velocity.X = 0;
                        break;
                    case 1:
                    {
                        velocity.Y = 0;
                        if (step < 0) isGrounded = true;
                        break;
                    }
                    case 2:
                        velocity.Z = 0;
                        break;
                }

                break;
            }
        }
    }
}