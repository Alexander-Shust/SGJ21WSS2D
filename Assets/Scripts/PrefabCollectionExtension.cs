using Unity.Entities;

public static class PrefabCollectionExtension
{
    public static Entity GetPrefab<T>(this SystemBase system) where T : IComponentData
    {
        var collectionEntity = system.GetSingletonEntity<PrefabCollection>();
        var prefabs = system.EntityManager.GetBuffer<PrefabBuffer>(collectionEntity);
        var result = Entity.Null;
        for (var i = 0; i < prefabs.Length; ++i)
        {
            var prefabI = prefabs[i].Value;
            if (system.EntityManager.HasComponent<T>(prefabI))
            {
                result = prefabI;
                break;
            }
        }
        return result;
    }

    public static void SetNameForEditor(this EntityManager manager, Entity entity, string name)
    {
#if UNITY_EDITOR
        manager.SetName(entity, name);
#endif
    }
}