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
                    var position = localToWorld.Position;
                    position.z = 0.0f;
                    transform.position = position;
                    transform.rotation = localToWorld.Rotation;
                }).WithoutBurst().Run();
        }
    }
}