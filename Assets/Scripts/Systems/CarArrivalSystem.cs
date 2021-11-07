using Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    public class CarArrivalSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<CarOnTheWay>();
            RequireSingletonForUpdate<CarComponent>();
            RequireSingletonForUpdate<GameSettingsComponent>();
            EntityManager.CreateEntity(typeof(CarOnTheWay));
        }

        protected override void OnUpdate()
        {
            var settings = GetSingleton<GameSettingsComponent>();
            var deltaTime = Time.DeltaTime;
            
            Entities.WithAll<CarComponent>()
                .WithStructuralChanges()
                .ForEach((Entity carEntity, ref Translation translation, in CarComponent carComponent) =>
                {
                    var direction = settings.CarStop - translation.Value;
                    if (Vector3.Magnitude(direction) < 0.2f)
                    {
                        EntityManager.DestroyEntity(GetSingletonEntity<CarOnTheWay>());
                        EntityManager.CreateEntity(typeof(CarArrived));
                        EntityManager.AddComponent<ToKillComponent>(carEntity);
                        this.ShowDialog("Приехали!");
                        return;
                    }
                    var delta = carComponent.MovementSpeed * deltaTime;
                    translation.Value += (float3) Vector3.Normalize(direction) * delta;
                }).WithoutBurst().Run();
        }

        public struct CarOnTheWay : IComponentData
        {
            
        }
    }
}