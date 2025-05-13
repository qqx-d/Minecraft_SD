using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace minecraft;

public class Chunk
{
    public MeshRenderer Mesh { get; private set; }

    private Dictionary<Vector3Int, Block> _blocks = new();
    public Vector3Int Position { get; private set; }
    
    public int BlockCount => _blocks.Count;

    private float _amplitude = 8f;
    
    public Chunk(Vector3Int position)
    {
        Mesh = new MeshRenderer();
        Position = position;

        GenerateData();
    }
    
    public void BuildMesh()
    {
        ChunkMeshBuilder.BuildChunkMesh(this);
    }

    private void GenerateData()
    {
        GenerateBlocks(GenerateHeightMap());
    }

    private void GenerateBlocks(float[,] heightMap)
    {
        for (int x = 0; x < WorldGenerator.ChunkWidth; x++)
        {
            for (int z = 0; z < WorldGenerator.ChunkWidth; z++)
            {
                int columnHeight = (int)MathF.Floor(heightMap[x, z]);

                for (int y = 0; y < WorldGenerator.ChunkHeight; y++)
                {
                    if (y > columnHeight) continue;

                    var blockPos = new Vector3Int(x, y, z);

                    var blockID = 0;

                    if (y == columnHeight)
                        blockID = 1;
                    else if (y >= columnHeight - 5)
                        blockID = 2;
                    else
                        blockID = 3;

                    _blocks[blockPos] = new Block(blockID);
                    
                }
                
                if (columnHeight < WorldGenerator.SeaLevel)
                {
                    for (int y = columnHeight + 1; y <= WorldGenerator.SeaLevel; y++)
                    {
                        var waterPos = new Vector3Int(x, y, z);
                        _blocks[waterPos] = new Block(6);
                    }
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
    
    public static bool IsBlockAt(Vector3 position)
    {
        var posInt = (Vector3Int)position;
        
        return WorldGenerator.Instance.TryGetBlockGlobal(posInt) != null;
    }

}