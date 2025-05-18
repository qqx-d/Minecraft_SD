namespace minecraft.World;

public class Block(int id)
{
    private readonly int[] _transparencyBlockIds = [0, 6, 9, 5];

    private readonly int[] _passableBlockIds = [0, 6, 9];
    
    public int Id { get; private set; } = id;

    public bool IsTransparent()
    {
        return _transparencyBlockIds.Contains(Id);
    }

    public bool IsPassable()
    {
        return _passableBlockIds.Contains(Id);
    }
}