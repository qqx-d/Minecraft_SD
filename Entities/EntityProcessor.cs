using minecraft.Sys;

namespace minecraft.Entities;

public static class EntityProcessor
{
    private static List<Entity> Entities { get; } = [];
    public static Entity[] GetEntities() => Entities.ToArray();
    
    public static void AddEntity(Entity entity)
    {
        Entities.Add(entity);
    }

    public static void RemoveEntity(Entity entity)
    {
        Entities.Remove(entity);
    }

    public static void UpdateProcessor()
    {
        for (int i = 0; i < Entities.Count; i++)
        {
            Entities[i].Update(Time.DeltaTime);
        }
    }
}