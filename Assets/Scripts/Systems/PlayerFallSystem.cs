using Components;
using Unity.Entities;

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
            
            this.ShowDialog("Не дотягиваюсь! А что, если подставить посылку?", 2.0f);
        }
    }
}