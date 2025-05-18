using minecraft.Sys;

namespace minecraft.World;

public static class TreeFeature
{
    private static readonly List<(Vector3Int localPos, Block block)> PendingTreeBlocks = [];

    public static void ClearBlocksFromPending()
    {
        PendingTreeBlocks.Clear();
    }
    
    public static IReadOnlyList<(Vector3Int localPos, Block block)> GetPendingTreeBlocks() => PendingTreeBlocks;

    
    public static void GenerateTreesInArea(Chunk chunk)
    {
        const int step = 4;

        for(var x = 0; x < WorldGenerator.ChunkWidth; x += step)
        for(var z = 0; z < WorldGenerator.ChunkWidth; z += step)
        {
            var worldX = chunk.Position.X + x;
            var worldZ = chunk.Position.Z + z;

            var forestNoise = WorldGenerator.ForestNoise.GetNoise(worldX, worldZ);
            if(forestNoise < 0.4f) continue;

            var chance = WorldGenerator.FastNoiseLite.GetNoise(worldX + 1000, worldZ + 1000);
            if(chance > 0.7f)
            {
                var localGround = FindGrassInChunk(chunk, x, z);
                
                if (localGround.HasValue)
                    PlaceTreeAtBuffered(localGround.Value + new Vector3Int(0, 1, 0));
            }
        }
    }

    private static Vector3Int? FindGrassInChunk(Chunk chunk, int localX, int localZ)
    {
        for(var y = WorldGenerator.ChunkHeight - 1; y >= 0; y--)
        {
            var pos = new Vector3Int(localX, y, localZ);
            
            if(chunk.TryGetBlockAt(pos, out var block) && block.Id == 1) 
                return pos;
        }

        return null;
    }

    private static void PlaceTreeAtBuffered(Vector3Int basePos)
    {
        // log 4 up
        for (var i = 0; i < 4; i++)
            PendingTreeBlocks.Add((basePos + new Vector3Int(0, i, 0), new Block(4)));

        // leaf — 3x3x3
        for (var dx = -1; dx <= 1; dx++)
        for (var dy = 0; dy <= 2; dy++)
        for (var dz = -1; dz <= 1; dz++)
        {
            var leafPos = basePos + new Vector3Int(dx, 3 + dy, dz);
            
            PendingTreeBlocks.Add((leafPos, new Block(5)));
        }
    }
}