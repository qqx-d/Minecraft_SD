using OpenTK.Mathematics;

namespace minecraft.Collision;

public class BoxCollider(Vector3 size)
{
    public Vector3 BoxSize { get; private set; } = size;

    public AABB GetAabb(Vector3 center)
    {
        return new AABB(center, BoxSize);
    }
}