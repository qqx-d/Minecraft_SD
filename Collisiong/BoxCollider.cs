using OpenTK.Mathematics;

namespace minecraft;

public class BoxCollider
{
    public Vector3 BoxSize { get; private set; }

    public BoxCollider(Vector3 size)
    {
        BoxSize = size;
    }

    public AABB GetAABB(Vector3 center)
    {
        return new AABB(center, BoxSize);
    }
}