﻿using CharacterDataEditor.Enums;
using System;

namespace CharacterDataEditor.Models.CharacterData
{
    public class CommandNormalDataModel
    {
        public DirectionType NumpadDirection { get; set; } = DirectionType.None;
        public CommandButton Button { get; set; } = CommandButton.Light;
        public bool GroundOrAir { get; set; } = false;
        public bool CancelWhenLanding { get; set; } = false;

        public override int GetHashCode()
        {
            var hash = HashCode.Combine(NumpadDirection, Button, GroundOrAir, CancelWhenLanding);

            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(CommandNormalDataModel))
            {
                return false;
            }

            var objAsCommandNormalData = obj as CommandNormalDataModel;

            if (objAsCommandNormalData.NumpadDirection == NumpadDirection)
            {
                if (objAsCommandNormalData.Button == Button)
                {
                    if (objAsCommandNormalData.GroundOrAir == GroundOrAir)
                    {
                        if (objAsCommandNormalData.CancelWhenLanding == CancelWhenLanding)
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
