using Components;
using Unity.Entities;
using UnityEngine;

namespace Systems
{
    public class BoxPickupSoundSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<PlayBoxPickupSound>();
        }

        protected override void OnUpdate()
        {
            EntityManager.DestroyEntity(GetSingletonEntity<PlayBoxPickupSound>());
            var soundManager = GameObject.FindWithTag("AudioSource");
            var pickupSounds = soundManager.GetComponent<PickupSounds>();
            pickupSounds.PlayRandomSound();
        }
    }
}