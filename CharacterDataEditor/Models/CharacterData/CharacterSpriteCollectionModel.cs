using System;

namespace CharacterDataEditor.Models.CharacterData
{
    public class CharacterSpriteCollectionModel
    {
        public string Idle { get; set; } = string.Empty;
        public string Crouch { get; set; } = string.Empty;
        public string StandBlock { get; set; } = string.Empty;
        public string CrouchBlock { get; set; } = string.Empty;
        public string WalkForward { get; set; } = string.Empty;
        public string WalkBackward { get; set; } = string.Empty;
        public string RunForward { get; set; } = string.Empty;
        public string RunBackward { get; set; } = string.Empty;
        public string JumpSquat { get; set; } = string.Empty;
        public string Jump { get; set; } = string.Empty;
        public string Hurt { get; set; } = string.Empty;
        public string Grab { get; set; } = string.Empty;
        public string Hold { get; set; } = string.Empty;
        public string Launched { get; set; } = string.Empty;
        public string Knockdown { get; set; } = string.Empty;
        public string GetUp { get; set; } = string.Empty;

        public override int GetHashCode()
        {
            var hash = HashCode.Combine(Idle, Crouch, StandBlock, CrouchBlock, WalkForward, WalkBackward, RunForward, RunBackward);
            hash = HashCode.Combine(hash, JumpSquat, Jump, Hurt, Grab, Hold, Launched, Knockdown);
            hash = HashCode.Combine(hash, GetUp);

            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(CharacterSpriteCollectionModel))
            {
                return false;
            }

            var objAsCharacterSpriteCollection = obj as CharacterSpriteCollectionModel;

            if (objAsCharacterSpriteCollection.Idle.Equals(Idle) &&
                objAsCharacterSpriteCollection.Crouch.Equals(Crouch) &&
                objAsCharacterSpriteCollection.StandBlock.Equals(StandBlock) &&
                objAsCharacterSpriteCollection.CrouchBlock.Equals(CrouchBlock) &&
                objAsCharacterSpriteCollection.WalkForward.Equals(WalkForward) &&
                objAsCharacterSpriteCollection.WalkBackward.Equals(WalkBackward) &&
                objAsCharacterSpriteCollection.RunForward.Equals(RunForward) &&
                objAsCharacterSpriteCollection.RunBackward.Equals(RunBackward) &&
                objAsCharacterSpriteCollection.JumpSquat.Equals(JumpSquat) &&
                objAsCharacterSpriteCollection.Jump.Equals(Jump) &&
                objAsCharacterSpriteCollection.Hurt.Equals(Hurt) &&
                objAsCharacterSpriteCollection.Grab.Equals(Grab) &&
                objAsCharacterSpriteCollection.Hold.Equals(Hold) &&
                objAsCharacterSpriteCollection.Launched.Equals(Launched) &&
                objAsCharacterSpriteCollection.Knockdown.Equals(Knockdown) &&
                objAsCharacterSpriteCollection.GetUp.Equals(GetUp))
            {
                return true;
            }

            return false;
        }
    }
}