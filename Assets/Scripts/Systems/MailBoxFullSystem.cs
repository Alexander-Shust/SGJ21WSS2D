using Components;
using Unity.Entities;

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
            
            this.ShowDialog("Работа мастера, впихнули невпихуемое! Продолжаем разговор...", 2.5f);
        }
    }
}