using System;
using Unity.Assertions;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace Physics.Components
{
    public struct StatefulTriggerEvent : IBufferElementData, IComparable<StatefulTriggerEvent>
    {
        private readonly EntityPair _entities;
        private readonly BodyIndexPair _bodyIndices;
        private readonly ColliderKeyPair _colliderKeys;

        public EventOverlapState State;
        public Entity EntityA => _entities.EntityA;
        public Entity EntityB => _entities.EntityB;
        public int BodyIndexA => _bodyIndices.BodyIndexA;
        public int BodyIndexB => _bodyIndices.BodyIndexB;
        public ColliderKey ColliderKeyA => _colliderKeys.ColliderKeyA;
        public ColliderKey ColliderKeyB => _colliderKeys.ColliderKeyB;

        public StatefulTriggerEvent(Entity entityA, Entity entityB, int bodyIndexA, int bodyIndexB,
            ColliderKey colliderKeyA, ColliderKey colliderKeyB)
        {
            _entities = new EntityPair
            {
                EntityA = entityA,
                EntityB = entityB
            };
            _bodyIndices = new BodyIndexPair
            {
                BodyIndexA = bodyIndexA,
                BodyIndexB = bodyIndexB
            };
            _colliderKeys = new ColliderKeyPair
            {
                ColliderKeyA = colliderKeyA,
                ColliderKeyB = colliderKeyB
            };
            State = default;
        }

        // Returns other entity in EntityPair, if provided with one
        public Entity GetOtherEntity(Entity entity)
        {
            Assert.IsTrue((entity == EntityA) || (entity == EntityB));
            int2 indexAndVersion = math.select(new int2(EntityB.Index, EntityB.Version),
                new int2(EntityA.Index, EntityA.Version), entity == EntityB);
            return new Entity
            {
                Index = indexAndVersion[0],
                Version = indexAndVersion[1]
            };
        }

        public int CompareTo(StatefulTriggerEvent other)
        {
            var cmpResult = EntityA.CompareTo(other.EntityA);
            if (cmpResult != 0)
            {
                return cmpResult;
            }

            cmpResult = EntityB.CompareTo(other.EntityB);
            if (cmpResult != 0)
            {
                return cmpResult;
            }

            if (ColliderKeyA.Value != other.ColliderKeyA.Value)
            {
                return ColliderKeyA.Value < other.ColliderKeyA.Value ? -1 : 1;
            }

            if (ColliderKeyB.Value != other.ColliderKeyB.Value)
            {
                return ColliderKeyB.Value < other.ColliderKeyB.Value ? -1 : 1;
            }

            return 0;
        }
    }
    
    // If this component is added to an entity, trigger events won't be added to dynamic buffer
    // of that entity by TriggerEventConversionSystem. This component is by default added to
    // CharacterController entity, so that CharacterControllerSystem can add trigger events to
    // CharacterController on its own, without TriggerEventConversionSystem interference.
}