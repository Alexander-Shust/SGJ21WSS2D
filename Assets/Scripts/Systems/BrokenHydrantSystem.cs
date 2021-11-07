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
            EntityManager.RemoveComponent<TrapComponent>(hydrantEntity);
            EntityManager.RemoveComponent<HydrantComponent>(hydrantEntity);
            var animator = EntityManager.GetComponentObject<Animator>(hydrantEntity);
            animator.SetBool("Broken", true);
            var soundManager = GameObject.FindWithTag("AudioSource");
            var hydrantSound = soundManager.GetComponent<HydrantSound>();
            hydrantSound.PlaySound();
            this.ShowDialog("Свежачок! Жаль, посылка промокла.", 1.5f);
        }
    }
}