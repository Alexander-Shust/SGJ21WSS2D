using Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    public class GirlRunSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<GirlRunComponent>();
            RequireSingletonForUpdate<GameSettingsComponent>();
        }

        protected override void OnUpdate()
        {
            var settings = GetSingleton<GameSettingsComponent>();
            var deltaTime = Time.DeltaTime;
            
            Entities.WithAll<GirlComponent>()
                .WithStructuralChanges()
                .ForEach((Entity girlEntity, ref Translation translation) =>
                {
                    var direction = settings.BatStop - translation.Value;
                    if (Vector3.Magnitude(direction) < 0.2f)
                    {
                        EntityManager.AddComponent<ToKillComponent>(girlEntity);
                        EntityManager.DestroyEntity(GetSingletonEntity<GirlRunComponent>());
                        return;
                    }
                    var delta = settings.BatSpeed * deltaTime;
                    translation.Value += (float3) Vector3.Normalize(direction) * delta;
                }).WithoutBurst().Run();
        }
    }
}