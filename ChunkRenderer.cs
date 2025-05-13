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
        while (_chunksToRender.Count > 0)
        {
            var chunk = _chunksToRender.Dequeue();
            
            if (!chunk.Mesh.IsMeshUploaded) continue;
            
            chunk.Mesh.Shader.Use();
            chunk.Mesh.Shader.SetMatrix4("view", Window.ActiveCamera.GetViewMatrix());
            chunk.Mesh.Shader.SetMatrix4("projection", Window.ActiveCamera.GetProjectionMatrix());
            chunk.Mesh.Shader.SetVector3("lightDir", new Vector3(0.5f, -1, 0.5f));
            chunk.Mesh.Shader.SetVector3("lightColor", new Vector3(1, 1, 1));
            chunk.Mesh.Shader.SetVector3("ambientColor", new Vector3(0.4f, 0.4f, 0.4f));
            
            var model = Matrix4.CreateTranslation(chunk.Position.X, chunk.Position.Y, chunk.Position.Z);
            chunk.Mesh.Shader.SetMatrix4("model", model);
            
            AtlasTexture.Use(TextureUnit.Texture0);
            chunk.Mesh.Shader.SetInt("texture0", 0);
     
            chunk.Mesh.Draw();
        }
    }
}