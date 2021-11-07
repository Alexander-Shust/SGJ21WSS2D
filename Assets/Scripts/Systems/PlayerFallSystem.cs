using Components;
using Unity.Entities;
using UnityEngine;

namespace Systems
{
    public class PlayerFallSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<PlayerFallComponent>();
        }

        protected override void OnUpdate()
        {
            EntityManager.DestroyEntity(GetSingletonEntity<PlayerFallComponent>());
            var soundManager = GameObject.FindWithTag("AudioSource");
            var failSounds = soundManager.GetComponent<FailSounds>();
            failSounds.PlayRandomSound();
            this.ShowDialog("Не дотягиваюсь! А что, если подставить посылку?", 2.0f);
        }
    }
}