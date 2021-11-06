using Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    public class BatFlySystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<BatFlyComponent>();
            RequireSingletonForUpdate<GameSettingsComponent>();
        }

        protected override void OnUpdate()
        {
            var settings = GetSingleton<GameSettingsComponent>();
            var deltaTime = Time.DeltaTime;
            
            Entities.WithAll<BatComponent>()
                .WithStructuralChanges()
                .ForEach((Entity batEntity, ref Translation translation) =>
                {
                    var direction = settings.BatStop - translation.Value;
                    if (Vector3.Magnitude(direction) < 0.2f)
                    {
                        EntityManager.AddComponent<ToKillComponent>(batEntity);
                        return;
                    }
                    var delta = settings.BatSpeed * deltaTime;
                    translation.Value += (float3) Vector3.Normalize(direction) * delta;
                }).WithoutBurst().Run();
        }
    }
}