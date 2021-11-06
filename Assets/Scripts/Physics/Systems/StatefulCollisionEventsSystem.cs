using Physics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;

namespace Physics.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(StepPhysicsWorld))]
    [UpdateBefore(typeof(EndFramePhysicsSystem))]
    public class StatefulCollisionEventsSystem : SystemBase
    {
        public JobHandle OutDependency => Dependency;

        private StepPhysicsWorld _stepPhysicsWorld;
        private BuildPhysicsWorld _buildPhysicsWorld;
        private EndFramePhysicsSystem _endFramePhysicsSystem;
        private EntityQuery _query;

        private NativeList<StatefulCollisionEvent> _previousFrameCollisionEvents;
        private NativeList<StatefulCollisionEvent> _currentFrameCollisionEvents;

        protected override void OnCreate()
        {
            _stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
            _buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
            _endFramePhysicsSystem = World.GetOrCreateSystem<EndFramePhysicsSystem>();
            _query = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(CollisionEventBuffer)
                }
            });

            _previousFrameCollisionEvents = new NativeList<StatefulCollisionEvent>(Allocator.Persistent);
            _currentFrameCollisionEvents = new NativeList<StatefulCollisionEvent>(Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            _previousFrameCollisionEvents.Dispose();
            _currentFrameCollisionEvents.Dispose();
        }

        protected void SwapCollisionEventState()
        {
            (_previousFrameCollisionEvents, _currentFrameCollisionEvents) = (_currentFrameCollisionEvents, _previousFrameCollisionEvents);
            _currentFrameCollisionEvents.Clear();
        }

        public static void UpdateCollisionEventState(NativeList<StatefulCollisionEvent> previousFrameCollisionEvents,
            NativeList<StatefulCollisionEvent> currentFrameCollisionEvents, NativeList<StatefulCollisionEvent> resultList)
        {
            var i = 0;
            var j = 0;

            while (i < currentFrameCollisionEvents.Length && j < previousFrameCollisionEvents.Length)
            {
                var currentFrameCollisionEvent = currentFrameCollisionEvents[i];
                var previousFrameCollisionEvent = previousFrameCollisionEvents[j];

                var cmpResult = currentFrameCollisionEvent.CompareTo(previousFrameCollisionEvent);
                
                // Appears in previous, and current frame, mark it as Colliding
                if (cmpResult == 0)
                {
                    currentFrameCollisionEvent.CollidingState = EventCollidingState.Colliding;
                    resultList.Add(currentFrameCollisionEvent);
                    i++;
                    j++;
                }
                else if (cmpResult < 0)
                {
                    // Appears in current, but not in previous, mark it as BeginColliding
                    currentFrameCollisionEvent.CollidingState = EventCollidingState.BeginColliding;
                    resultList.Add(currentFrameCollisionEvent);
                    i++;
                }
                else
                {
                    // Appears in previous, but not in current, mark it as EndColliding
                    previousFrameCollisionEvent.CollidingState = EventCollidingState.EndColliding;
                    resultList.Add(previousFrameCollisionEvent);
                    j++;
                }
            }
            
            if (i == currentFrameCollisionEvents.Length)
            {
                while (j < previousFrameCollisionEvents.Length)
                {
                    var collisionEvent = previousFrameCollisionEvents[j++];
                    collisionEvent.CollidingState = EventCollidingState.EndColliding;
                    resultList.Add(collisionEvent);
                }
            }
            else if (j == previousFrameCollisionEvents.Length)
            {
                while (i < currentFrameCollisionEvents.Length)
                {
                    var collisionEvent = currentFrameCollisionEvents[i++];
                    collisionEvent.CollidingState = EventCollidingState.BeginColliding;
                    resultList.Add(collisionEvent);
                }
            }
        }

        protected static void AddCollisionEventsToDynamicBuffer(NativeList<StatefulCollisionEvent> collisionEventList,
            ref BufferFromEntity<StatefulCollisionEvent> bufferFromEntity,
            NativeHashSet<Entity> entitiesWithCollisionEventBuffers)
        {
            foreach (var collisionEvent in collisionEventList)
            {
                if (entitiesWithCollisionEventBuffers.Contains(collisionEvent.EntityA))
                {
                    bufferFromEntity[collisionEvent.EntityA].Add(collisionEvent);
                }

                if (entitiesWithCollisionEventBuffers.Contains(collisionEvent.EntityB))
                {
                    bufferFromEntity[collisionEvent.EntityB].Add(collisionEvent);
                }
            }
        }

        protected override void OnUpdate()
        {
            if(_query.CalculateEntityCount() == 0)
                return;
            
            Dependency = JobHandle.CombineDependencies(_stepPhysicsWorld.FinalSimulationJobHandle, Dependency);

            Entities
                .WithName("ClearCollisionEventDynamicBufferJobParallel")
                .WithBurst()
                .WithAll<CollisionEventBuffer>()
                .ForEach((ref DynamicBuffer<StatefulCollisionEvent> buffer) =>
                {
                    buffer.Clear();
                }).ScheduleParallel();
            
            SwapCollisionEventState();

            var previousFrameCollisionEvents = _previousFrameCollisionEvents;
            var currentFrameCollisionEvents = _currentFrameCollisionEvents;

            var collisionEventBufferFromEntity = GetBufferFromEntity<StatefulCollisionEvent>();
            var physicsCollisionEventBufferTags = GetComponentDataFromEntity<CollisionEventBuffer>();

            var entitiesWithBuffer = new NativeHashSet<Entity>(0, Allocator.TempJob);

            Entities
                .WithName("CollectCollisionBufferJob")
                .WithBurst()
                .WithAll<CollisionEventBuffer>()
                .ForEach((Entity e, ref DynamicBuffer<StatefulCollisionEvent> buffer) =>
                {
                    entitiesWithBuffer.Add(e);
                }).Schedule();

            var collectCollisionEventsJob = new CollectCollisionEventsJob
            {
                CollisionEvents = currentFrameCollisionEvents,
                CollisionEventBufferTags = physicsCollisionEventBufferTags,
                PhysicsWorld = _buildPhysicsWorld.PhysicsWorld,
                EntitiesWithBuffer = entitiesWithBuffer
            };
            
            Dependency = collectCollisionEventsJob.Schedule(_stepPhysicsWorld.Simulation, ref _buildPhysicsWorld.PhysicsWorld, Dependency);

            Job
                .WithName("ConvertCollisionEventStreamToDynamicBufferJob")
                .WithBurst()
                .WithCode(() =>
                {
                    currentFrameCollisionEvents.Sort();

                    var collisionEventWithStates =
                        new NativeList<StatefulCollisionEvent>(currentFrameCollisionEvents.Length, Allocator.Temp);
                    UpdateCollisionEventState(previousFrameCollisionEvents, currentFrameCollisionEvents,
                        collisionEventWithStates);
                    AddCollisionEventsToDynamicBuffer(collisionEventWithStates, ref collisionEventBufferFromEntity,
                        entitiesWithBuffer);
                }).Schedule();
            
            _endFramePhysicsSystem.AddInputDependency(Dependency);
            entitiesWithBuffer.Dispose(Dependency);
        }

        [BurstCompile]
        public struct CollectCollisionEventsJob : ICollisionEventsJob
        {
            public NativeList<StatefulCollisionEvent> CollisionEvents;
            public ComponentDataFromEntity<CollisionEventBuffer> CollisionEventBufferTags;

            [ReadOnly] public NativeHashSet<Entity> EntitiesWithBuffer;
            [ReadOnly] public PhysicsWorld PhysicsWorld;

            public void Execute(CollisionEvent collisionEvent)
            {
                var collisionEventBufferElement = new StatefulCollisionEvent(
                    collisionEvent.EntityA,
                    collisionEvent.EntityB,
                    collisionEvent.BodyIndexA, 
                    collisionEvent.BodyIndexB, 
                    collisionEvent.ColliderKeyA,
                    collisionEvent.ColliderKeyB,
                    collisionEvent.Normal);

                var calculateDetails = false;

                if (EntitiesWithBuffer.Contains(collisionEvent.EntityA))
                {
                    if (CollisionEventBufferTags[collisionEvent.EntityA].CalculateDetails != 0)
                    {
                        calculateDetails = true;
                    }
                }

                if (!calculateDetails && EntitiesWithBuffer.Contains(collisionEvent.EntityB))
                {
                    if (CollisionEventBufferTags[collisionEvent.EntityB].CalculateDetails != 0)
                    {
                        calculateDetails = true;
                    }
                }

                if (calculateDetails)
                {
                    var details = collisionEvent.CalculateDetails(ref PhysicsWorld);
                    collisionEventBufferElement.CollisionDetails = new StatefulCollisionEvent.Details(
                        details.EstimatedContactPointPositions.Length, details.EstimatedImpulse,
                        details.AverageContactPointPosition);
                }
                
                CollisionEvents.Add(collisionEventBufferElement);
            }
        }
        
    }
}