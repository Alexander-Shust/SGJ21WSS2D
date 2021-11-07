using Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    public class BatStealsBoxSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<BoxFlyComponent>();
        }

        protected override void OnUpdate()
        {
            var boxFlyEntity = GetSingletonEntity<BoxFlyComponent>();
            var boxFlyComponent = GetComponent<BoxFlyComponent>(boxFlyEntity);
            var boxPrefab = this.GetPrefab<FlyingBoxComponent>();
            if (boxPrefab == Entity.Null)
                return;
            var soundManager = GameObject.FindWithTag("AudioSource");
            var failSounds = soundManager.GetComponent<FailSounds>();
            failSounds.PlayRandomSound();
            this.ShowDialog("Стой! Это не тебе посылка...", 2.0f);
            var boxEntity = EntityManager.Instantiate(boxPrefab);
            EntityManager.SetComponentData(boxEntity, new Translation {Value = boxFlyComponent.Value});
            EntityManager.SetComponentData(boxEntity, new Rotation {Value = quaternion.identity});
            EntityManager.DestroyEntity(boxFlyEntity);
        }
    }
}