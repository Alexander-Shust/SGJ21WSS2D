using Unity.Entities;

namespace Components
{
    [GenerateAuthoringComponent]
    public struct GameSettingsComponent : IComponentData
    {
        public float StartingScore;
        public float BoxPickupBonus;
        public float GirlBonus;
        public float EndBonus;
    }
}