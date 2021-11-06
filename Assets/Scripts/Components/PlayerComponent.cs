using Unity.Entities;

namespace Components
{
    [GenerateAuthoringComponent]
    public struct PlayerComponent : IComponentData
    {
        public float MovementSpeed;
    }
}