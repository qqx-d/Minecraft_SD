using mmd_sd.Helpers;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace minecraft;

public class Window : GameWindow
{
    public static int Width { get; private set; } = 1000;
    public static int Height { get; private set; } = 800;
    public static Camera? ActiveCamera { get; private set; }
    public static Player.Player? Player {get; private set; }
    
    private WorldGenerator _worldGenerator;
    private PolygonMode _polygonMode = PolygonMode.Fill;
    private CameraMode _cameraMode = CameraMode.Fp;

    public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
    {
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);

        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        ActiveCamera = new Camera( Width / (float)Height);
        
        ActiveCamera.transform.position = new Vector3(0, 10, 0);
        
        Player = new Player.Player
        {
            Position = new Vector3(0, 15, 0)
        };
        
        CrosshairRenderer.Initialize();
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.ClearColor(0.529f, 0.808f, 0.922f, 1.0f);
        GL.Enable(EnableCap.DepthTest);
        CursorState = CursorState.Grabbed;
        
        _worldGenerator = new WorldGenerator();
    }
    
    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);
        
        if(!IsFocused) return;
        
        Input.UpdateState(KeyboardState, MouseState);
        Physics.ApplyForceToEntities((float) e.Time);
        
        Player.Update((float)e.Time);
        ActiveCamera.transform.position = Player.Position;
        
        _worldGenerator.Update();
        
        HandleInput();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        ChunkRenderer.DrawAll();
        
        CrosshairRenderer.Draw();
        
        SwapBuffers();
    }
    

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        if (ActiveCamera == null) return;
        GL.Viewport(0, 0, Size.X, Size.Y);
        ActiveCamera.AspectRatio = Size.X / (float)Size.Y;
    }
    
    private void HandleInput()
    {
        if (ActiveCamera == null) return;

        if (_cameraMode == CameraMode.Free)
        {
            var direction = ActiveCamera.transform.forward * Input.Vertical + ActiveCamera.transform.right * Input.Horizontal;
            ActiveCamera.Move(direction, 0.1f);
        }
        
        if (Input.GetKey(Keys.F5))
            _polygonMode = _polygonMode == PolygonMode.Fill ? PolygonMode.Line : PolygonMode.Fill;

        ActiveCamera.ProcessMouseMovement(-Input.MouseDelta.X, Input.MouseDelta.Y);
        
        GL.PolygonMode(TriangleFace.FrontAndBack, _polygonMode);
    }
}