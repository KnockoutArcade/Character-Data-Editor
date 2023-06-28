using CharacterDataEditor.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CharacterDataEditor.Models.CharacterData
{
    public class NonmoveSoundDataModel : BaseCharacter
    {
        public string WalkingSoundEffect { get; set; } = "";
        public List<int> WalkForwardFootsteps { get; set; } = new List<int>();
        public List<int> WalkBackwardFootsteps { get; set; } = new List<int>();
        public string RunningSoundEffect { get; set; } = "";
        public List<int> RunForwardFootsteps { get; set; } = new List<int>();
        public List<int> RunBackwardFootsteps { get; set; } = new List<int>();

        public override int GetHashCode()
        {
            var hash = HashCode.Combine(WalkingSoundEffect, WalkForwardFootsteps, WalkBackwardFootsteps, RunningSoundEffect, RunForwardFootsteps, RunBackwardFootsteps);

            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(NonmoveSoundDataModel))
            {
                return false;
            }

            var objAsSoundData = obj as NonmoveSoundDataModel;

            if (objAsSoundData.WalkingSoundEffect.Equals(WalkingSoundEffect))
            {
                if (objAsSoundData.WalkForwardFootsteps.SequenceEqual(WalkForwardFootsteps))
                {
                    if (objAsSoundData.WalkBackwardFootsteps.SequenceEqual(WalkBackwardFootsteps))
                    {
                        if (objAsSoundData.RunningSoundEffect.Equals(RunningSoundEffect))
                        {
                            if (objAsSoundData.RunForwardFootsteps.SequenceEqual(RunForwardFootsteps))
                            {
                                if (objAsSoundData.RunBackwardFootsteps.SequenceEqual(RunBackwardFootsteps))
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
