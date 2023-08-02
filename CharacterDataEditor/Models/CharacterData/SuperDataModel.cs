using CharacterDataEditor.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharacterDataEditor.Models.CharacterData
{
    public class SuperDataModel
    {
        public SuperType Type { get; set; } = SuperType.Attack;
        public int ScreenFreezeTime { get; set; } = 0;
        public int InvincibilityFrames { get; set; } = 0;
        public bool FinalBlowKO { get; set; } = false;
        public int Duration { get; set; } = 0;
        public float IncreaseAttackBy { get; set; } = 0.0f;
        public float IncreaseSpeedBy { get; set; } = 0.0f;
        public JumpType JumpType { get; set; } = JumpType.None;
        public bool SpiritInstall { get; set; } = false;

        public override int GetHashCode()
        {
            var hash = HashCode.Combine(Type, ScreenFreezeTime, InvincibilityFrames, FinalBlowKO, Duration, IncreaseAttackBy, IncreaseSpeedBy, JumpType);
            hash = HashCode.Combine(hash, SpiritInstall);

            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(SuperDataModel))
            {
                return false;
            }

            var objAsSuperData = obj as SuperDataModel;

            //objAsSuperData..Equals() &&
            if (objAsSuperData.Type.Equals(Type) &&
                 objAsSuperData.ScreenFreezeTime.Equals(ScreenFreezeTime) &&
                 objAsSuperData.InvincibilityFrames.Equals(InvincibilityFrames) &&
                 objAsSuperData.FinalBlowKO.Equals(FinalBlowKO) &&
                 objAsSuperData.Duration.Equals(Duration) &&
                 objAsSuperData.IncreaseAttackBy.Equals(IncreaseAttackBy) &&
                 objAsSuperData.IncreaseSpeedBy.Equals(IncreaseSpeedBy) &&
                 objAsSuperData.JumpType.Equals(JumpType) &&
                 objAsSuperData.SpiritInstall.Equals(SpiritInstall))
            {
                return true;
            }

            return false;
        }
    }
}
