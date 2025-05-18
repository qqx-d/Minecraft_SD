using minecraft.Sys;
using minecraft.World;
using OpenTK.Mathematics;

namespace minecraft.Rendering;

public static class ChunkMeshBuilder
{
    private const float GrassMeshHalfSize = 0.45f;
    private const float GrassMeshHeight = 0.9f;
    
    private static readonly BlockFace[] BlockFaces = Enum.GetValues<BlockFace>();
    
    public static void BuildChunkMesh(Chunk chunk)
    {
        var verticesOpaque = new List<float>();
        var indicesOpaque = new List<uint>();
        var verticesTransparent = new List<float>();
        var indicesTransparent = new List<uint>();

        uint offsetOpaque = 0;
        uint offsetWater  = 0;

        var blocks = chunk.GetAllBlocks();
        var chunkOrigin = chunk.Position;

        foreach (var (localPos, block) in blocks)
        {
            if (block.Id == 0) continue;

            var worldPos = new Vector3Int(
                chunkOrigin.X + localPos.X,
                chunkOrigin.Y + localPos.Y,
                chunkOrigin.Z + localPos.Z
            );
 
            if (block.Id == 9)
            {
                var positionVec = new Vector3(localPos.X, localPos.Y, localPos.Z);
                AddCrossPlaneGrass(positionVec, verticesTransparent, indicesTransparent, ref offsetWater);
                
                continue;
            }
            
            foreach (var face in BlockFaces)
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
                else if (block.IsTransparent())
                {
                    if(!WorldGenerator.Instance.TryGetBlockGlobal(neighborPos, out var neighbor) || neighbor.IsTransparent())
                    {
                        if (neighbor == null || neighbor.Id != block.Id)
                        {
                            BlockRenderer.AddFace(face, positionVec, uvOffset, verticesTransparent, indicesTransparent, ref offsetWater);
                        }
                    }
                }
            }
        }
        
        chunk.OpaqueMesh.Vertices = verticesOpaque.ToArray();
        chunk.OpaqueMesh.Indices = indicesOpaque.ToArray();
        chunk.TransparentMesh.Vertices = verticesTransparent.ToArray();
        chunk.TransparentMesh.Indices = indicesTransparent.ToArray();
    }
    
    private static void AddCrossPlaneGrass(Vector3 pos, List<float> vertices, List<uint> indices, ref uint offset)
    {
        var center = pos - new Vector3(0f, 0.5f, 0f);
        

        Vector3[] quad1 =
        [
            center + new Vector3(-GrassMeshHalfSize, 0, 0),
            center + new Vector3( GrassMeshHalfSize, 0, 0),
            center + new Vector3( GrassMeshHalfSize, GrassMeshHeight, 0),
            center + new Vector3(-GrassMeshHalfSize, GrassMeshHeight, 0)
        ];

        Vector3[] quad2 =
        [
            center + new Vector3(0, 0, -GrassMeshHalfSize),
            center + new Vector3(0, 0,  GrassMeshHalfSize),
            center + new Vector3(0, GrassMeshHeight,  GrassMeshHalfSize),
            center + new Vector3(0, GrassMeshHeight, -GrassMeshHalfSize)
        ];
        
        AddQuad(quad1, BlockRenderer.BaseUVs, vertices, indices, ref offset, 5, 0, BlockTextureRegistry.TileSize);
        AddQuad(quad2, BlockRenderer.BaseUVs, vertices, indices, ref offset, 5, 0, BlockTextureRegistry.TileSize);
    }

    private static void AddQuad(Vector3[] quad, Vector2[] uvs, List<float> verts, List<uint> inds, ref uint offset, int tileX, int tileY, float tileSize)
    {
        for (var i = 0; i < 4; i++)
        {
            verts.Add(quad[i].X);
            verts.Add(quad[i].Y);
            verts.Add(quad[i].Z);

            verts.Add(0); verts.Add(1); verts.Add(0);

            verts.Add((tileX + uvs[i].X) * tileSize);
            verts.Add((tileY + uvs[i].Y) * tileSize);
        }

        inds.Add(offset + 0);
        inds.Add(offset + 1);
        inds.Add(offset + 2);
        inds.Add(offset + 2);
        inds.Add(offset + 3);
        inds.Add(offset + 0);

        offset += 4;
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
        };
    }
}
