using System.Collections.Concurrent;
using minecraft.Rendering;
using minecraft.Sys;
using OpenTK.Mathematics;
using static FastNoiseLite;

namespace minecraft.World;

public class WorldGenerator
{
    public static WorldGenerator Instance { get; private set; }
    
    public Random Random { get; } = new();
    
    public const int ChunkWidth = 16;
    public const int ChunkHeight = 64;
    public const int RenderDistance = 12;
    
    public const int SeaLevel = 32;
    
    public static FastNoiseLite FastNoiseLite { get; private set; }
    public static FastNoiseLite ForestNoise { get; private set; }
    public static FastNoiseLite CaveNoise { get; private set; }
    
    private readonly Dictionary<Vector3Int, Chunk> _generatedChunks = new();
    private readonly ConcurrentQueue<Chunk> _chunksToUpdate = new();
    
    private readonly ConcurrentQueue<Chunk> _chunksToUpload = new();
    
    private readonly SemaphoreSlim _generationLimiter;
    private readonly SemaphoreSlim _meshBuildLimiter;
    
    private readonly Vector3Int[] _initialPositions =
    [
        new(0, 0, -ChunkWidth),
        new(-ChunkWidth, 0, 0),
        new(0, 0, 0),
        new(-ChunkWidth, 0, -ChunkWidth)
    ];
    
    private readonly Vector3Int[] _neighbourChunksDirections =
    [
        new(ChunkWidth, 0, 0),
        new(-ChunkWidth, 0, 0),
        new(0, 0, ChunkWidth),
        new(0, 0, -ChunkWidth)
    ];
    
    public WorldGenerator()
    {
        Instance = this;

        var cpu = Environment.ProcessorCount;
        
        _generationLimiter = new SemaphoreSlim(Math.Max(1, cpu - 8));
        _meshBuildLimiter = new SemaphoreSlim(Math.Max(1, cpu  - 8));

        InitializeNoises();
    }

    private void InitializeNoises()
    {
        var seed = new Random().Next(0, 99999999);
        
        FastNoiseLite = new FastNoiseLite();
        FastNoiseLite.SetNoiseType(NoiseType.OpenSimplex2);
        FastNoiseLite.SetFrequency(0.009f);
        FastNoiseLite.SetSeed(seed);
        
        ForestNoise = new FastNoiseLite();
        ForestNoise.SetSeed(seed);
        ForestNoise.SetFrequency(3f);
        ForestNoise.SetNoiseType(NoiseType.OpenSimplex2);

        CaveNoise = new FastNoiseLite();
        CaveNoise.SetSeed(seed);
        CaveNoise.SetFrequency(0.05f);
        CaveNoise.SetNoiseType(NoiseType.OpenSimplex2);
    }

    public Chunk GetChunkAt(Vector3Int position)
    {
        if(!_generatedChunks.ContainsKey(position))
        {
            _generatedChunks.Add(position, new Chunk(position));
        }

        return _generatedChunks[position];
    }

    public void Update()
    {
        RenderChunks(Window.ActiveCamera.transform.position);

        while (_chunksToUpdate.TryDequeue(out var chunk))
        {
            chunk.OpaqueMesh.Upload(chunk.OpaqueMesh.Vertices, chunk.OpaqueMesh.Indices);
            chunk.TransparentMesh.Upload(chunk.TransparentMesh.Vertices, chunk.TransparentMesh.Indices);
        }
        
        while (_chunksToUpload.TryDequeue(out var chunk))
        {
            chunk.OpaqueMesh.Upload(chunk.OpaqueMesh.Vertices, chunk.OpaqueMesh.Indices);
            chunk.TransparentMesh.Upload(chunk.TransparentMesh.Vertices, chunk.TransparentMesh.Indices);
            ChunkRenderer.AddToRender(chunk);
        }
    }


