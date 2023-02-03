using System;

namespace CharacterDataEditor.Models
{
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
}