using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class DialogConverter : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        if (TryGetComponent<Text>(out var text))
        {
            conversionSystem.AddHybridComponent(text);
        }
        if (TryGetComponent<RectTransform>(out var transform))
        {
            conversionSystem.AddHybridComponent(transform);
        }
        if (TryGetComponent<CanvasRenderer>(out var renderer))
        {
            conversionSystem.AddHybridComponent(renderer);
        }
    }
}