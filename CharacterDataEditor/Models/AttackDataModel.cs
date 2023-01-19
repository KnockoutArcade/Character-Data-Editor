namespace CharacterDataEditor.Models
{
    public record AttackDataModel
    {
        public int Start { get; set; }
        public int Lifetime { get; set; }
        public int AttackWidth { get; set; }
        public int AttackHeight { get; set; }
        public int WidthOffset { get; set; }
        public int HeightOffset { get; set; }
        public int Group { get; set; }
        public int Damage { get; set; }
        public int AttackHitStop { get; set; }
        public int AttackHitStun { get; set; }
        public AttackType AttackType { get; set; }
        public int BlockStun { get; set; }
        public int KnockBack { get; set; }
        public int AirKnockbackVertical { get; set; }
        public int AirKnockbackHorizontal { get; set; }
        public bool Launches { get; set; }
        public int LaunchKnockbackVertical { get; set; }
        public int LaunchKnockbackHorizontal { get; set; }
        public int Pushback { get; set; }
        public int ParticleXOffset { get; set; }
        public int ParticleYOffset { get; set; }
        public string ParticleEffect { get; set; }
        public int ParticleDuration { get; set; }
        public int HoldXOffset { get; set; }
        public int HoldYOffset { get; set; }
    }
}