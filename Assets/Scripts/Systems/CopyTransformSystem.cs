using Components;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    public class CopyTransformSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<CopyTransformComponent>()
                .ForEach((Entity entity, in LocalToWorld localToWorld) =>
                {
                    var transform = EntityManager.GetComponentObject<Transform>(entity);
                    transform.position = localToWorld.Position;
                    transform.rotation = localToWorld.Rotation;
                }).WithoutBurst().Run();
        }
    }
}