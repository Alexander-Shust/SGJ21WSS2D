using Components;
using Unity.Entities;
using UnityEngine;

namespace Systems
{
    public class BrokenSodaMachineSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<BrokenSodaMachineComponent>();
        }

        protected override void OnUpdate()
        {
            EntityManager.DestroyEntity(GetSingletonEntity<BrokenSodaMachineComponent>());
            var sodaMachineEntity = GetSingletonEntity<SodaMachineComponent>();
            var animator = EntityManager.GetComponentObject<Animator>(sodaMachineEntity);
            animator.SetBool("Broken", true);
        }
    }
}