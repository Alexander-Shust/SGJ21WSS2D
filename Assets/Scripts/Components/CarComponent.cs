using Unity.Entities;

namespace Components
{
    [GenerateAuthoringComponent]
    public struct CarComponent : IComponentData
    {
        public float MovementSpeed;
    }
}