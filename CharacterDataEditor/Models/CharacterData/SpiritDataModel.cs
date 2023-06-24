using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CharacterDataEditor.Enums;
using Newtonsoft.Json;

namespace CharacterDataEditor.Models.CharacterData
{
    public class SpiritDataModel
    {
        public bool ToggleState { get; set; } = false;
        public MoveType SpiritAttack { get; set; } = MoveType.None; 
        public int StartXOffset { get; set; } = 0;
        public int StartYOffset { get; set; } = 0;

        public override int GetHashCode()
        {
            var hash = HashCode.Combine(ToggleState, SpiritAttack, StartXOffset, StartYOffset);

            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(SpiritDataModel))
            {
                return false;
            }

            var objAsSpiritData = obj as SpiritDataModel;

            if (objAsSpiritData.ToggleState.Equals(ToggleState))
            {
                if (objAsSpiritData.SpiritAttack.Equals(SpiritAttack))
                {
                    if (objAsSpiritData.StartXOffset.Equals(StartXOffset))
                    {
                        if (objAsSpiritData.StartYOffset.Equals(StartYOffset))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
