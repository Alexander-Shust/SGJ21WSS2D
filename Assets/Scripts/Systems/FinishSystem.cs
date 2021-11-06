using Components;
using Physics.Components;
using Physics.Systems;
using Unity.Entities;
using Unity.Jobs;

namespace Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(StatefulCollisionEventsSystem))]
    public class FinishSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _ecbSystem;
        private StatefulCollisionEventsSystem _collisionSystem;

        protected override void OnCreate()
        {
            _ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _collisionSystem = World.GetOrCreateSystem<StatefulCollisionEventsSystem>();
        }

        protected override void OnUpdate()
        {
            Dependency = JobHandle.CombineDependencies(_collisionSystem.OutDependency, Dependency);
            var ecb = _ecbSystem.CreateCommandBuffer().AsParallelWriter();
            Entities.WithAll<FinishComponent, StatefulCollisionEvent>()
                .ForEach((Entity finishEntity, int entityInQueryIndex, in DynamicBuffer<StatefulCollisionEvent> events) =>
                {
                    foreach (var collisionEvent in events)
                    {
                        if (collisionEvent.CollidingState != EventCollidingState.BeginColliding)
                            continue;
                        var playerEntity = collisionEvent.GetOtherEntity(finishEntity);
                        var playerComponent = GetComponent<PlayerComponent>(playerEntity);
                        if (!playerComponent.HasBox)
                            continue;
                        playerComponent.HasBox = false;
                        ecb.SetComponent(entityInQueryIndex, playerEntity, playerComponent);
                        ecb.AddComponent<ToKillComponent>(entityInQueryIndex, finishEntity);
                        var newEntity = ecb.CreateEntity(entityInQueryIndex);
                        ecb.AddComponent<GameOverComponent>(entityInQueryIndex, newEntity);
                        break;
                    }
                }).ScheduleParallel();
            _ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}