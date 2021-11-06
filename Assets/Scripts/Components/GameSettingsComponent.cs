using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
    [GenerateAuthoringComponent]
    public struct GameSettingsComponent : IComponentData
    {
        public float StartingScore;
        public float BoxPickupBonus;
        public float TrapBonus;
        public float EndBonus;
        public float PlayerSpeed;
        public float BatSpeed;
        public float GirlSpeed;
        public float3 CarStop;
        public float3 PlayerSpawnPoint;
        public float3 BatStop;
        public float3 GirlStop;
    }
}