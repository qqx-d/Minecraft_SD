using minecraft.Entities;
using minecraft.Entities.Player;
using minecraft.World;
using mmd_sd.Helpers;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace minecraft.Sys;

public class Window : GameWindow
{

    public static Vector2i Size = new(1000, 800);
    public static int Width => Size.X;
    public static int Height => Size.Y;

    public static Camera? ActiveCamera { get; private set; }
    public static Player? Player { get; private set; }

    private WorldGenerator _worldGenerator;

    private ScreenMode _screenMode = ScreenMode.Windowed;
    private PolygonMode _polygonMode = PolygonMode.Fill;
    private CameraMode _cameraMode = CameraMode.Fp;


    public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(
        gameWindowSettings, nativeWindowSettings)
    {
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);

        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        InitializeObjects();
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.ClearColor(0.529f, 0.808f, 0.922f, 1.0f);
        GL.Enable(EnableCap.DepthTest);
        CursorState = CursorState.Grabbed;

        _worldGenerator = new WorldGenerator();
        _worldGenerator.GenerateInitialChunks();
    }
    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        if (!IsFocused) return;

        Time.DeltaTime = (float) e.Time;
        
        Input.UpdateState(KeyboardState, MouseState);
        
        Physics.ApplyForceToEntities();
        EntityProcessor.UpdateProcessor();
        
        HandleInputs();
        _worldGenerator.Update();
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
        
        GL.Viewport(0, 0, e.Size.X, e.Size.Y);

        if (ActiveCamera != null)
        {
            ActiveCamera.AspectRatio = (e.Size.X / (float) e.Size.Y);
        }
        
        Size = new Vector2i(e.Size.X, e.Size.Y);
    }

    private void InitializeObjects()
    {
        ActiveCamera = new Camera(Size.X / (float)Size.Y)
        {
            transform =
            {
                position = new Vector3(0, 300, 0)
            }
        };

        Player = new Player
        {
            Position = new Vector3(0, 200, 0)
        };

        CrosshairRenderer.Initialize();
    }

    private void HandleInputs()
    {
        TogglePolygonMode();
        ToggleScreenMode();
        HandleCameraInput();
    }

    private void HandleCameraInput()
    {
        if (ActiveCamera == null) return;

        if (Input.GetKeyDown(Keys.F3))
        {
            _cameraMode = _cameraMode == CameraMode.Fp ? CameraMode.Free : CameraMode.Fp;
        }

        if(_cameraMode == CameraMode.Free)
        {
            var direction = ActiveCamera.transform.forward * Input.Vertical +
                                   ActiveCamera.transform.right * Input.Horizontal;
            ActiveCamera.Move(direction, 0.1f);
        }
        else if (_cameraMode == CameraMode.Fp)
        {
            ActiveCamera.transform.position = Player.Position;
        }

        ActiveCamera.ProcessMouseMovement(-Input.MouseDelta.X, Input.MouseDelta.Y);
    }

    private void TogglePolygonMode()
    {
        if (Input.GetKeyDown(Keys.F5))
        {
            _polygonMode = _polygonMode == PolygonMode.Fill ? PolygonMode.Line : PolygonMode.Fill;
        }
        
        GL.PolygonMode(TriangleFace.FrontAndBack, _polygonMode);
    }
    private void ToggleScreenMode()
    {
        if (Input.GetKeyDown(Keys.F11))
        {
            _screenMode = _screenMode == ScreenMode.Windowed ? ScreenMode.Fullscreen : ScreenMode.Windowed;

            if (_screenMode == ScreenMode.Fullscreen)
            {
                var monitor = Monitors.GetPrimaryMonitor();
                var mode = monitor.CurrentVideoMode;
                WindowState = WindowState.Fullscreen;
                WindowBorder = WindowBorder.Hidden;
                CurrentMonitor = monitor;
            }
            else
            {
                WindowBorder = WindowBorder.Resizable;
                WindowState = WindowState.Normal;
                ((NativeWindow)this).Size = Size;
                CenterWindow();
            }
        }
    }
}