using Unity.Entities;

namespace Physics.Components
{
    public struct CollisionEventBuffer : IComponentData
    {
        public int CalculateDetails;
    }
}