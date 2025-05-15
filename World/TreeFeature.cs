namespace minecraft.World;

public static class TreeFeature
{
    private static WorldGenerator WorldGenerator => WorldGenerator.Instance;
    
    public static void GenerateTreesInArea(Vector3Int areaOrigin)
    {
        const int step = 4;

        for (var x = 0; x < WorldGenerator.ChunkWidth; x += step)
        for (var z = 0; z < WorldGenerator.ChunkWidth; z += step)
        {
            var worldX = (int)(areaOrigin.X + x);
            var worldZ = (int)(areaOrigin.Z + z);

            var forestNoise = WorldGenerator.ForestNoise.GetNoise(worldX, worldZ);
            if(forestNoise < 0.6f) continue;
            
            var chance = WorldGenerator.FastNoiseLite.GetNoise(worldX + 1000, worldZ + 1000);
            if(chance > 0.85f)
            {
                Vector3Int? ground = FindGrass(worldX, worldZ);
                if (ground.HasValue)
                    PlaceTreeAt(ground.Value + new Vector3Int(0, 1, 0));
            }
        }
    }

    private static Vector3Int? FindGrass(int x, int z)
    {
        for (var y = WorldGenerator.ChunkHeight - 1; y >= 0; y--)
        {
            var pos = new Vector3Int(x, y, z);

            if(WorldGenerator.TryGetBlockGlobal(pos, out var block))
            {
                if (block.Id == 1) return pos;
            }
        }
        return null;
    }

    private static void PlaceTreeAt(Vector3Int basePos)
    {
        for (var i = 0; i < 4; i++)
            WorldGenerator.AddBlockGlobal(basePos + new Vector3Int(0, i, 0), new Block(4));

        for (var dx = -1; dx <= 1; dx++)
        for (var dy = 0; dy <= 2; dy++)
        for (var dz = -1; dz <= 1; dz++)
        {
            if (dx == 0 && dy == 1 && dz == 0) continue;
            var leafPos = basePos + new Vector3Int(dx, 3 + dy, dz);

            if(!WorldGenerator.TryGetBlockGlobal(leafPos, out _))
            {
                WorldGenerator.AddBlockGlobal(leafPos, new Block(5));
            }
        }
    }
}