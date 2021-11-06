using Physics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Rendering;

namespace Physics.Systems
{
    // This system converts stream of CollisionEvents to StatefulCollisionEvents that are stored in a Dynamic Buffer.
    // In order for CollisionEvents to be transformed to StatefulCollisionEvents and stored in a Dynamic Buffer, it is required to:
    //    1) Tick Raises Collision Events on PhysicsShapeAuthoring on the entity that should raise collision events
    //    2) Add a DynamicBufferCollisionEventAuthoring component to that entity (and select if details should be calculated or not)
    //    3) If this is desired on a Character Controller, tick RaiseCollisionEvents flag on CharacterControllerAuthoring (skip 1) and 2)),

    // This system converts stream of TriggerEvents to StatefulTriggerEvents that are stored in a Dynamic Buffer.
    // In order for TriggerEvents to be transformed to StatefulTriggerEvents and stored in a Dynamic Buffer, it is required to:
    //    1) Tick IsTrigger on PhysicsShapeAuthoring on the entity that should raise trigger events
    //    2) Add a DynamicBufferTriggerEventAuthoring component to that entity
    //    3) If this is desired on a Character Controller, tick RaiseTriggerEvents on CharacterControllerAuthoring (skip 1) and 2)),
    //    note that Character Controller will not become a trigger, it will raise events when overlapping with one

    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(StepPhysicsWorld))]
    [UpdateBefore(typeof(EndFramePhysicsSystem))]
    public class StatefulTriggerEventsSystem : SystemBase
    {
        public JobHandle OutDependency => Dependency;

        private StepPhysicsWorld _stepPhysicsWorld;
        private BuildPhysicsWorld _buildPhysicsWorld;
        private EndFramePhysicsSystem _endFramePhysicsSystem;
        private EntityQuery _query;

        private NativeList<StatefulTriggerEvent> _previousFrameTriggerEvents;
        private NativeList<StatefulTriggerEvent> _currentFrameTriggerEvents;

        protected override void OnCreate()
        {
            _stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
            _buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
            _endFramePhysicsSystem = World.GetOrCreateSystem<EndFramePhysicsSystem>();
            _query = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(StatefulTriggerEvent)
                },
                None = new ComponentType[]
                {
                    typeof(ExcludeFromTriggerEventConversion)
                }
            });

            _previousFrameTriggerEvents = new NativeList<StatefulTriggerEvent>(Allocator.Persistent);
            _currentFrameTriggerEvents = new NativeList<StatefulTriggerEvent>(Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            _previousFrameTriggerEvents.Dispose();
            _currentFrameTriggerEvents.Dispose();
        }

        protected void SwapTriggerEventStates()
        {
            (_previousFrameTriggerEvents, _currentFrameTriggerEvents) = (_currentFrameTriggerEvents, _previousFrameTriggerEvents);
            _currentFrameTriggerEvents.Clear();
        }

        protected static void AddTriggerEventsToDynamicBuffers(NativeList<StatefulTriggerEvent> triggerEventList,
            ref BufferFromEntity<StatefulTriggerEvent> bufferFromEntity,
            NativeHashSet<Entity> entitiesWithTriggerBuffers)
        {
            foreach (var triggerEvent in triggerEventList)
            {
                if (entitiesWithTriggerBuffers.Contains(triggerEvent.EntityA))
                {
                    bufferFromEntity[triggerEvent.EntityA].Add(triggerEvent);
                }

                if (entitiesWithTriggerBuffers.Contains(triggerEvent.EntityB))
                {
                    bufferFromEntity[triggerEvent.EntityB].Add(triggerEvent);
                }
            }
        }

        public static void UpdateTriggerEventState(NativeList<StatefulTriggerEvent> previousFrameTriggerEvents,
            NativeList<StatefulTriggerEvent> currentFrameTriggerEvents,
            NativeList<StatefulTriggerEvent> resultList)
        {
            var i = 0;
            var j = 0;

            while (i < currentFrameTriggerEvents.Length && j < previousFrameTriggerEvents.Length)
            {
                var currentFrameTriggerEvent = currentFrameTriggerEvents[i];
                var previousFrameTriggerEvent = previousFrameTriggerEvents[j];

                var cmpResult = currentFrameTriggerEvent.CompareTo(previousFrameTriggerEvent);

                //Appears in previous, and current frame, mark it as Stay
                if (cmpResult == 0)
                {
                    currentFrameTriggerEvent.State = EventOverlapState.Overlapping;
                    resultList.Add(currentFrameTriggerEvent);
                    ++i;
                    ++j;
                }
                else if (cmpResult < 0)
                {
                    //Appears in current, but not in previous, mark it as Enter
                    currentFrameTriggerEvent.State = EventOverlapState.BeginOverlapping;
                    resultList.Add(currentFrameTriggerEvent);
                    ++i;
                }
                else
                {
                    // appears in previous, but not in current, mark it as Exit
                    previousFrameTriggerEvent.State = EventOverlapState.EndOverlapping;
                    resultList.Add(previousFrameTriggerEvent);
                    ++j;
                }
            }

            if (i == currentFrameTriggerEvents.Length)
            {
                while (j < previousFrameTriggerEvents.Length)
                {
                    var triggerEvent = previousFrameTriggerEvents[j++];
                    triggerEvent.State = EventOverlapState.EndOverlapping;
                    resultList.Add(triggerEvent);
                }
            }
            else if (j == previousFrameTriggerEvents.Length)
            {
                while (i < currentFrameTriggerEvents.Length)
                {
                    var triggerEvent = currentFrameTriggerEvents[i++];
                    triggerEvent.State = EventOverlapState.BeginOverlapping;
                    resultList.Add(triggerEvent);
                }
            }
        }

        protected override void OnUpdate()
        {
            if (_query.CalculateEntityCount() == 0)
                return;

            Dependency = JobHandle.CombineDependencies(_stepPhysicsWorld.FinalSimulationJobHandle, Dependency);

            Entities
                .WithName("ClearTriggerEventDynamicBuffersJobParallel")
                .WithBurst()
                .WithNone<ExcludeFromTriggerEventConversion>()
                .ForEach((ref DynamicBuffer<StatefulTriggerEvent> buffer) => { buffer.Clear(); }).ScheduleParallel();

            SwapTriggerEventStates();

            var currentFrameTriggerEvents = _currentFrameTriggerEvents;
            var previousFrameTriggerEvents = _previousFrameTriggerEvents;

            var triggerEventsBufferFromEntity = GetBufferFromEntity<StatefulTriggerEvent>();
            var physicsWorld = _buildPhysicsWorld.PhysicsWorld;

            var collectTriggerEventsJob = new CollectTriggerEventsJob
            {
                TriggerEvents = currentFrameTriggerEvents
            };

            var collectJobHandle =
                collectTriggerEventsJob.Schedule(_stepPhysicsWorld.Simulation, ref physicsWorld, Dependency);

            var entitiesWithBufferSet = new NativeHashSet<Entity>(0, Allocator.TempJob);

            var collectTriggerBuffersHandle = Entities
                .WithName("CollectTriggerBufferJob")
                .WithBurst()
                .WithNone<ExcludeFromTriggerEventConversion>()
                .ForEach((Entity e, ref DynamicBuffer<StatefulTriggerEvent> buffer) =>
                {
                    entitiesWithBufferSet.Add(e);
                }).Schedule(Dependency);

            Dependency = JobHandle.CombineDependencies(collectJobHandle, collectTriggerBuffersHandle);

            Job.WithName("ConverTriggerEventsStreamToDynamicBufferJob")
                .WithBurst()
                .WithCode(() =>
                {
                    currentFrameTriggerEvents.Sort();

                    var triggerEventsWithState =
                        new NativeList<StatefulTriggerEvent>(currentFrameTriggerEvents.Length, Allocator.Temp);

                    UpdateTriggerEventState(previousFrameTriggerEvents, currentFrameTriggerEvents,
                        triggerEventsWithState);
                    AddTriggerEventsToDynamicBuffers(triggerEventsWithState,
                        ref triggerEventsBufferFromEntity,
                        entitiesWithBufferSet);
                }).Schedule();

            _endFramePhysicsSystem.AddInputDependency(Dependency);
            entitiesWithBufferSet.Dispose(Dependency);
        }

        [BurstCompile]
        public struct CollectTriggerEventsJob : ITriggerEventsJob
        {
            public NativeList<StatefulTriggerEvent> TriggerEvents;

            public void Execute(TriggerEvent triggerEvent)
            {
                TriggerEvents.Add(new StatefulTriggerEvent(
                    triggerEvent.EntityA,
                    triggerEvent.EntityB,
                    triggerEvent.BodyIndexA,
                    triggerEvent.BodyIndexB,
                    triggerEvent.ColliderKeyA,
                    triggerEvent.ColliderKeyB)
                );
            }
        }
    }
}