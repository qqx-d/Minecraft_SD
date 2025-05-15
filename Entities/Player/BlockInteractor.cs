using minecraft.Entities.Player;
using minecraft.Sys;
using OpenTK.Mathematics;

namespace minecraft;

public class BlockInteractor
{
    
    private readonly Player _player;

    public BlockInteractor(Player player)
    {
        _player = player;
    }

    public bool TryBreakBlock()
    {
        var rayOrigin = Window.ActiveCamera.transform.position;
        var rayDirection = Vector3.Normalize(Window.ActiveCamera.transform.forward);

        for (float i = 0; i < 5; i += 0.05f)
        {
            var checkPos = rayOrigin + rayDirection * i;
            var blockPos = WorldToBlock(checkPos - rayDirection * 0.01f);

            if(WorldGenerator.Instance.TryGetBlockGlobal(blockPos, out var block))
            {
                if(!block.IsTransparent())
                {
                    WorldGenerator.Instance.RemoveBlockGlobal(blockPos);
                    return true;
                }
            }
        }
        return false;
    }

    public bool TryPlaceBlock()
    {
        var rayOrigin = Window.ActiveCamera.transform.position;
        var rayDirection = Vector3.Normalize(Window.ActiveCamera.transform.forward);

        for (float i = 0; i < 5; i += 0.05f)
        {
            var checkPos = rayOrigin + rayDirection * i;
            var blockPos = WorldToBlock(checkPos);

            WorldGenerator.Instance.TryGetBlockGlobal(blockPos, out var block);
            
            if (block == null || block.IsTransparent()) continue;
     
            var prevPoint = checkPos - rayDirection * 0.05f;
            var placePos = WorldToBlock(prevPoint);

            var blockBox = new AABB(
                new Vector3(placePos.X - 0.05f, placePos.Y + 0.5f, placePos.Z - 0.05f),
                Vector3.One
            );

            if(!_player.GetAabb().Intersects(blockBox))
            {
                WorldGenerator.Instance.TryGetBlockGlobal(blockPos, out var existing);
                
                if(existing != null || block.IsTransparent())
                {
                    WorldGenerator.Instance.AddBlockGlobal(placePos, new Block(_player.SelectedBlockID));
                    return true;
                }
            }
            break;
        }
        return false;
    }

    private Vector3Int WorldToBlock(Vector3 pos)
    {
        return new Vector3Int(
            (int)MathF.Round(pos.X),
            (int)MathF.Round(pos.Y),
            (int)MathF.Round(pos.Z)
        );
    }
}
