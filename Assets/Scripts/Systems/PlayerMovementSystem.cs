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
            var x = Input.GetAxis("Horizontal");
            var y = Input.GetAxis("Vertical");
            
            Entities.WithAll<PlayerComponent>()
                .ForEach((Entity playerEntity, ref Translation translation, in PlayerComponent playerComponent) =>
                {
                    var animator = EntityManager.GetComponentObject<Animator>(playerEntity);
                    var hasBox = animator.GetBool("HasBox");
                    if (hasBox != playerComponent.HasBox)
                        animator.SetBool("HasBox", !hasBox);
                    var current = animator.GetInteger("Current");
                    var newPos = 0;
                    if (x > float.Epsilon)
                    {
                        newPos = 2;
                    }
                    else if (x < -float.Epsilon)
                    {
                        newPos = 4;
                    }
                    else if (y > float.Epsilon)
                    {
                        newPos = 3;
                    }
                    else if (y < -float.Epsilon)
                    {
                        newPos = 1;
                    }
                    else if (current != 0)
                    {
                        animator.SetInteger("Previous", current);
                        animator.SetInteger("Current", 0);
                    }
                    if (newPos != 0 && newPos != current)
                    {
                        animator.SetInteger("Previous", current);
                        animator.SetInteger("Current", newPos);
                    }
                    var direction = new float3(x, y, 0.0f);
                    translation.Value += direction * playerComponent.MovementSpeed * deltaTime;
                }).WithoutBurst().Run();
        }
    }
}