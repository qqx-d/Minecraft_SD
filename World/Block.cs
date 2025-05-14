namespace minecraft;

public class Block(int id)
{
    public static readonly int[] TransparentBlockIds = new []
    {
        0, // air
        6, // water
    };

    public int Id { get; private set; } = id;

    public bool IsTransparent()
    {
        return TransparentBlockIds.Contains(Id);
    }
}