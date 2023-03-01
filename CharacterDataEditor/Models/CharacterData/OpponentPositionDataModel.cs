using System;
using System.Collections.Generic;
using System.Linq;

namespace CharacterDataEditor.Models.CharacterData
{
    public class OpponentPositionDataModel
    {
        public int NumberOfFrames { get { return Frames?.Count ?? 0; } }
        public float DistanceFromWall { get; set; } = 0.0f;
        public List<OpponentPositionFrameModel> Frames { get; set; } = new List<OpponentPositionFrameModel>();

        public override int GetHashCode()
        {
            return HashCode.Combine(DistanceFromWall, Frames);
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
                    return true;
                }
            }

            return false;
        }
    }
}