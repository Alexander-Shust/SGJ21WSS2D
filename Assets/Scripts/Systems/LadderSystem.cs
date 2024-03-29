﻿using Components;
using Physics.Components;
using Physics.Systems;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Transforms;

namespace Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(StatefulCollisionEventsSystem))]
    public class LadderSystem : SystemBase
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
            Entities.WithAll<LadderComponent, StatefulCollisionEvent>()
                .ForEach((Entity ladderEntity, int entityInQueryIndex, in Translation translation, in DynamicBuffer<StatefulCollisionEvent> events) =>
                {
                    foreach (var collisionEvent in events)
                    {
                        if (collisionEvent.CollidingState != EventCollidingState.BeginColliding)
                            continue;
                        var playerEntity = collisionEvent.GetOtherEntity(ladderEntity);
                        var playerComponent = GetComponent<PlayerComponent>(playerEntity);
                        if (!playerComponent.HasBox)
                            continue;
                        playerComponent.HasBox = false;
                        ecb.SetComponent(entityInQueryIndex, playerEntity, playerComponent);
                        ecb.RemoveComponent<LadderComponent>(entityInQueryIndex, ladderEntity);
                        ecb.RemoveComponent<TrapComponent>(entityInQueryIndex, ladderEntity);
                        var newEntity = ecb.CreateEntity(entityInQueryIndex);
                        ecb.AddComponent<PlayerFallComponent>(entityInQueryIndex, newEntity);
                        ecb.RemoveComponent<PhysicsCollider>(entityInQueryIndex, ladderEntity);
                        var currentScore = GetComponent<ScoreComponent>(scoreEntity).Value;
                        ecb.SetComponent(entityInQueryIndex, scoreEntity, new ScoreComponent {Value = currentScore + settings.TrapBonus});
                        break;
                    }
                }).ScheduleParallel();
            _ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}