using minecraft.Rendering;

namespace minecraft.World;

public class Chunk
{
    public MeshRenderer OpaqueMesh { get; private set; }
    public MeshRenderer TransparentMesh { get; private set; }
    

    private readonly Dictionary<Vector3Int, Block> _blocks = new();
    
    public Vector3Int Position { get; private set; }
    
    private readonly List<(Vector3Int localPos, Block block)> _pendingTreeBlocks = [];


    private const float HeightAmplitude = 8f;
    private const float GrassChance = 0.5f;

    public bool DataGenerated { get; private set; } = false;
    
    
    public Chunk(Vector3Int position)
    {
        OpaqueMesh = new MeshRenderer("shader");
        TransparentMesh = new MeshRenderer("water");
        Position = position;
    }
    
    public void BuildMesh()
    {
        ChunkMeshBuilder.BuildChunkMesh(this);
    }

    public async Task GenerateDataAsync()
    {
        await Task.Run(() =>
        {
            var heightMap = GenerateHeightNoise();
            var caveMap = GenerateCaveNoise();
            GenerateBlocks(heightMap, caveMap);
            TreeFeature.GenerateTreesInArea(this);
            ApplyBufferedBlocks();

            _pendingTreeBlocks.Clear();
            DataGenerated = true;
        });
    }

    private void GenerateBlocks(float[,] heightMap, float[,,] caveMap)
    {
        for(var x = 0; x < WorldGenerator.ChunkWidth; x++)
        {
            for(var z = 0; z < WorldGenerator.ChunkWidth; z++)
            {
                var columnHeight = (int)MathF.Floor(heightMap[x, z]);
                
                if(columnHeight < WorldGenerator.SeaLevel)
                {
                    for (var y = columnHeight + 1; y is <= WorldGenerator.SeaLevel; y++)
                    {
                        var waterPos = new Vector3Int(x, y, z);
                        _blocks[waterPos] = new Block(6);
                    }
                }

                for (var y = 0; y < WorldGenerator.ChunkHeight; y++)
                {
                    var blockPos = new Vector3Int(x, y, z);
                    int blockId;

                    if (y == columnHeight)
                    {
                        var isUnderWater = columnHeight < WorldGenerator.SeaLevel;
                        var nearWater = false;

                        for (var dx = -1; dx <= 1; dx++)
                        {
                            for (var dz = -1; dz <= 1; dz++)
                            {
                                var nx = x + dx;
                                var nz = z + dz;

                                if (nx < 0 || nz < 0 || nx >= WorldGenerator.ChunkWidth ||
                                    nz >= WorldGenerator.ChunkWidth)
                                    continue;
    
                                var neighborHeight = (int)MathF.Floor(heightMap[nx, nz]);
                                
                                if (neighborHeight < WorldGenerator.SeaLevel)
                                {
                                    nearWater = true;
                                    break;
                                }
                            }

                            if (nearWater) break;
                        }

                        if (isUnderWater || nearWater)
                            blockId = 7;
                        else
                            blockId = 1;
                        
                        _blocks[blockPos] = new Block(blockId);
                        
                        if (blockId == 1)
                        {
                            var abovePos = new Vector3Int(x, y + 1, z);
                            var chance = WorldGenerator.Instance.Random.NextDouble();
                            
                            if (!_blocks.ContainsKey(abovePos) && chance < GrassChance)
                            {
                                _blocks[abovePos] = new Block(9);
                            }
                        }

                    }
                    else if (y >= columnHeight - 5)
                        blockId = 2;
                    else
                        blockId = 3;

                    var depthFactor = (WorldGenerator.SeaLevel - y) / 6f;
                    var caveThreshold = 0.4f + Math.Clamp(1f - depthFactor, 0, 1f);

                    if (caveMap[x, y, z] > caveThreshold || y > columnHeight)
                        continue;

                    
                    _blocks[blockPos] = new Block(blockId);
                }
            }
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
    
    public void BufferBlock(Vector3Int localPos, Block block)
    {
        _pendingTreeBlocks.Add((localPos, block));
    }

    private void ApplyBufferedBlocks()
    {
        foreach (var (pos, block) in _pendingTreeBlocks)
            _blocks[pos] = block;

        _pendingTreeBlocks.Clear();
    }

    public Dictionary<Vector3Int, Block> GetAllBlocks()
    {
        return _blocks;
    }
    
    public bool TryGetBlockAt(Vector3Int localPos, out Block block)
    {
        return _blocks.TryGetValue(localPos, out block);
    }
}