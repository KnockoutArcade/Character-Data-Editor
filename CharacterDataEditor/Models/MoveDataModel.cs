using System.Collections.Generic;
using CharacterDataEditor.Enums;

namespace CharacterDataEditor.Models
{
    public record MoveDataModel
    {
        public MoveType MoveType { get; set; }
        public string SpriteName { get; set; }
        public int NumberOfFrames { get; set; }
        public List<FrameDataModel> FrameData { get; set; }
        public AttackDataModel AttackData { get; set; }
        public bool IsThrow { get; set; }
        public List<HurtboxDataModel> HurtboxData { get; set; }
    }
}