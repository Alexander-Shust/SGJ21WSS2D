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
            EntityManager.RemoveComponent<TrapComponent>(sodaMachineEntity);
            EntityManager.RemoveComponent<SodaMachineComponent>(sodaMachineEntity);
            var animator = EntityManager.GetComponentObject<Animator>(sodaMachineEntity);
            animator.SetBool("Broken", true);
            var soundManager = GameObject.FindWithTag("AudioSource");
            var failSounds = soundManager.GetComponent<FailSounds>();
            failSounds.PlayRandomSound();
            this.ShowDialog("А-а-а! Посылка-то где?!", 1.5f);
        }
    }
}