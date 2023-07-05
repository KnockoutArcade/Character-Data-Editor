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
        public bool PerformAttack { get; set; } = false; 
        public bool StartAtCurrent { get; set; } = false;
        public int StartXOffset { get; set; } = 0;
        public int StartYOffset { get; set; } = 0;
        public bool ReturnToPlayer { get; set; } = false;

        public override int GetHashCode()
        {
            var hash = HashCode.Combine(ToggleState, PerformAttack, StartAtCurrent, StartXOffset, StartYOffset, ReturnToPlayer);

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
                if (objAsSpiritData.PerformAttack.Equals(PerformAttack))
                {
                    if (objAsSpiritData.StartAtCurrent.Equals(StartAtCurrent))
                    {
                        if (objAsSpiritData.StartXOffset.Equals(StartXOffset))
                        {
                            if (objAsSpiritData.StartYOffset.Equals(StartYOffset))
                            {
                                if (objAsSpiritData.ReturnToPlayer.Equals(ReturnToPlayer))
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
