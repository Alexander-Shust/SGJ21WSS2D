using Components;
using Physics.Components;
using Physics.Systems;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(StatefulCollisionEventsSystem))]
    public class GirlSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _ecbSystem;
        private StatefulCollisionEventsSystem _collisionSystem;

        protected override void OnCreate()
        {
            RequireSingletonForUpdate<GameSettingsComponent>();
            RequireSingletonForUpdate<ScoreComponent>();
            _ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _collisionSystem = World.GetOrCreateSystem<StatefulCollisionEventsSystem>();
        }

        protected override void OnUpdate()
        {
            var settings = GetSingleton<GameSettingsComponent>();
            var scoreEntity = GetSingletonEntity<ScoreComponent>();
            Dependency = JobHandle.CombineDependencies(_collisionSystem.OutDependency, Dependency);
            var ecb = _ecbSystem.CreateCommandBuffer().AsParallelWriter();
            Entities.WithAll<GirlComponent, StatefulCollisionEvent>()
                .ForEach((Entity girlEntity, int entityInQueryIndex, in Translation translation, in DynamicBuffer<StatefulCollisionEvent> events) =>
                {
                    foreach (var collisionEvent in events)
                    {
                        if (collisionEvent.CollidingState != EventCollidingState.BeginColliding)
                            continue;
                        var playerEntity = collisionEvent.GetOtherEntity(girlEntity);
                        var playerComponent = GetComponent<PlayerComponent>(playerEntity);
                        if (!playerComponent.HasBox)
                            continue;
                        playerComponent.HasBox = false;
                        ecb.SetComponent(entityInQueryIndex, playerEntity, playerComponent);
                        var girlRunEntity = ecb.CreateEntity(entityInQueryIndex);
                        ecb.AddComponent<GirlRunComponent>(entityInQueryIndex, girlRunEntity);
                        ecb.SetComponent(entityInQueryIndex, girlRunEntity, new GirlRunComponent
                        {
                            Value = translation.Value
                        });
                        var currentScore = GetComponent<ScoreComponent>(scoreEntity).Value;
                        ecb.SetComponent(entityInQueryIndex, scoreEntity, new ScoreComponent {Value = currentScore + settings.TrapBonus});
                        break;
                    }
                }).ScheduleParallel();
            _ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}