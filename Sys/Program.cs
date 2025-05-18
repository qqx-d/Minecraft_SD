using minecraft.Rendering;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace minecraft.Sys;

public static class Program 
{
    public static void Main()
    {
        using var window = new Window(GameWindowSettings.Default, 
            new NativeWindowSettings 
            { 
                ClientSize = new Vector2i(1000, 800), 
                Title = "Window", 
                Flags = ContextFlags.ForwardCompatible,
            });
        
        ChunkRenderer.AtlasTexture = Texture.LoadFromFile("Resources/Textures/atlas.png");
        BlockTextureRegistry.Initialize();
        
        window.Run();
    }
}