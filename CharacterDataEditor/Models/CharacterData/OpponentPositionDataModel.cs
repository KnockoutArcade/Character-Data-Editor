using System;
using System.Collections.Generic;
using System.Linq;

namespace CharacterDataEditor.Models.CharacterData
{
    public class OpponentPositionDataModel
    {
        public float DistanceFromWall { get; set; } = 0.0f;
        public int ThrowOffset { get; set; } = 0;
        public int NumberOfFrames { get { return Frames?.Count ?? 0; } }
        public List<OpponentPositionFrameModel> Frames { get; set; } = new List<OpponentPositionFrameModel>();

        public override int GetHashCode()
        {
            return HashCode.Combine(DistanceFromWall, Frames, ThrowOffset);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(OpponentPositionDataModel))
            {
                return false;
            }

            var objAsOpponentPositionData = obj as OpponentPositionDataModel;

            if (objAsOpponentPositionData.DistanceFromWall == DistanceFromWall)
            {
                if (objAsOpponentPositionData.Frames.SequenceEqual(Frames))
                {
                    if (objAsOpponentPositionData.ThrowOffset == ThrowOffset)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}