using System;
using System.Collections.Generic;
using CharacterDataEditor.Enums;

namespace CharacterDataEditor.Models
{
    public record MoveDataModel
    {
        public string UID { get; set; } = Guid.NewGuid().ToString();
        public MoveType MoveType { get; set; } = MoveType.BackwardThrow;
        public string SpriteName { get; set; } = string.Empty;
        public int NumberOfFrames { get { return FrameData?.Count ?? 0; } }
        public List<FrameDataModel> FrameData { get; set; } = new List<FrameDataModel>();
        public int NumberOfHitboxes { get { return AttackData?.Count ?? 0; } }
        public List<AttackDataModel> AttackData { get; set; } = new List<AttackDataModel>();
        public bool IsThrow { get; set; } = false;
        public int NumberOfHurtboxes { get { return HurtboxData?.Count ?? 0; } }
        public List<HurtboxDataModel> HurtboxData { get; set; } = new List<HurtboxDataModel>();
    }
}