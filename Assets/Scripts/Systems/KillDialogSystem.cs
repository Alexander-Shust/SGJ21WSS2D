using Components;
using Unity.Entities;

namespace Systems
{
    public class KillDialogSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            Entities.WithAll<DialogComponent>()
                .WithStructuralChanges()
                .ForEach((Entity dialogEntity, ref DialogComponent dialogComponent) =>
                {
                    var currentLifeTime = dialogComponent.Value;
                    currentLifeTime -= deltaTime;
                    if (currentLifeTime <= 0)
                    {
                        EntityManager.DestroyEntity(dialogEntity);
                        return;
                    }
                    EntityManager.SetComponentData(dialogEntity, new DialogComponent {Value = currentLifeTime});
                }).WithoutBurst().Run();
        }
    }
}