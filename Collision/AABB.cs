using OpenTK.Mathematics;

namespace minecraft.Collision;

public struct AABB
{
    public Vector3 Min;
    public Vector3 Max;

    public AABB(Vector3 center, Vector3 size)
    {
        var halfSize = size / 2f;
        Min = center - halfSize;
        Max = center + halfSize;
    }

    public bool Intersects(AABB other)
    {
        return (Min.X <= other.Max.X && Max.X >= other.Min.X) &&
               (Min.Y <= other.Max.Y && Max.Y >= other.Min.Y) &&
               (Min.Z <= other.Max.Z && Max.Z >= other.Min.Z);
    }
}