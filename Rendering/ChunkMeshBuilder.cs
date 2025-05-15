using minecraft.World;
using OpenTK.Mathematics;

namespace minecraft.Rendering;

public static class ChunkMeshBuilder
{
    public static void BuildChunkMesh(Chunk chunk)
    {
        var verticesOpaque = new List<float>();
        var indicesOpaque  = new List<uint>();
        var verticesTransparent  = new List<float>();
        var indicesTransparent   = new List<uint>();

        uint offsetOpaque = 0;
        uint offsetWater  = 0;

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

                var neighborTransparent = 
                    !WorldGenerator.Instance.TryGetBlockGlobal(neighborPos, out var neighborBlock) || neighborBlock.IsTransparent();

                var positionVec = new Vector3(localPos.X, localPos.Y, localPos.Z);
                var blockTexture = BlockTextureRegistry.BlockTextures[block.Id];
                var uvOffset = blockTexture.GetUvForFace(face);
                
                if (neighborTransparent && !block.IsTransparent())
                {
                    BlockRenderer.AddFace(face, positionVec, uvOffset, verticesOpaque, indicesOpaque, ref offsetOpaque);
                }
                else if (!WorldGenerator.Instance.TryGetBlockGlobal(neighborPos, out _) && block.IsTransparent())
                {
                    BlockRenderer.AddFace(face, positionVec, uvOffset, verticesTransparent, indicesTransparent, ref offsetWater);
                }

            }

        }

        chunk.OpaqueMesh.Vertices = verticesOpaque.ToArray();
        chunk.OpaqueMesh.Indices = indicesOpaque.ToArray();
        chunk.TransparentMesh.Vertices = verticesTransparent.ToArray();
        chunk.TransparentMesh.Indices = indicesTransparent.ToArray();
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
}
