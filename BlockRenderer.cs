using OpenTK.Mathematics;
using System.Collections.Generic;

namespace minecraft;

public static class BlockRenderer
{
    
    public const int TextureAltasSize = 16;
    
    public static void AddFace(BlockFace face, Vector3 position, Vector2 uvOffset, List<float> vertices, List<uint> indices, ref uint vertexOffset)
    {
        Vector3[] faceVerts = face switch
        {
            BlockFace.Front  => [new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f)],
            BlockFace.Back   => [new Vector3(0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f)],
            BlockFace.Left   => [new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, -0.5f)],
            BlockFace.Right  => [new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f)],
            BlockFace.Top    => [new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f)],
            BlockFace.Bottom => [new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, 0.5f)],
            _ => throw new ArgumentOutOfRangeException(nameof(face), face, null)
        };

        var (x, y, z) = face switch
        {
            BlockFace.Front  => Vector3.UnitZ,
            BlockFace.Back   => -Vector3.UnitZ,
            BlockFace.Left   => -Vector3.UnitX,
            BlockFace.Right  => Vector3.UnitX,
            BlockFace.Top    => Vector3.UnitY,
            BlockFace.Bottom => -Vector3.UnitY,
            _ => Vector3.Zero
        };

        Vector2[] baseUVs =
        [
            new(0, 0),
            new(1, 0),
            new(1, 1),
            new(0, 1)
        ];

        var tileSize = 1f / TextureAltasSize;

        for (int i = 0; i < 4; i++)
        {
            var final = faceVerts[i] + position;

            vertices.Add(final.X);
            vertices.Add(final.Y);
            vertices.Add(final.Z);

            vertices.Add(x);
            vertices.Add(y);
            vertices.Add(z);

            var uv = new Vector2(baseUVs[i].X * tileSize, baseUVs[i].Y) + uvOffset;
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