using CharacterDataEditor.Enums;

namespace CharacterDataEditor.Models
{
    public record AttackDataModel
    {
        public int Start { get; set; } = 0;
        public int Lifetime { get; set; } = 0;
        public int AttackWidth { get; set; } = 0;
        public int AttackHeight { get; set; } = 0;
        public int WidthOffset { get; set; } = 0;
        public int HeightOffset { get; set; } = 0;
        public int Group { get; set; } = 0;
        public int Damage { get; set; } = 0;
        public int AttackHitStop { get; set; } = 0;
        public int AttackHitStun { get; set; } = 0;
        public AttackType AttackType { get; set; } = AttackType.Low;
        public int BlockStun { get; set; } = 0;
        public int KnockBack { get; set; } = 0;
        public int AirKnockbackVertical { get; set; } = 0;
        public int AirKnockbackHorizontal { get; set; } = 0;
        public bool Launches { get; set; } = false;
        public int LaunchKnockbackVertical { get; set; } = 0;
        public int LaunchKnockbackHorizontal { get; set; } = 0;
        public int Pushback { get; set; } = 0;
        public int ParticleXOffset { get; set; } = 0;
        public int ParticleYOffset { get; set; } = 0;
        public string ParticleEffect { get; set; } = string.Empty;
        public int ParticleDuration { get; set; } = 0;
        public int HoldXOffset { get; set; } = 0;
        public int HoldYOffset { get; set; } = 0;
    }
}