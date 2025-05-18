using System.Collections.Concurrent;
using minecraft.Rendering;
using minecraft.Sys;

namespace minecraft.World;

public class Chunk
{
    public MeshRenderer OpaqueMesh { get; private set; }
    public MeshRenderer TransparentMesh { get; private set; }
    

    private readonly ConcurrentDictionary<Vector3Int, Block> _blocks = new();
    
    public Vector3Int Position { get; private set; }

    private const float HeightAmplitude = 8f;
    private const float GrassChance = 0.5f;

    public bool DataGenerated { get; private set; } = false;
    
    public Chunk(Vector3Int position)
    {
        OpaqueMesh = new MeshRenderer("simple_shader");
        TransparentMesh = new MeshRenderer("transparent_support");
        Position = position;
    }
    
    public void BuildMesh()
    {
        ChunkMeshBuilder.BuildChunkMesh(this);
    }

    public async Task GenerateDataAsync(bool withTrees = true)
    {
        await Task.Run(() =>
        {
            GenerateBlocks(GenerateHeightNoise(), GenerateCaveNoise());

            if (withTrees)
            {
                TreeFeature.GenerateTreesInArea(this);
                ApplyBufferedBlocks();
            }

            DataGenerated = true;
        });
    }

    private void GenerateBlocks(float[,] heightMap, float[,,] caveMap)
    {
        for (var x = 0; x < WorldGenerator.ChunkWidth; x++)
        {
            for (var z = 0; z < WorldGenerator.ChunkWidth; z++)
            {
                var columnHeight = (int)MathF.Floor(heightMap[x, z]);

                GenerateWaterAboveColumn(x, z, columnHeight);

                for (var y = 0; y < WorldGenerator.ChunkHeight; y++)
                {
                    Vector3Int pos = new(x, y, z);
                    var blockId = GetBlockId(x, y, z, columnHeight, heightMap);

                    if (!ShouldPlaceBlock(x, y, z, blockId, columnHeight, caveMap))
                        continue;

                    _blocks[pos] = new Block(blockId);

                    TryPlaceShortGrass(x, y, z, blockId);
                }
            }
        }
    }

    private void GenerateWaterAboveColumn(int x, int z, int columnHeight)
    {
        if (columnHeight >= WorldGenerator.SeaLevel) return;

        for (var y = columnHeight + 1; y <= WorldGenerator.SeaLevel; y++)
        {
            _blocks[new Vector3Int(x, y, z)] = new Block(6);
        }
    }

    private int GetBlockId(int x, int y, int z, int columnHeight, float[,] heightMap)
    {
        if(y == 0)
            return 8;

        if(y == columnHeight)
            return IsNearWater(x, z, heightMap) || columnHeight < WorldGenerator.SeaLevel ? 7 : 1;

        if(y >= columnHeight - 5)
            return 2;

        return 3;
    }

    private bool IsNearWater(int x, int z, float[,] heightMap)
    {
        for(var dx = -1; dx <= 1; dx++)
        {
            for(var dz = -1; dz <= 1; dz++)
            {
                var nx = x + dx;
                var nz = z + dz;

                if(nx < 0 || nz < 0 || nx >= WorldGenerator.ChunkWidth || nz >= WorldGenerator.ChunkWidth)
                    continue;

                var neighborHeight = (int)MathF.Floor(heightMap[nx, nz]);

                if(neighborHeight < WorldGenerator.SeaLevel)
                    return true;
            }
        }

        return false;
    }

    private bool ShouldPlaceBlock(int x, int y, int z, int blockId, int columnHeight, float[,,] caveMap)
    {
        if (y == 0 || (y == columnHeight && blockId == 1))
            return true;

        var depthFactor = (WorldGenerator.SeaLevel - 10f - y) / 6f;
        var caveThreshold = 0.4f + Math.Clamp(1f - depthFactor, 0f, 1f);

        return caveMap[x, y, z] <= caveThreshold && y <= columnHeight;
    }

    private void TryPlaceShortGrass(int x, int y, int z, int blockId)
    {
        if(blockId != 1) return;

        Vector3Int above = new(x, y + 1, z);
        var chance = WorldGenerator.Instance.Random.NextDouble();

        if (!_blocks.ContainsKey(above) && chance < GrassChance)
        {
            _blocks[above] = new Block(9);
        }
    }

    private float[,] GenerateHeightNoise()
    {
        var map = new float[WorldGenerator.ChunkWidth, WorldGenerator.ChunkWidth];

        for (var x = 0; x < WorldGenerator.ChunkWidth; x++)
        {
            for (var z = 0; z < WorldGenerator.ChunkWidth; z++)
            {
                var worldX = Position.X + x;
                var worldZ = Position.Z + z;
                
                var baseHeight = WorldGenerator.ChunkHeight / 2f;
                var noise = WorldGenerator.FastNoiseLite.GetNoise(worldX, worldZ);
                var height = baseHeight + noise * HeightAmplitude;
                
                map[x, z] = height;

            }
        }

        return map;
    }

    private float[,,] GenerateCaveNoise()
    {
        var noiseMap = new float[WorldGenerator.ChunkWidth, WorldGenerator.ChunkHeight, WorldGenerator.ChunkWidth];

        for (var x = 0; x < WorldGenerator.ChunkWidth; x++)
        for (var y = 0; y < WorldGenerator.ChunkHeight; y++)
        for (var z = 0; z < WorldGenerator.ChunkWidth; z++)
        {
            var worldX = Position.X + x;
            var worldY = y;
            var worldZ = Position.Z + z;

            var noise = WorldGenerator.CaveNoise.GetNoise(worldX, worldY, worldZ);
            noiseMap[x, y, z] = noise;
        }

        return noiseMap;
    }
    
    private void ApplyBufferedBlocks()
    {
        foreach (var (pos, block) in TreeFeature.GetPendingTreeBlocks())
            _blocks[pos] = block;

        TreeFeature.ClearBlocksFromPending();
    }

    public ConcurrentDictionary<Vector3Int, Block> GetAllBlocks()
    {
        return _blocks;
    }
    
    public bool TryGetBlockAt(Vector3Int localPos, out Block block)
    {
        return _blocks.TryGetValue(localPos, out block);
    }
}