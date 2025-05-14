using OpenTK.Mathematics;

namespace minecraft.Rendering;

public static class ChunkMeshBuilder
{

    private const int TileCountInRaw = 16;
    private static readonly Dictionary<int, Vector2> UvOffsets = new();
    
    public static void BuildChunkMesh(Chunk chunk)
    {
        var vertices = new  List<float>();
        var  indices = new List<uint>();
        uint offset = 0;

        var blocks = chunk.GetAllBlocks();
        var chunkOrigin = chunk.Position;

        foreach (var kvp in blocks)
        {
            var localPos = kvp.Key;
            var block = kvp.Value;
            
            if (block.Id == 0) continue;
            
            var worldPos = new Vector3Int(
                chunkOrigin.X + localPos.X,
                chunkOrigin.Y + localPos.Y,
                chunkOrigin.Z + localPos.Z
            );

            foreach (var face in Enum.GetValues<BlockFace>())
            {
                var neighborPos = worldPos + GetDirection(face);
                
                if(!WorldGenerator.Instance.TryGetBlockGlobal(neighborPos, out _))
                {
                    var positionVec = new Vector3(localPos.X, localPos.Y, localPos.Z);
                    var uvOffset = UvOffsets[block.Id];
                    
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

    public static void GenerateUvOffsets()
    {
        for (var i = 0; i < TileCountInRaw; i++)
        {
            UvOffsets.Add(i, new Vector2(1f / TileCountInRaw * i, 0f));
        }
    }
}
