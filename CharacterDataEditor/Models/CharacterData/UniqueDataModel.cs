using CharacterDataEditor.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CharacterDataEditor.Models.CharacterData
{
    public class UniqueDataModel : BaseCharacter
    {
        public int AdditionalMovesets { get; set; } = 0;
        public SpiritDataType SpiritData { get; set; } = SpiritDataType.None;

        public override int GetHashCode()
        {
            var hash = HashCode.Combine(AdditionalMovesets, SpiritData);

            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(UniqueDataModel))
            {
                return false;
            }

            var objAsUniqueData = obj as UniqueDataModel;

            if (objAsUniqueData.AdditionalMovesets == AdditionalMovesets)
            {
                if (objAsUniqueData.SpiritData.Equals(SpiritData))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
