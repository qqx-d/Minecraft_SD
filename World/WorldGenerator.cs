using minecraft.Sys;
using OpenTK.Mathematics;
using static FastNoiseLite;

namespace minecraft;

public class WorldGenerator
{
    public static WorldGenerator Instance { get; private set; }
    
    public const int ChunkWidth = 16;
    public const int ChunkHeight = 16;
    public const int RenderDistance = 24;
    
    public const int SeaLevel = 4;
    
    public static FastNoiseLite FastNoiseLite { get; private set; }
    public static FastNoiseLite ForestNoise;


    private readonly Dictionary<Vector3Int, Chunk> _chunks = new();
    public WorldGenerator()
    {
        Instance = this;

        var seed = new Random().Next(0, 99999999);
        
        FastNoiseLite = new FastNoiseLite();
        FastNoiseLite.SetNoiseType(NoiseType.OpenSimplex2);
        FastNoiseLite.SetFrequency(0.009f);
        FastNoiseLite.SetSeed(seed);
        
        ForestNoise = new FastNoiseLite();
        ForestNoise.SetSeed(seed);
        ForestNoise.SetFrequency(3f);
        ForestNoise.SetNoiseType(NoiseType.OpenSimplex2);

    }
    
    public Chunk GetChunkAt(Vector3Int position)
    {
        if(!_chunks.ContainsKey(position))
        {
            _chunks.Add(position, new Chunk(position));
        }

        return _chunks[position];
    }

    public void Update()
    {
        RenderChunks(Window.ActiveCamera.transform.position);
    }

    private void RenderChunks(Vector3 targetPosition)
    {
        var playerChunkX = (int)MathF.Floor(targetPosition.X / ChunkWidth);
        var playerChunkZ = (int)MathF.Floor(targetPosition.Z / ChunkWidth);

        var chunksToRender = new List<Chunk>();

        for (var x = -RenderDistance; x <= RenderDistance; x++)
        for (var z = -RenderDistance; z <= RenderDistance; z++)
        {
            var chunkPos = new Vector3Int(playerChunkX + x, 0, playerChunkZ + z) * ChunkWidth;
            var chunk = GetChunkAt(chunkPos);

            if (!chunk.DataGenerated)
            {
                chunk.GenerateData();
            }

            chunksToRender.Add(chunk);
        }
        
        foreach (var chunk in chunksToRender)
        {
            if (!chunk.Mesh.IsMeshUploaded)
            {
                chunk.BuildMesh();
                //TreeFeature.GenerateTreesInArea(chunk.Position);
            }

            ChunkRenderer.AddToRender(chunk);
        }
    }

    
    public void AddBlockGlobal(Vector3Int worldPos, Block block)
    {
        var chunkX = (int)MathF.Floor(worldPos.X / ChunkWidth) * ChunkWidth;
        var chunkZ = (int)MathF.Floor(worldPos.Z / ChunkWidth) * ChunkWidth;
        Vector3Int chunkPos = new(chunkX, 0, chunkZ);

        var chunk = GetChunkAt(chunkPos);
        Vector3Int localPos = new(
            (int)(worldPos.X - chunkPos.X),
            (int)(worldPos.Y),
            (int)(worldPos.Z - chunkPos.Z)
        );

        chunk.GetAllBlocks()[localPos] = block;
        chunk.BuildMesh();

        UpdateNeighborChunks(worldPos);
    }

    public void RemoveBlockGlobal(Vector3Int worldPos)
    {
        var chunkX = (int)MathF.Floor(worldPos.X / ChunkWidth) * ChunkWidth;
        var chunkZ = (int)MathF.Floor(worldPos.Z / ChunkWidth) * ChunkWidth;
        Vector3Int chunkPos = new(chunkX, 0, chunkZ);

        var chunk = GetChunkAt(chunkPos);
        Vector3Int localPos = new(
            (int)(worldPos.X - chunkPos.X),
            (int)(worldPos.Y),
            (int)(worldPos.Z - chunkPos.Z)
        );

        chunk.GetAllBlocks().Remove(localPos);
        chunk.BuildMesh();

        UpdateNeighborChunks(worldPos);
    }

    private void UpdateNeighborChunks(Vector3Int worldPos)
    {
        foreach (var dir in new[] {
                     new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0),
                     new Vector3Int(0, 0, 1), new Vector3Int(0, 0, -1),
                     new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0)
                 })
        {
            var neighborPos = worldPos + dir;
            var chunkX = (int)MathF.Floor(neighborPos.X / ChunkWidth) * ChunkWidth;
            var chunkZ = (int)MathF.Floor(neighborPos.Z / ChunkWidth) * ChunkWidth;
            Vector3Int chunkPos = new(chunkX, 0, chunkZ);

            var neighbor = GetChunkAt(chunkPos);
            neighbor.BuildMesh();
        }
    }

    
    public bool TryGetBlockGlobal(Vector3Int worldPos, out Block blockAtGlobal)
    {
        var chunkX = (int)MathF.Floor(worldPos.X / ChunkWidth) * ChunkWidth;
        var chunkZ = (int)MathF.Floor(worldPos.Z / ChunkWidth) * ChunkWidth;

        var chunkPos = new Vector3Int(chunkX, 0, chunkZ);
        
        if(_chunks.TryGetValue(chunkPos, out var chunk))
        {
            var localX = (int)(worldPos.X - chunkPos.X);
            var localY = (int)(worldPos.Y);
            var localZ = (int)(worldPos.Z - chunkPos.Z);

            var localPos = new Vector3Int(localX, localY, localZ);

            if (chunk.TryGetBlockAt(localPos, out var block))
            {
                blockAtGlobal = block;
                
                return true;
            }
        }

        blockAtGlobal = null;
        
        return false;
    }
}