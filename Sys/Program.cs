using minecraft;
using minecraft.Rendering;
using minecraft.Sys;
using minecraft.World;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

public static class Program 
{
    public static void Main()
    {
        using var window = new Window(GameWindowSettings.Default, 
            new NativeWindowSettings 
            { 
                ClientSize = new Vector2i(Window.Width, Window.Height), 
                Title = "Window", 
                Flags = ContextFlags.ForwardCompatible,
            });
        
        ChunkRenderer.AtlasTexture = Texture.LoadFromFile("Resources/Textures/atlas.png");
        BlockTextureRegistry.Initialize();
        
        window.Run();
    }
}