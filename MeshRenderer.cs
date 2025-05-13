using OpenTK.Graphics.OpenGL;

namespace minecraft;

public class MeshRenderer
{
    private int _vao;
    private int _vbo;
    private int _ebo;
    private int _indexCount;

    public bool IsMeshUploaded { get; private set; } = false;
    public Shader Shader { get; private set; }

    public MeshRenderer()
    {
        Shader = new Shader("Resources/Shaders/shader.vert", "Resources/Shaders/shader.frag");
    }
    
    public void Upload(float[] vertices, uint[] indices)
    {
        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        _ebo = GL.GenBuffer();

        GL.BindVertexArray(_vao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

        var stride = 8 * sizeof(float);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));
        GL.EnableVertexAttribArray(2);

        _indexCount = indices.Length;
        
        IsMeshUploaded = true;
    }

    public void Draw()
    {
        if (_indexCount == 0) return;

        GL.BindVertexArray(_vao);
        GL.DrawElements(PrimitiveType.Triangles, _indexCount, DrawElementsType.UnsignedInt, 0);
    }
}