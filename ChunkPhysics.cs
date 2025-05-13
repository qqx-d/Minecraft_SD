using OpenTK.Mathematics;

namespace minecraft;

public static class ChunkPhysics
{
    public static bool CheckCollision(AABB box)
    {
        Vector3 min = box.Min;
        Vector3 max = box.Max;

        // Округляем в мирные координаты
        int startX = (int)MathF.Floor(min.X);
        int endX   = (int)MathF.Ceiling(max.X - 0.0001f);

        int startY = (int)MathF.Floor(min.Y);
        int endY   = (int)MathF.Ceiling(max.Y - 0.0001f);

        int startZ = (int)MathF.Floor(min.Z);
        int endZ   = (int)MathF.Ceiling(max.Z - 0.0001f);


        for (int x = startX; x <= endX; x++)
        for (int y = startY; y <= endY; y++)
        for (int z = startZ; z <= endZ; z++)
        {
            var block = WorldGenerator.Instance.TryGetBlockGlobal(new Vector3Int(x, y, z));
            if (block == null || block.ID == 0) continue;

            var blockBox = new AABB(new Vector3(x, y + 0.5f, z), Vector3.One);
            if (box.Intersects(blockBox)) return true;
        }

        return false;
    }
}