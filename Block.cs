namespace minecraft;

public class Block
{
    public int ID { get; private set; }
    
    public Block(int id)
    {
        ID = id;
    }
    
    public static bool IsTransparent(int id)
    {
        return id == 6;
    }

}