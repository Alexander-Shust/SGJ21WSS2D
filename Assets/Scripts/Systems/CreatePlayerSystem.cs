using Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    public class CreatePlayerSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<GameSettingsComponent>();
            RequireSingletonForUpdate<CarArrived>();
            RequireSingletonForUpdate<PlayerIdleComponent>();
            RequireSingletonForUpdate<PrefabCollection>();
        }

        protected override void OnUpdate()
        {
            EntityManager.DestroyEntity(GetSingletonEntity<CarArrived>());
            var settings = GetSingleton<GameSettingsComponent>();
            var carPrefab = this.GetPrefab<CarComponent>();
            if (carPrefab == Entity.Null)
                return;
            var playerEntity = GetSingletonEntity<PlayerIdleComponent>();
            EntityManager.RemoveComponent<PlayerIdleComponent>(playerEntity);
            var renderer = EntityManager.GetComponentObject<SpriteRenderer>(playerEntity);
            renderer.enabled = true;
            EntityManager.SetComponentData(playerEntity, new Translation {Value = settings.PlayerSpawnPoint});
            EntityManager.SetComponentData(playerEntity, new Rotation {Value = quaternion.identity});
            EntityManager.AddComponentData(playerEntity, new PlayerComponent
            {
                MovementSpeed = settings.PlayerSpeed,
                HasBox = false
            });
            var carEntity = EntityManager.Instantiate(carPrefab);
            EntityManager.SetComponentData(carEntity, new Translation {Value = settings.CarStop});
            EntityManager.SetComponentData(carEntity, new Rotation {Value = quaternion.identity});
        }
    }
}