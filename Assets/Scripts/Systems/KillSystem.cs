using Components;
using Unity.Entities;
using UnityEngine;

namespace Systems
{
    public class KillSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<ToKillComponent>()
                .WithStructuralChanges()
                .ForEach((Entity entity) =>
                {
                    var component = EntityManager.GetComponentObject<Transform>(entity);
                    Object.Destroy(component.gameObject);
                    EntityManager.DestroyEntity(entity);
                }).WithoutBurst().Run();
        }
    }
}