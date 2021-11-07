using Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;

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
                        var bubble = GameObject.FindWithTag("Bubble");
                        bubble.GetComponent<RawImage>().enabled = false;
                        return;
                    }
                    EntityManager.SetComponentData(dialogEntity, new DialogComponent {Value = currentLifeTime});
                }).WithoutBurst().Run();
        }
    }
}