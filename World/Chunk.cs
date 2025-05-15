using minecraft.Rendering;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace minecraft;

public class Chunk
{
    public MeshRenderer OpaqueMesh { get; private set; }
    public MeshRenderer TransparentMesh { get; private set; }
    

    private readonly Dictionary<Vector3Int, Block> _blocks = new();
    
    public Vector3Int Position { get; private set; }
    public bool DataGenerated { get; private set; } = false;
    

    private float _amplitude = 8f;
    
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

    public void GenerateData()
    {
        GenerateBlocks(GenerateHeightMap());
        
        DataGenerated = true;
    }

    private void GenerateBlocks(float[,] heightMap)
    {
        for(var x = 0; x < WorldGenerator.ChunkWidth; x++)
        {
            for(var z = 0; z < WorldGenerator.ChunkWidth; z++)
            {
                int columnHeight = (int)MathF.Floor(heightMap[x, z]);

                if (columnHeight < WorldGenerator.SeaLevel)
                {
                    for (int y = columnHeight + 1; y <= WorldGenerator.SeaLevel; y++)
                    {
                        var waterPos = new Vector3Int(x, y, z);
                        _blocks[waterPos] = new Block(6);
                    }
                }
                
                for (int y = 0; y < WorldGenerator.ChunkHeight; y++)
                {
                    if (y > columnHeight) continue;

                    var blockPos = new Vector3Int(x, y, z);

                    var blockID = 0;

                    if (y == columnHeight)
                    {
                        var isUnderWater = columnHeight < WorldGenerator.SeaLevel;
                        var nearWater = false;
                        
                        for(var dx = -1; dx <= 1; dx++)
                        {
                            for(var dz = -1; dz <= 1; dz++)
                            {
                                var nx = x + dx;
                                var nz = z + dz;

                                if (nx < 0 || nz < 0 || nx >= WorldGenerator.ChunkWidth || nz >= WorldGenerator.ChunkWidth)
                                    continue;

                                var neighborHeight = (int)MathF.Floor(heightMap[nx, nz]);
                                if(neighborHeight < WorldGenerator.SeaLevel)
                                {
                                    nearWater = true;
                                    break;
                                }
                            }
                            if (nearWater) break;
                        }

                        if (isUnderWater || nearWater)
                            blockID = 7;
                        else
                            blockID = 1;
                    }
                    else if (y >= columnHeight - 5)
                        blockID = 2;
                    else
                        blockID = 3;

                    _blocks[blockPos] = new Block(blockID);
                    
                }
            }
        }
    }

    private float[,] GenerateHeightMap()
    {
        var map = new float[WorldGenerator.ChunkWidth, WorldGenerator.ChunkWidth];

        for (int x = 0; x < WorldGenerator.ChunkWidth; x++)
        {
            for (int z = 0; z < WorldGenerator.ChunkWidth; z++)
            {
                var worldX = Position.X + x;
                var worldZ = Position.Z + z;
                
                var baseHeight = WorldGenerator.ChunkHeight / 2f;
                var noise = WorldGenerator.FastNoiseLite.GetNoise(worldX, worldZ);
                var height = baseHeight + noise * _amplitude;
                map[x, z] = height;

            }
        }

        return map;
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