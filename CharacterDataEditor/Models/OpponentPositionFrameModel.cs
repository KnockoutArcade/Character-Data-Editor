using CharacterDataEditor.Enums;
using System;

namespace CharacterDataEditor.Models
{
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
}