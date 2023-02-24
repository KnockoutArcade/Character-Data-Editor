using System;
using System.Collections.Generic;
using System.Linq;

namespace CharacterDataEditor.Models.CharacterData
{
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
}