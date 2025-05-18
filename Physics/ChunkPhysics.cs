using minecraft.Collision;
using minecraft.Sys;
using minecraft.World;
using OpenTK.Mathematics;

namespace minecraft.Physics;

public static class ChunkPhysics
{
    public static bool CheckCollision(AABB box)
    {
        var min = box.Min;
        var max = box.Max;

        var startX = (int)MathF.Floor(min.X);
        var endX   = (int)MathF.Ceiling(max.X - 0.0001f);

        var startY = (int)MathF.Floor(min.Y);
        var endY   = (int)MathF.Ceiling(max.Y - 0.0001f);

        var startZ = (int)MathF.Floor(min.Z);
        var endZ   = (int)MathF.Ceiling(max.Z - 0.0001f);


        for (var x = startX; x <= endX; x++)
        for (var y = startY; y <= endY; y++)
        for (var z = startZ; z <= endZ; z++)
        {
            WorldGenerator.Instance.TryGetBlockGlobal(new Vector3Int(x, y, z), out var block);
            
            if(block == null || block.IsPassable()) continue;
            
            var blockBox = new AABB(new Vector3(x, y + 0.5f, z), Vector3.One);
            
            if (box.Intersects(blockBox)) return true;
        }

        return false;
    }
}