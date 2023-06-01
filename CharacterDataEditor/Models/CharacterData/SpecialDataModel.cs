using CharacterDataEditor.Enums;
using System;

namespace CharacterDataEditor.Models.CharacterData
{
    public class SpecialDataModel
    {
        public int NumberOfEnhancements { get; set; } = 0;
        public int NumpadInput { get; set; } = 0;
        public int StartingFrame { get; set; } = 0;
        public int LastFrame { get; set; } = 0;
        public MoveType MoveType { get; set; } = MoveType.EnhancedNeutralSpecial;
        public bool TransitionImmediately { get; set; } = false;
        public int TransitionFrame { get; set; } = 0;

        public override int GetHashCode()
        {
            var hash = HashCode.Combine(NumberOfEnhancements, NumpadInput, StartingFrame, LastFrame, MoveType, TransitionImmediately, TransitionFrame);

            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(SpecialDataModel))
            {
                return false;
            }

            var objAsSpecialData = obj as SpecialDataModel;

            if (objAsSpecialData.NumberOfEnhancements == NumberOfEnhancements)
            {
                if (objAsSpecialData.NumpadInput == NumpadInput)
                {
                    if (objAsSpecialData.StartingFrame == StartingFrame)
                    {
                        if (objAsSpecialData.LastFrame == LastFrame)
                        {
                            if (objAsSpecialData.MoveType == MoveType)
                            {
                                if (objAsSpecialData.TransitionImmediately == TransitionImmediately)
                                {
                                    if (objAsSpecialData.TransitionFrame == TransitionFrame)
                                    {
                                        return true;
                                    }
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
