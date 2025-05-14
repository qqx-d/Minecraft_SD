using OpenTK.Mathematics;

namespace minecraft.World;

public class BlockTextureData
{
    public Vector2 Top;
    public Vector2 Bottom;
    public Vector2 Side;

    public Vector2 GetUvForFace(BlockFace face)
    {
        return face switch
        {
            BlockFace.Top => Top,
            BlockFace.Bottom => Bottom,
            _ => Side,
        };
    }
}