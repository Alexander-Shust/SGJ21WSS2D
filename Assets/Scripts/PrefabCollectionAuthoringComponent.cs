using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class PrefabCollectionAuthoringComponent : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public List<GameObject> prefabs = new List<GameObject>();
    
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent<PrefabCollection>(entity);
        var buffer = dstManager.AddBuffer<PrefabBuffer>(entity);
        foreach (var prefab in prefabs)
        {
            var prefabEntity = conversionSystem.GetPrimaryEntity(prefab);
            if (dstManager.Exists(prefabEntity))
            {
                buffer.Add(new PrefabBuffer
                {
                    Value = prefabEntity
                });
            }
        }
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        foreach (var prefab in prefabs)
        {
            referencedPrefabs.Add(prefab.gameObject);
        }
    }
}