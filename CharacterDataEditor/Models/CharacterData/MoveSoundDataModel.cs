using CharacterDataEditor.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CharacterDataEditor.Models.CharacterData
{
    public class MoveSoundDataModel
    {
        public string SoundEffect { get; set; } = string.Empty;
        public int SFXPlayFrame { get; set; } = 0;

        public override int GetHashCode()
        {
            var hash = HashCode.Combine(SoundEffect, SFXPlayFrame);

            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(MoveSoundDataModel))
            {
                return false;
            }

            var objAsSoundData = obj as MoveSoundDataModel;

            if (objAsSoundData.SoundEffect.Equals(SoundEffect))
            {
                if (objAsSoundData.SFXPlayFrame.Equals(SFXPlayFrame))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
