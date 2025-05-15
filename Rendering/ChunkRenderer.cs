using minecraft.Sys;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace minecraft;

public static class ChunkRenderer
{
    
    private static readonly Queue<Chunk> _chunksToRender = new();
    
    public static Texture AtlasTexture;
    
    public static void AddToRender(Chunk chunk)
    {
        if(!_chunksToRender.Contains(chunk))
            _chunksToRender.Enqueue(chunk);
    }
    
    public static void RemoveToRender(Chunk chunk)
    {
        if(_chunksToRender.Contains(chunk))
            _chunksToRender.Dequeue();
    }
    
    public static void DrawAll()
    {
        var chunks = _chunksToRender.ToArray();

        RenderSimpleMesh(chunks);
        RenderTransparentSupportedMesh(chunks);
        
        _chunksToRender.Clear();
    }

    private static void RenderSimpleMesh(Chunk[] chunks)
    {
        foreach (var chunk in chunks)
        {
            if (!chunk.Mesh.IsMeshUploaded) continue;

            var shader = chunk.Mesh.Shader;
            shader.Use();
            shader.SetMatrix4("view",       Window.ActiveCamera.GetViewMatrix());
            shader.SetMatrix4("projection", Window.ActiveCamera.GetProjectionMatrix());
            shader.SetVector3("lightDir",   new Vector3(0.5f, -1f, 0.5f));
            shader.SetVector3("lightColor", new Vector3(1f, 1f, 1f));
            shader.SetVector3("ambientColor", new Vector3(0.2f, 0.2f, 0.2f));

            var model = Matrix4.CreateTranslation(chunk.Position.X, chunk.Position.Y, chunk.Position.Z);
            shader.SetMatrix4("model", model);

            AtlasTexture.Use(TextureUnit.Texture0);
            shader.SetInt("texture0", 0);

            chunk.Mesh.Draw();
        }
    }
    private static void RenderTransparentSupportedMesh(Chunk[] chunks)
    {
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.DepthMask(false);
        
        Vector3 camPos = Window.ActiveCamera.transform.position;
        var sorted = chunks
            .Where(c => c.TransparentMesh.IsMeshUploaded)
            .OrderByDescending(c =>
            {
                var center = new Vector3(
                    c.Position.X + WorldGenerator.ChunkWidth / 2f,
                    c.Position.Y + WorldGenerator.ChunkHeight / 2f,
                    c.Position.Z + WorldGenerator.ChunkWidth / 2f);
                return (center - camPos).LengthSquared;
            });

        foreach (var chunk in sorted)
        {
            var shader = chunk.TransparentMesh.Shader;
            shader.Use();
            shader.SetMatrix4("view",       Window.ActiveCamera.GetViewMatrix());
            shader.SetMatrix4("projection", Window.ActiveCamera.GetProjectionMatrix());
            shader.SetVector3("cameraPos",  camPos);

            var model = Matrix4.CreateTranslation(chunk.Position.X, chunk.Position.Y, chunk.Position.Z);
            shader.SetMatrix4("model", model);
            
            AtlasTexture.Use(TextureUnit.Texture0);
            shader.SetInt("texture0", 0);

            chunk.TransparentMesh.Draw();
        }

        GL.DepthMask(true);
        GL.Disable(EnableCap.Blend);
    }
}