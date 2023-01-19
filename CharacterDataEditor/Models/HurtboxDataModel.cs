namespace CharacterDataEditor.Models
{
    public record HurtboxDataModel
    {
        public int Start { get; set; }
        public int Lifetime { get; set; }
        public int AttackWidth { get; set; }
        public int AttackHeight { get; set; }
        public int WidthOffset { get; set; }
        public int HeightOffset { get; set; }
    }
}