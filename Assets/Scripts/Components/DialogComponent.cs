using Unity.Entities;

namespace Components
{
    [GenerateAuthoringComponent]
    public struct DialogComponent : IComponentData
    {
        public float Value;
    }
}