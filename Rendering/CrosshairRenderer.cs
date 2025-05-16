using minecraft.Sys;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace minecraft;

public static class CrosshairRenderer
{
    private static int _vao;
    private static int _vbo;
    private static Shader? _shader;
    private static bool _initialized = false;

    private static readonly float[] _vertices =
    {
        -0.01f, -0.01f,
        0.01f, -0.01f,
        0.01f,  0.01f,
        -0.01f,  0.01f
    };

    private static readonly uint[] _indices =
    {
        0, 1, 2,
        2, 3, 0
    };

    public static void Initialize()
    {
        if (_initialized) return;

        _shader = new Shader("Resources/Shaders/screenquad.vert", "Resources/Shaders/screenquad.frag");

        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        var ebo = GL.GenBuffer();

        GL.BindVertexArray(_vao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.BindVertexArray(0);

        _initialized = true;
    }

    public static void Draw()
    {
        if (!_initialized || _shader == null) return;

        GL.Disable(EnableCap.DepthTest);
        GL.BindVertexArray(_vao);

        _shader.Use();
        var aspect = Window.Size.X / (float)Window.Size.Y;
        var projection = Matrix4.CreateOrthographicOffCenter(-aspect, aspect, -1, 1, -1, 1);
        

        _shader.SetMatrix4("model", Matrix4.Identity);
        _shader.SetMatrix4("view", Matrix4.Identity);
        _shader.SetMatrix4("projection", projection);

        GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

        GL.BindVertexArray(0);
        GL.Enable(EnableCap.DepthTest);
    }
}