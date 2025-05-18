using minecraft.Collision;
using minecraft.Entities;
using minecraft.Sys;
using minecraft.World;
using OpenTK.Mathematics;

namespace minecraft.Physics;

public class Physics
{
    private static readonly List<Entity> Entities = [];
    public static float Gravity { get; private set; } = -12.8f;
    public static float GravityOnWater { get; private set; } = -135f;

    private static readonly Vector3 WaterOffset = new(0f, 0.5f, 0f);
    
    public Physics(Entity entity)
    {
        if(!Entities.Contains(entity))
            Entities.Add(entity);
    }

    public static void ApplyForceToEntities()
    {
        for(var i = 0; i < Entities.Count; i++)
        {
            var entity = Entities[i];

            var position = Vector3Int.RoundToInt(entity.Position - WaterOffset);
            
            entity.IsInWater = WorldGenerator.Instance.TryGetBlockGlobal(position, out var block) && block.Id == 6;

            // gravity
            entity.IsGrounded = false;
            if(!entity.IsInWater)
                entity.Velocity.Y += Gravity * Time.DeltaTime;
            else
                entity.Velocity.Y = GravityOnWater * Time.DeltaTime;
            
            entity.Physics.MoveAxis(1, entity.Velocity.Y * Time.DeltaTime, ref entity.Position, ref entity.Velocity, entity.Collider, ref entity.IsGrounded);
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
            var testBox = collider.GetAabb(newPos);

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