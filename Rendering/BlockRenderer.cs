using OpenTK.Mathematics;

namespace minecraft.Rendering;

public static class BlockRenderer
{
    private static readonly Vector3[] FrontFace = {
        new(-0.5f, -0.5f, 0.5f),
        new( 0.5f, -0.5f, 0.5f),
        new( 0.5f,  0.5f, 0.5f),
        new(-0.5f,  0.5f, 0.5f)
    };
    private static readonly Vector3[] BackFace = {
        new( 0.5f, -0.5f, -0.5f),
        new(-0.5f, -0.5f, -0.5f),
        new(-0.5f,  0.5f, -0.5f),
        new( 0.5f,  0.5f, -0.5f)
    };
    private static readonly Vector3[] LeftFace = {
        new(-0.5f, -0.5f, -0.5f),
        new(-0.5f, -0.5f,  0.5f),
        new(-0.5f,  0.5f,  0.5f),
        new(-0.5f,  0.5f, -0.5f)
    };
    private static readonly Vector3[] RightFace = {
        new(0.5f, -0.5f, 0.5f),
        new(0.5f, -0.5f, -0.5f),
        new(0.5f, 0.5f, -0.5f),
        new(0.5f, 0.5f, 0.5f)
    };
    private static readonly Vector3[] TopFace = {
        new(-0.5f,  0.5f,  0.5f),
        new( 0.5f,  0.5f,  0.5f),
        new( 0.5f,  0.5f, -0.5f),
        new(-0.5f,  0.5f, -0.5f)
    };
    private static readonly Vector3[] BottomFace = {
        new(-0.5f, -0.5f, -0.5f),
        new(0.5f, -0.5f, -0.5f),
        new(0.5f, -0.5f, 0.5f),
        new(-0.5f, -0.5f, 0.5f)
    };

    public static readonly Vector2[] BaseUVs = {
        new(0, 0),
        new(1, 0),
        new(1, 1),
        new(0, 1)
    };
    
    public static void AddFace(BlockFace face, Vector3 position, Vector2 uvOffset, List<float> vertices, List<uint> indices, ref uint vertexOffset)
    {
        var faceVerts = face switch
        {
            BlockFace.Front  => FrontFace,
            BlockFace.Back   => BackFace,
            BlockFace.Left   => LeftFace,
            BlockFace.Right  => RightFace,
            BlockFace.Top    => TopFace,
            BlockFace.Bottom => BottomFace,
        };

        var (x, y, z) = face switch
        {
            BlockFace.Front  => Vector3.UnitZ,
            BlockFace.Back   => -Vector3.UnitZ,
            BlockFace.Left   => -Vector3.UnitX,
            BlockFace.Right  => Vector3.UnitX,
            BlockFace.Top    => Vector3.UnitY,
            BlockFace.Bottom => -Vector3.UnitY,
        };
        
        for (var i = 0; i < 4; i++)
        {
            var final = faceVerts[i] + position;

            vertices.Add(final.X);
            vertices.Add(final.Y);
            vertices.Add(final.Z);

            vertices.Add(x);
            vertices.Add(y);
            vertices.Add(z);
            
            var uv = uvOffset + BaseUVs[i] * BlockTextureRegistry.TileSize;
            vertices.Add(uv.X);
            vertices.Add(uv.Y);
        }

        indices.Add(vertexOffset + 0);
        indices.Add(vertexOffset + 1);
        indices.Add(vertexOffset + 2);
        indices.Add(vertexOffset + 2);
        indices.Add(vertexOffset + 3);
        indices.Add(vertexOffset + 0);

        vertexOffset += 4;
    }
}