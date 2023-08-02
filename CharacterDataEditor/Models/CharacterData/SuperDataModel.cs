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
        public bool FinalBlowKO { get; set; } = false;
        public int Duration { get; set; } = 0;
        public float IncreaseAttackBy { get; set; } = 0.0f;
        public float IncreaseSpeedBy { get; set; } = 0.0f;
        public float IncreaseDefenseBy { get; set; } = 0.0f;
        public bool ShortHop { get; set; } = false;
        public bool SuperJump { get; set; } = false;
        public bool DoubleJump { get; set; } = false;
        public bool SpiritInstall { get; set; } = false;

        public override int GetHashCode()
        {
            var hash = HashCode.Combine(Type, ScreenFreezeTime, FinalBlowKO, Duration, IncreaseAttackBy, IncreaseSpeedBy, IncreaseDefenseBy, ShortHop);
            hash = HashCode.Combine(hash, SuperJump, DoubleJump, SpiritInstall);

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
                 objAsSuperData.FinalBlowKO.Equals(FinalBlowKO) &&
                 objAsSuperData.Duration.Equals(Duration) &&
                 objAsSuperData.IncreaseAttackBy.Equals(IncreaseAttackBy) &&
                 objAsSuperData.IncreaseSpeedBy.Equals(IncreaseSpeedBy) &&
                 objAsSuperData.IncreaseDefenseBy.Equals(IncreaseDefenseBy) &&
                 objAsSuperData.ShortHop.Equals(ShortHop) &&
                 objAsSuperData.SuperJump.Equals(SuperJump) &&
                 objAsSuperData.DoubleJump.Equals(DoubleJump) &&
                 objAsSuperData.SpiritInstall.Equals(SpiritInstall))
            {
                return true;
            }

            return false;
        }
    }
}
