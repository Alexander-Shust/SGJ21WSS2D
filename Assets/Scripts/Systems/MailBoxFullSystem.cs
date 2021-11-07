using Components;
using Unity.Entities;
using UnityEngine;

namespace Systems
{
    public class MailBoxFullSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<MailBoxFullComponent>();
        }

        protected override void OnUpdate()
        {
            EntityManager.DestroyEntity(GetSingletonEntity<MailBoxFullComponent>());
            var soundManager = GameObject.FindWithTag("AudioSource");
            var failSounds = soundManager.GetComponent<FailSounds>();
            failSounds.PlayRandomSound();
            this.ShowDialog("Впихнул! Стоп, это чужой ящик, а назад уже никак...", 2.5f);
        }
    }
}