    private void RenderChunks(Vector3 targetPosition)
    {
        var playerChunkX = (int) MathF.Floor(targetPosition.X / ChunkWidth);
        var playerChunkZ = (int) MathF.Floor(targetPosition.Z / ChunkWidth);

        var chunkPositions = new List<Vector3Int>();

        for (var dx = -RenderDistance; dx <= RenderDistance; dx++)
        for (var dz = -RenderDistance; dz <= RenderDistance; dz++)
        {
            var chunkPos = new Vector3Int((playerChunkX + dx) * ChunkWidth, 0, (playerChunkZ + dz) * ChunkWidth);
            chunkPositions.Add(chunkPos);
        }
        
        chunkPositions = chunkPositions
            .OrderBy(pos => (new Vector3(pos.X, 0, pos.Z) - new Vector3(playerChunkX * ChunkWidth, 0, playerChunkZ * ChunkWidth)).LengthSquared)
            .ToList();

        foreach(var chunkPos in chunkPositions)
        {
            var chunk = GetChunkAt(chunkPos);

            if (!chunk.DataGenerated)
            {
                _ = Task.Run(async () =>
                {
                    await _generationLimiter.WaitAsync();
                    try
                    {
                        await chunk.GenerateDataAsync();
                        ChunkMeshBuilder.BuildChunkMesh(chunk);
                        _chunksToUpload.Enqueue(chunk);
                        UpdateNeighborChunkMeshes(chunk.Position);
                    }
                    finally
                    {
                        _generationLimiter.Release();
                    }
                });
            }
            
            ChunkRenderer.AddToRender(chunk);
        }
    }

    
    public void GenerateInitialChunks()
    {
        foreach (var pos in _initialPositions)
        {
            var chunk = GetChunkAt(pos);
            chunk.GenerateDataAsync().Wait();
            ChunkMeshBuilder.BuildChunkMesh(chunk);
            chunk.OpaqueMesh.Upload(chunk.OpaqueMesh.Vertices, chunk.OpaqueMesh.Indices);
            chunk.TransparentMesh.Upload(chunk.TransparentMesh.Vertices, chunk.TransparentMesh.Indices);
            ChunkRenderer.AddToRender(chunk);
        }
    }
    
    public void AddBlockGlobal(Vector3Int worldPos, Block block)
    {
        var chunkX = (int)MathF.Floor((float) worldPos.X / ChunkWidth) * ChunkWidth;
        var chunkZ = (int)MathF.Floor((float) worldPos.Z / ChunkWidth) * ChunkWidth;
        Vector3Int chunkPos = new(chunkX, 0, chunkZ);

        var chunk = GetChunkAt(chunkPos);
        Vector3Int localPos = new(
            (worldPos.X - chunkPos.X),
            (worldPos.Y),
            (worldPos.Z - chunkPos.Z)
        );

        chunk.GetAllBlocks()[localPos] = block;

        _ = UpdateChunkMeshAsync(chunk);
        UpdateNeighborChunks(worldPos);
    }

    public void RemoveBlockGlobal(Vector3Int worldPos)
    {
        var chunkX = (int)MathF.Floor((float) worldPos.X / ChunkWidth) * ChunkWidth;
        var chunkZ = (int)MathF.Floor((float) worldPos.Z / ChunkWidth) * ChunkWidth;
        Vector3Int chunkPos = new(chunkX, 0, chunkZ);

        var chunk = GetChunkAt(chunkPos);
        Vector3Int localPos = new(
            (worldPos.X - chunkPos.X),
            (worldPos.Y),
            (worldPos.Z - chunkPos.Z)
        );

        chunk.GetAllBlocks().Remove(localPos);

        _ = UpdateChunkMeshAsync(chunk);
        UpdateNeighborChunks(worldPos);
    }


    private void UpdateNeighborChunks(Vector3Int worldPos)
    {
        foreach (var dir in _neighbourChunksDirections)
        {
            var neighborPos = worldPos + dir;
            var chunkX = (int)MathF.Floor((float) neighborPos.X / ChunkWidth) * ChunkWidth;
            var chunkZ = (int)MathF.Floor((float) neighborPos.Z / ChunkWidth) * ChunkWidth;
            Vector3Int chunkPos = new(chunkX, 0, chunkZ);

            var neighbor = GetChunkAt(chunkPos);
            neighbor.BuildMesh();
        }
    }

    private async Task UpdateChunkMeshAsync(Chunk chunk)
    {
        await _meshBuildLimiter.WaitAsync();
        try
        {
            await Task.Run(() =>
            {
                chunk.BuildMesh();
                _chunksToUpdate.Enqueue(chunk);
            });
        }
        finally
        {
            _meshBuildLimiter.Release();
        }
    }
    
    private void UpdateNeighborChunkMeshes(Vector3Int chunkPos)
    {
        foreach (var dir in _neighbourChunksDirections)
        {
            var neighborPos = chunkPos + dir;
            if (_generatedChunks.TryGetValue(neighborPos, out var neighborChunk) && neighborChunk.DataGenerated)
            {
                _ = UpdateChunkMeshAsync(neighborChunk);
            }
        }
    }
    
    public bool TryGetBlockGlobal(Vector3Int worldPos, out Block blockAtGlobal)
    {
        var chunkX = (int)MathF.Floor((float) worldPos.X / ChunkWidth) * ChunkWidth;
        var chunkZ = (int)MathF.Floor((float) worldPos.Z / ChunkWidth) * ChunkWidth;

        var chunkPos = new Vector3Int(chunkX, 0, chunkZ);
        
        if(_generatedChunks.TryGetValue(chunkPos, out var chunk))
        {
            var localX = (worldPos.X - chunkPos.X);
            var localY = (worldPos.Y);
            var localZ = (worldPos.Z - chunkPos.Z);

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