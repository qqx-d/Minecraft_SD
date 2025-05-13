using OpenTK.Mathematics;

namespace minecraft;

public static class ChunkMeshBuilder
{

    private const int TileCountInRaw = 16;
    private static readonly Dictionary<int, Vector2> _uvOffsets = new();
    
    public static void BuildChunkMesh(Chunk chunk)
    {
        List<float> vertices = new();
        List<uint> indices = new();
        uint offset = 0;

        var blocks = chunk.GetAllBlocks();
        Vector3Int chunkOrigin = chunk.Position;

        foreach (var kvp in blocks)
        {
            var localPos = kvp.Key;
            var block = kvp.Value;
            
            if (block.ID == 0) continue;
            
            var worldPos = new Vector3Int(
                chunkOrigin.X + localPos.X,
                chunkOrigin.Y + localPos.Y,
                chunkOrigin.Z + localPos.Z
            );

            foreach (var face in Enum.GetValues<BlockFace>())
            {
                Vector3Int neighborPos = worldPos + GetDirection(face);
                
                if (WorldGenerator.Instance.TryGetBlockGlobal(neighborPos) == null)
                {
                    Vector3 positionVec = new(localPos.X, localPos.Y, localPos.Z);
                    Vector2 uvOffset = _uvOffsets[block.ID];
                    BlockRenderer.AddFace(face, positionVec, uvOffset, vertices, indices, ref offset);
                }
            }
        }

        chunk.Mesh.Upload(vertices.ToArray(), indices.ToArray());
    }

    private static Vector3Int GetDirection(BlockFace face)
    {
        return face switch
        {
            BlockFace.Top    => new Vector3Int(0, 1, 0),
            BlockFace.Bottom => new Vector3Int(0, -1, 0),
            BlockFace.Left   => new Vector3Int(-1, 0, 0),
            BlockFace.Right  => new Vector3Int(1, 0, 0),
            BlockFace.Front  => new Vector3Int(0, 0, 1),
            BlockFace.Back   => new Vector3Int(0, 0, -1),
            _ => Vector3Int.Zero
        };
    }

    public static void GenerateUVOffsets()
    {
        for (var i = 0; i < TileCountInRaw; i++)
        {
            _uvOffsets.Add(i, new Vector2(1f / TileCountInRaw * i, 0f));
        }
    }
}
