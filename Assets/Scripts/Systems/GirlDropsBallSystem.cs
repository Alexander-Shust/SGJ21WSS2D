using Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    public class GirlDropsBallSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<GirlRunComponent>();
            RequireSingletonForUpdate<SpawnBallComponent>();
            EntityManager.CreateEntity(typeof(SpawnBallComponent));
        }

        protected override void OnUpdate()
        {
            EntityManager.DestroyEntity(GetSingletonEntity<SpawnBallComponent>());
            var girlRunEntity = GetSingletonEntity<GirlRunComponent>();
            var girlRunComponent = GetComponent<GirlRunComponent>(girlRunEntity);
            var girlEntity = GetSingletonEntity<GirlComponent>();
            var animator = EntityManager.GetComponentObject<Animator>(girlEntity);
            animator.SetBool("GirlRun", true);
            var ballPrefab = this.GetPrefab<LittleBallComponent>();
            if (ballPrefab == Entity.Null)
                return;
            var boxEntity = EntityManager.Instantiate(ballPrefab);
            EntityManager.SetComponentData(boxEntity, new Translation {Value = girlRunComponent.Value});
            EntityManager.SetComponentData(boxEntity, new Rotation {Value = quaternion.identity});
        }

        public struct SpawnBallComponent : IComponentData
        {
            
        }
    }
}