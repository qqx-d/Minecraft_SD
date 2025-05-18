using minecraft.Sys;
using minecraft.World;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace minecraft.Rendering;

public static class ChunkRenderer
{
    private static readonly Queue<Chunk> ChunksToRender = new();
    
    public static Texture AtlasTexture;
    
    public static void AddToRender(Chunk chunk)
    {
        if(!ChunksToRender.Contains(chunk))
            ChunksToRender.Enqueue(chunk);
    }
    
    public static void DrawAll()
    {
        var chunks = ChunksToRender.ToArray();

        RenderSimpleMesh(chunks);
        RenderTransparentSupportedMesh(chunks);
        
        ChunksToRender.Clear();
    }

    private static void RenderSimpleMesh(Chunk[] chunks)
    {
        foreach (var chunk in chunks)
        {
            if (!chunk.OpaqueMesh.IsMeshUploaded) continue;

            var camera = Window.ActiveCamera;
            var shader = chunk.OpaqueMesh.Shader;
            shader.Use();
            shader.SetMatrix4("view",camera.GetViewMatrix());
            shader.SetMatrix4("projection",camera.GetProjectionMatrix());
            shader.SetVector3("lightDir",   new Vector3(0.5f, -1f, 0.5f));
            shader.SetVector3("lightColor", new Vector3(1f, 1f, 1f));
            shader.SetVector3("ambientColor", new Vector3(0.2f, 0.2f, 0.2f));

            shader.SetVector3("cameraPos", camera.Transform.position);
            
            var model = Matrix4.CreateTranslation(chunk.Position.X, chunk.Position.Y, chunk.Position.Z);
            shader.SetMatrix4("model", model);

            AtlasTexture.Use(TextureUnit.Texture0);
            shader.SetInt("texture0", 0);

            chunk.OpaqueMesh.Draw();
        }
    }
    private static void RenderTransparentSupportedMesh(Chunk[] chunks)
    {
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.DepthMask(false);
        
        var camera = Window.ActiveCamera;
        var cameraPosition = camera.Transform.position;
        var sorted = chunks
            .Where(c => c.TransparentMesh.IsMeshUploaded)
            .OrderByDescending(c =>
            {
                var center = new Vector3(
                    c.Position.X + WorldGenerator.ChunkWidth / 2f,
                    c.Position.Y + WorldGenerator.ChunkHeight / 2f,
                    c.Position.Z + WorldGenerator.ChunkWidth / 2f);
                return (center - cameraPosition).LengthSquared;
            });

        foreach (var chunk in sorted)
        {
            var shader = chunk.TransparentMesh.Shader;
            shader.Use();
            shader.SetMatrix4("view",camera.GetViewMatrix());
            shader.SetMatrix4("projection",camera.GetProjectionMatrix());
            shader.SetVector3("cameraPos", cameraPosition);

            var model = Matrix4.CreateTranslation(chunk.Position.X, chunk.Position.Y, chunk.Position.Z);
            shader.SetMatrix4("model", model);
            
            shader.SetVector3("cameraPos", camera.Transform.position);
            
            AtlasTexture.Use(TextureUnit.Texture0);
            shader.SetInt("texture0", 0);
            
            chunk.TransparentMesh.Draw();
        }

        GL.DepthMask(true);
        GL.Disable(EnableCap.Blend);
    }
}