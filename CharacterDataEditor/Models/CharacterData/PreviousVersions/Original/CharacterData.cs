using CharacterDataEditor.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CharacterDataEditor.Models.CharacterData.PreviousVersions.Original
{
    public class CharacterDataModel : BaseCharacter
    {
        public string Name { get; set; } = string.Empty;
        public string BaseSprite { get; set; } = string.Empty;
        public PaletteModel BaseColor { get; set; } = new PaletteModel();
        public int NumberOfPalettes { get { return Palettes?.Count ?? 0; } }
        public List<PaletteModel> Palettes { get; set; } = new List<PaletteModel>();
        public List<MoveDataModel> MoveData { get; set; } = new List<MoveDataModel>();

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, BaseSprite, BaseColor, Palettes, MoveData);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(CharacterDataModel))
            {
                return false;
            }

            var objAsCharacterDataModel = (CharacterDataModel)obj;

            if (objAsCharacterDataModel.Name.Equals(Name))
            {
                if (objAsCharacterDataModel.BaseSprite.Equals(BaseSprite))
                {
                    if (objAsCharacterDataModel.BaseColor.Equals(BaseColor))
                    {
                        if (objAsCharacterDataModel.Palettes.SequenceEqual(Palettes))
                        {
                            if (objAsCharacterDataModel.MoveData.SequenceEqual(MoveData))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
    }

    public class PaletteModel
    {
        public string UID { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public int NumberOfReplacableColors { get { return ColorPalette?.Count ?? 0; } }
        public List<RGBModel> ColorPalette { get; set; } = new List<RGBModel>();

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, UID, ColorPalette);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(PaletteModel))
            {
                return false;
            }

            var objAsPaletteModel = (PaletteModel)obj;

            if (objAsPaletteModel.UID.Equals(UID))
            {
                if (objAsPaletteModel.Name.Equals(Name))
                {
                    if (objAsPaletteModel.ColorPalette.SequenceEqual(ColorPalette))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public record RGBModel
    {
        public RGBModel() { }
        public RGBModel(float r, float g, float b)
        {
            //convert to 0-255 by multiplying by 255
            r *= 255.0f;
            g *= 255.0f;
            b *= 255.0f;

            //round away from zero... this gave me the most consistent results
            //so that there wouldn't be a "jump" in the numbers when using
            //the color picker
            var red = Math.Round(r, 0, MidpointRounding.AwayFromZero);
            var green = Math.Round(g, 0, MidpointRounding.AwayFromZero);
            var blue = Math.Round(b, 0, MidpointRounding.AwayFromZero);

            //convert to int
            Red = (int)red;
            Green = (int)green;
            Blue = (int)blue;
        }

        public int Red { get; set; } = 0;
        public int Blue { get; set; } = 0;
        public int Green { get; set; } = 0;
    }

    public class RehitDataModel
    {
        public int HitBox { get; set; } = 0;
        public int NumberOfHits { get { return HitOnFrames?.Count ?? 0; } }
        public List<int> HitOnFrames { get; set; } = new List<int>();

        public override int GetHashCode()
        {
            return HashCode.Combine(HitBox, HitOnFrames);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(RehitDataModel))
            {
                return false;
            }

            var objAsRehitData = obj as RehitDataModel;

            if (objAsRehitData.HitBox == HitBox)
            {
                if (objAsRehitData.HitOnFrames.SequenceEqual(HitOnFrames))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class OpponentPositionFrameModel
    {
        public int Frame { get; set; } = 0;
        public int RelativeX { get; set; } = 0;
        public int RelativeY { get; set; } = 0;
        public SpriteType Sprite { get; set; } = SpriteType.HurtSprite;
        public int Index { get; set; } = 0;
        public int Rotation { get; set; } = 0;
        public int XScale { get; set; } = 0;

        public override int GetHashCode()
        {
            return HashCode.Combine(Frame, RelativeX, RelativeY, Sprite, Index, Rotation, XScale);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(OpponentPositionFrameModel))
            {
                return false;
            }

            var objAsOpponentPositionFrame = obj as OpponentPositionFrameModel;

            if (objAsOpponentPositionFrame.XScale == XScale)
            {
                if (objAsOpponentPositionFrame.Rotation == Rotation)
                {
                    if (objAsOpponentPositionFrame.Sprite == Sprite)
                    {
                        if (objAsOpponentPositionFrame.Frame == Frame)
                        {
                            if (objAsOpponentPositionFrame.Index == Index)
                            {
                                if (objAsOpponentPositionFrame.RelativeX == RelativeX)
                                {
                                    if (objAsOpponentPositionFrame.RelativeY == RelativeY)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }
    }

    public class OpponentPositionDataModel
    {
        public int NumberOfFrames { get { return Frames?.Count ?? 0; } }
        public float DistanceFromWall { get; set; } = 0.0f;
        public List<OpponentPositionFrameModel> Frames { get; set; } = new List<OpponentPositionFrameModel>();

        public override int GetHashCode()
        {
            return HashCode.Combine(DistanceFromWall, Frames);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(OpponentPositionDataModel))
            {
                return false;
            }

            var objAsOpponentPositionData = obj as OpponentPositionDataModel;

            if (objAsOpponentPositionData.DistanceFromWall == DistanceFromWall)
            {
                if (objAsOpponentPositionData.Frames.SequenceEqual(Frames))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class MovementDataModel
    {
        public int StartingFrame { get; set; } = 0;
        public float HorizontalSpeed { get; set; } = 0.0f;
        public float VerticalSpeed { get; set; } = 0.0f;
        public bool OverwriteSpeed { get; set; } = false;

        public override int GetHashCode()
        {
            return HashCode.Combine(StartingFrame, HorizontalSpeed, VerticalSpeed, OverwriteSpeed);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(MovementDataModel))
            {
                return false;
            }

            var objAsMovementData = obj as MovementDataModel;

            if (objAsMovementData.StartingFrame == StartingFrame)
            {
                if (objAsMovementData.HorizontalSpeed == HorizontalSpeed)
                {
                    if (objAsMovementData.VerticalSpeed == VerticalSpeed)
                    {
                        if (objAsMovementData.OverwriteSpeed == OverwriteSpeed)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }

    public class MoveDataModel
    {
        public string UID { get; set; } = Guid.NewGuid().ToString();
        public MoveType MoveType { get; set; } = MoveType.BackwardThrow;
        public string SpriteName { get; set; } = string.Empty;
        public int NumberOfFrames { get { return FrameData?.Count ?? 0; } }
        public List<FrameDataModel> FrameData { get; set; } = new List<FrameDataModel>();
        public int NumberOfHitboxes { get { return AttackData?.Count ?? 0; } }
        public List<AttackDataModel> AttackData { get; set; } = new List<AttackDataModel>();
        public bool IsThrow { get; set; } = false;
        public OpponentPositionDataModel OpponentPositionData { get; set; } = new OpponentPositionDataModel();
        public int NumberOfHurtboxes { get { return HurtboxData?.Count ?? 0; } }
        public List<HurtboxDataModel> HurtboxData { get; set; } = new List<HurtboxDataModel>();
        public RehitDataModel RehitData { get; set; } = new RehitDataModel();
        public int NumberOfMovementData { get { return MovementData?.Count ?? 0; } }
        public List<MovementDataModel> MovementData { get; set; } = new List<MovementDataModel>();

        public override int GetHashCode()
        {
            var hash = HashCode.Combine(UID, MoveType, SpriteName, FrameData, AttackData, IsThrow, HurtboxData, RehitData);
            hash = HashCode.Combine(hash, OpponentPositionData, MovementData);

            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(MoveDataModel))
            {
                return false;
            }

            var objAsMoveData = obj as MoveDataModel;

            if (objAsMoveData.UID == UID)
            {
                if (objAsMoveData.MoveType == MoveType)
                {
                    if (objAsMoveData.SpriteName == SpriteName)
                    {
                        if (objAsMoveData.FrameData.SequenceEqual(FrameData))
                        {
                            if (objAsMoveData.AttackData.SequenceEqual(AttackData))
                            {
                                if (objAsMoveData.IsThrow == IsThrow)
                                {
                                    if (objAsMoveData.HurtboxData.SequenceEqual(HurtboxData))
                                    {
                                        if (objAsMoveData.OpponentPositionData.Equals(OpponentPositionData))
                                        {
                                            if (objAsMoveData.RehitData.Equals(RehitData))
                                            {
                                                if (objAsMoveData.MovementData.SequenceEqual(MovementData))
                                                {
                                                    return true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }
    }

    public class HurtboxDataModel
    {
        public int Start { get; set; } = 0;
        public int Lifetime { get; set; } = 0;
        public int AttackWidth { get; set; } = 0;
        public int AttackHeight { get; set; } = 0;
        public int WidthOffset { get; set; } = 0;
        public int HeightOffset { get; set; } = 0;

        public override int GetHashCode()
        {
            return HashCode.Combine(Start, Lifetime, AttackHeight, AttackWidth, WidthOffset, HeightOffset);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(HurtboxDataModel))
            {
                return false;
            }

            var objAsHurtboxData = obj as HurtboxDataModel;

            if (objAsHurtboxData.Start == Start)
            {
                if (objAsHurtboxData.Lifetime == Lifetime)
                {
                    if (objAsHurtboxData.AttackHeight == AttackHeight)
                    {
                        if (objAsHurtboxData.AttackWidth == AttackWidth)
                        {
                            if (objAsHurtboxData.WidthOffset == WidthOffset)
                            {
                                if (objAsHurtboxData.HeightOffset == HeightOffset)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }
    }

    public class FrameDataModel
    {
        public int Length { get; set; } = 0;
        public int ImageIndex { get; set; } = 0;

        public override int GetHashCode()
        {
            return HashCode.Combine(Length, ImageIndex);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(FrameDataModel))
            {
                return false;
            }

            var objAsFrameData = obj as FrameDataModel;

            if (objAsFrameData.Length == Length)
            {
                if (objAsFrameData.ImageIndex == ImageIndex)
                {
                    return true;
                }
            }

            return false;
        }
    }
    public class AttackDataModel
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
        public float BlockStun { get; set; } = 0;
        public float KnockBack { get; set; } = 0;
        public float AirKnockbackVertical { get; set; } = 0.0f;
        public float AirKnockbackHorizontal { get; set; } = 0.0f;
        public bool Launches { get; set; } = false;
        public float LaunchKnockbackVertical { get; set; } = 0.0f;
        public float LaunchKnockbackHorizontal { get; set; } = 0.0f;
        public float Pushback { get; set; } = 0;
        public int ParticleXOffset { get; set; } = 0;
        public int ParticleYOffset { get; set; } = 0;
        public string ParticleEffect { get; set; } = string.Empty;
        public int ParticleDuration { get; set; } = 0;
        public int HoldXOffset { get; set; } = 0;
        public int HoldYOffset { get; set; } = 0;
        public float MeterGain { get; set; } = 0.0f;
        public float ComboScaling { get; set; } = 0.0f;

        public override int GetHashCode()
        {
            var hash = HashCode.Combine(Start, Lifetime, AttackWidth, AttackHeight, WidthOffset, HeightOffset, Group, Damage);
            hash = HashCode.Combine(hash, AttackHitStop, AttackHitStun, AttackType, BlockStun, KnockBack, AirKnockbackHorizontal, AirKnockbackVertical);
            hash = HashCode.Combine(hash, Launches, LaunchKnockbackHorizontal, LaunchKnockbackVertical, Pushback, ParticleXOffset, ParticleYOffset, ParticleEffect);
            hash = HashCode.Combine(hash, ParticleDuration, HoldXOffset, HoldYOffset, MeterGain, ComboScaling);

            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(AttackDataModel))
            {
                return false;
            }

            var objAsAttackData = obj as AttackDataModel;

            if (objAsAttackData.Start == Start)
            {
                if (objAsAttackData.Lifetime == Lifetime)
                {
                    if (objAsAttackData.AttackWidth == AttackWidth)
                    {
                        if (objAsAttackData.AttackHeight == AttackHeight)
                        {
                            if (objAsAttackData.WidthOffset == WidthOffset)
                            {
                                if (objAsAttackData.Group == Group)
                                {
                                    if (objAsAttackData.Damage == Damage)
                                    {
                                        if (objAsAttackData.AttackHitStop == AttackHitStop)
                                        {
                                            if (objAsAttackData.AttackHitStun == AttackHitStun)
                                            {
                                                if (objAsAttackData.AttackType == AttackType)
                                                {
                                                    if (objAsAttackData.BlockStun == BlockStun)
                                                    {
                                                        if (objAsAttackData.KnockBack == KnockBack)
                                                        {
                                                            if (objAsAttackData.AirKnockbackHorizontal == AirKnockbackHorizontal)
                                                            {
                                                                if (objAsAttackData.AirKnockbackVertical == AirKnockbackVertical)
                                                                {
                                                                    if (objAsAttackData.Launches == Launches)
                                                                    {
                                                                        if (objAsAttackData.LaunchKnockbackHorizontal == LaunchKnockbackHorizontal)
                                                                        {
                                                                            if (objAsAttackData.LaunchKnockbackVertical == LaunchKnockbackVertical)
                                                                            {
                                                                                if (objAsAttackData.Pushback == Pushback)
                                                                                {
                                                                                    if (objAsAttackData.ParticleDuration == ParticleDuration)
                                                                                    {
                                                                                        if (objAsAttackData.ParticleEffect == ParticleEffect)
                                                                                        {
                                                                                            if (objAsAttackData.ParticleXOffset == ParticleXOffset)
                                                                                            {
                                                                                                if (objAsAttackData.ParticleYOffset == ParticleYOffset)
                                                                                                {
                                                                                                    if (objAsAttackData.HoldXOffset == HoldXOffset)
                                                                                                    {
                                                                                                        if (objAsAttackData.HoldYOffset == HoldYOffset)
                                                                                                        {
                                                                                                            if (objAsAttackData.MeterGain == MeterGain)
                                                                                                            {
                                                                                                                if (objAsAttackData.ComboScaling == ComboScaling)
                                                                                                                {
                                                                                                                    return true;
                                                                                                                }
                                                                                                            }
                                                                                                        }
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}