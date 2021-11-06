using Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    public class BatStealsBoxSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<BoxFlyComponent>();
        }

        protected override void OnUpdate()
        {
            var boxFlyEntity = GetSingletonEntity<BoxFlyComponent>();
            var boxFlyComponent = GetComponent<BoxFlyComponent>(boxFlyEntity);
            var boxPrefab = this.GetPrefab<FlyingBoxComponent>();
            if (boxPrefab == Entity.Null)
                return;
            var boxEntity = EntityManager.Instantiate(boxPrefab);
            EntityManager.SetComponentData(boxEntity, new Translation {Value = boxFlyComponent.Value});
            EntityManager.SetComponentData(boxEntity, new Rotation {Value = quaternion.identity});
            EntityManager.DestroyEntity(boxFlyEntity);
        }
    }
}