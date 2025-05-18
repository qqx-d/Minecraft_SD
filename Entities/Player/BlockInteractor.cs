using minecraft.Collision;
using minecraft.Sys;
using minecraft.World;
using OpenTK.Mathematics;

namespace minecraft.Entities.Player;

public class BlockInteractor(Player player)
{
    public bool TryBreakBlock()
    {
        var camera = Window.ActiveCamera;
        var rayOrigin = camera.Transform.position;
        var rayDirection = Vector3.Normalize(camera.Transform.forward);

        for (float i = 0; i < 5; i += 0.05f)
        {
            var checkPos = rayOrigin + rayDirection * i;
            var blockPos =  Vector3Int.RoundToInt(checkPos - rayDirection * 0.01f);

            if(WorldGenerator.Instance.TryGetBlockGlobal(blockPos, out var block))
            {
                if (!block.IsPassable())
                {
                    WorldGenerator.Instance.RemoveBlockGlobal(blockPos);

                    var abovePos = blockPos + Vector3Int.Up;
                    if (WorldGenerator.Instance.TryGetBlockGlobal(abovePos, out var aboveBlock) && aboveBlock.Id == 9)
                    {
                        WorldGenerator.Instance.RemoveBlockGlobal(abovePos);
                    }

                    return true;
                }

            }
        }

        return false;
    }

    public bool TryPlaceBlock()
    {
        var camera = Window.ActiveCamera;
        var rayOrigin = camera.Transform.position;
        var rayDirection = Vector3.Normalize(camera.Transform.forward);

        for (float i = 0; i < 5; i += 0.05f)
        {
            var checkPos = rayOrigin + rayDirection * i;
            var blockPos =  Vector3Int.RoundToInt(checkPos);

            WorldGenerator.Instance.TryGetBlockGlobal(blockPos, out var block);

            if (block == null || block.IsPassable())
                continue;

            var prevPoint = checkPos - rayDirection * 0.05f;
            var placePos = Vector3Int.RoundToInt(prevPoint);

            var blockBox = new AABB(
                new Vector3(placePos.X - 0.05f, placePos.Y + 0.5f, placePos.Z - 0.05f),
                Vector3.One
            );

            if (!player.GetAabb().Intersects(blockBox))
            {
                WorldGenerator.Instance.TryGetBlockGlobal(blockPos, out var existing);

                if (existing != null || !block.IsPassable())
                {
                    WorldGenerator.Instance.AddBlockGlobal(placePos, new Block(player.SelectedBlockId));
                    return true;
                }
            }
            break;
        }
        return false;
    }
}
