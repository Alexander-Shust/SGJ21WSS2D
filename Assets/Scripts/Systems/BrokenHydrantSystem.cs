using Components;
using Unity.Entities;
using UnityEngine;

namespace Systems
{
    public class BrokenHydrantSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<BrokenHydrantComponent>();
        }

        protected override void OnUpdate()
        {
            EntityManager.DestroyEntity(GetSingletonEntity<BrokenHydrantComponent>());
            var hydrantEntity = GetSingletonEntity<HydrantComponent>();
            var animator = EntityManager.GetComponentObject<Animator>(hydrantEntity);
            animator.SetBool("Broken", true);
        }
    }
}