using System;
using OpenTK.Mathematics;

namespace minecraft.World;

public static class TreeFeature
{
    private static WorldGenerator WorldGenerator => WorldGenerator.Instance;

    public static void GenerateTreesInArea(Chunk chunk)
    {
        const int step = 4;

        for (var x = 0; x < WorldGenerator.ChunkWidth; x += step)
        for (var z = 0; z < WorldGenerator.ChunkWidth; z += step)
        {
            int worldX = (int) chunk.Position.X + x;
            int worldZ = (int) chunk.Position.Z + z;

            var forestNoise = WorldGenerator.ForestNoise.GetNoise(worldX, worldZ);
            if (forestNoise < 0.4f) continue;

            var chance = WorldGenerator.FastNoiseLite.GetNoise(worldX + 1000, worldZ + 1000);
            if (chance > 0.7f)
            {
                Vector3Int? localGround = FindGrassInChunk(chunk, x, z);
                if (localGround.HasValue)
                    PlaceTreeAtBuffered(chunk, localGround.Value + new Vector3Int(0, 1, 0));
            }
        }
    }

    private static Vector3Int? FindGrassInChunk(Chunk chunk, int localX, int localZ)
    {
        for (int y = WorldGenerator.ChunkHeight - 1; y >= 0; y--)
        {
            var pos = new Vector3Int(localX, y, localZ);
            if (chunk.TryGetBlockAt(pos, out var block) && block.Id == 1) // grass
                return pos;
        }

        return null;
    }

    private static void PlaceTreeAtBuffered(Chunk chunk, Vector3Int basePos)
    {
        // Ствол (4 блока вверх)
        for (int i = 0; i < 4; i++)
            chunk.BufferBlock(basePos + new Vector3Int(0, i, 0), new Block(4)); // log

        // Листва — 3x3x3
        for (int dx = -1; dx <= 1; dx++)
        for (int dy = 0; dy <= 2; dy++)
        for (int dz = -1; dz <= 1; dz++)
        {
            if (dx == 0 && dy == 1 && dz == 0) continue;

            var leafPos = basePos + new Vector3Int(dx, 3 + dy, dz);
            chunk.BufferBlock(leafPos, new Block(5)); // leaf
        }
    }
}