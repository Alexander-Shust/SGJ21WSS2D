using Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    public class PlayerMovementSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<PlayerComponent>();
        }

        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            var playerInput = new float2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            
            Entities.WithAll<PlayerComponent>()
                .ForEach((Entity playerEntity, ref Translation translation, in PlayerComponent playerComponent) =>
                {
                    var direction = new float3(playerInput.x, playerInput.y, 0.0f);
                    translation.Value += direction * playerComponent.MovementSpeed * deltaTime;
                }).WithoutBurst().Run();
        }
    }
}