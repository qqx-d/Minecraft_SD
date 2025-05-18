using minecraft.Collision;
using OpenTK.Mathematics;

namespace minecraft.Entities;

public abstract class Entity
{
    public Vector3 Position = Vector3.Zero;
    public Vector3 Velocity = Vector3.Zero;
    
    public bool IsGrounded;
    public bool IsInWater;

    public BoxCollider Collider { get; private set; }
    public Physics.Physics Physics { get; private set; }

    protected Entity(Vector3 size)
    {
        Collider = new BoxCollider(size);
        Physics = new Physics.Physics(this);
        
        EntityProcessor.AddEntity(this);
    }
    
    public virtual void Update(float deltaTime) { }

    protected void MoveHorizontal(Vector3 direction, float speed, float dt)
    {
        var move = direction * speed * dt;
        Physics.MoveAxis(0, move.X, ref Position, ref Velocity, Collider, ref IsGrounded);
        Physics.MoveAxis(2, move.Z, ref Position, ref Velocity, Collider, ref IsGrounded);
    }

    protected void MoveVertical(Vector3 direction, float speed, float dt)
    {
        var move = direction * speed * dt;
        Physics.MoveAxis(1, move.Y, ref Position, ref Velocity, Collider, ref IsGrounded);
    }

    protected void Jump(float force)
    {
        if (IsGrounded || IsInWater)
            Velocity.Y = force;
    }

    public AABB GetAabb() => new(Position, Collider.BoxSize);
}