using OpenTK.Mathematics;

namespace minecraft.World;

public static class BlockTextureRegistry
{
    public static Dictionary<int, BlockTextureData> BlockTextures = new();

    public static void Initialize()
    {
        var tileSize = 1f / 16f;
        
        // grass
        BlockTextures[1] = new BlockTextureData
        {
            Top = new Vector2(1, 1) * tileSize,     // grass_top
            Bottom = new Vector2(2, 1) * tileSize,  // dirt
            Side = new Vector2(1, 0) * tileSize     // grass_side
        };
        
        // dirt
        BlockTextures[2] = new BlockTextureData
        {
            Top = new Vector2(2, 1) * tileSize,
            Bottom = new Vector2(2, 1) * tileSize,
            Side = new Vector2(2, 1) * tileSize
        };
        
        // stone
        BlockTextures[3] = new BlockTextureData
        {
            Top = new Vector2(3, 1) * tileSize,
            Bottom = new Vector2(3, 1) * tileSize,
            Side = new Vector2(3, 1) * tileSize
        };
        
        // oak log
        BlockTextures[4] = new BlockTextureData
        {
            Top = new Vector2(4, 0) * tileSize,     // log_top
            Bottom = new Vector2(4, 0) * tileSize,  // log_bottom
            Side = new Vector2(4, 1) * tileSize     // log_side
        };
        
        // leaf
        BlockTextures[5] = new BlockTextureData
        {
            Top = new Vector2(5, 1) * tileSize,
            Bottom = new Vector2(5, 1) * tileSize,
            Side = new Vector2(5, 1) * tileSize
        };
        
        // water
        BlockTextures[6] = new BlockTextureData
        {
            Top = new Vector2(6, 1) * tileSize,
            Bottom = new Vector2(6, 1) * tileSize,
            Side = new Vector2(6, 1) * tileSize
        };
        
        // sand
        BlockTextures[7] = new BlockTextureData
        {
            Top = new Vector2(3, 0) * tileSize,
            Bottom = new Vector2(3, 0) * tileSize,
            Side = new Vector2(3, 0) * tileSize
        };
    }
}