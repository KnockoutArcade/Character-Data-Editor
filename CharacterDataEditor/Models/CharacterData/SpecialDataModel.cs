using CharacterDataEditor.Enums;
using System;

namespace CharacterDataEditor.Models.CharacterData
{
    public class SpecialDataModel
    {
        public int NumpadInput { get; set; } = 0;
        public int StartingFrame { get; set; } = 0;
        public int EndingFrame { get; set; } = 0;
        public MoveType EnhancementMove { get; set; } = MoveType.EnhancedNeutralSpecial;
        public bool TransitionImmediately { get; set; } = false;
        public int TransitionFrame { get; set; } = 0;

        public override int GetHashCode()
        {
            var hash = HashCode.Combine(NumpadInput, StartingFrame, EndingFrame, EnhancementMove, TransitionImmediately, TransitionFrame);

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

            if (objAsSpecialData.NumpadInput == NumpadInput)
            {
                if (objAsSpecialData.StartingFrame == StartingFrame)
                {
                    if (objAsSpecialData.EndingFrame == EndingFrame)
                    {
                        if (objAsSpecialData.EnhancementMove == EnhancementMove)
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

            return false;
        }
    }
}
