﻿using CharacterDataEditor.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CharacterDataEditor.Models.CharacterData
{
    public class UniqueDataModel
    {
        public int AdditionalMovesets { get; set; } = 0;
        public SpiritDataType SpiritData { get; set; } = SpiritDataType.None;
        public string Spirit { get; set; } = "None"; // I originally wanted to store the entire CharacterDataModel but that's too taxing on the program
        public bool DoubleJump { get; set; } = false;
        public bool LinkMovesetsWithSpirits { get; set; } = false;
        public int SpiritOffMoveset { get; set; } = 0;
        public int SpiritOnMoveset { get; set; } = 0;

        public override int GetHashCode()
        {
            var hash = HashCode.Combine(AdditionalMovesets, SpiritData, Spirit, DoubleJump, LinkMovesetsWithSpirits, SpiritOffMoveset, SpiritOnMoveset);

            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(UniqueDataModel))
            {
                return false;
            }

            var objAsUniqueData = obj as UniqueDataModel;

            if (objAsUniqueData.AdditionalMovesets == AdditionalMovesets)
            {
                if (objAsUniqueData.SpiritData.Equals(SpiritData))
                {
                    if (objAsUniqueData.Spirit.Equals(Spirit))
                    {
                        if (objAsUniqueData.DoubleJump.Equals(DoubleJump))
                        {
                            if (objAsUniqueData.LinkMovesetsWithSpirits.Equals(LinkMovesetsWithSpirits))
                            {
                                if (objAsUniqueData.SpiritOffMoveset.Equals(SpiritOffMoveset))
                                {
                                    if (objAsUniqueData.SpiritOnMoveset.Equals(SpiritOnMoveset))
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
