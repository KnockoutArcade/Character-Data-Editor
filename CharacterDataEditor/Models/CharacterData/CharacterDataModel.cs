using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CharacterDataEditor.Models.CharacterData
{
    public class CharacterDataModel : BaseCharacter
    {
        public string Name { get; set; } = string.Empty;
        public string BaseSprite { get; set; } = string.Empty;
        public PaletteModel BaseColor { get; set; } = new PaletteModel();
        public int NumberOfPalettes { get { return Palettes?.Count ?? 0; } }
        public List<PaletteModel> Palettes { get; set; } = new List<PaletteModel>();
        public List<MoveDataModel> MoveData { get; set; } = new List<MoveDataModel>();

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, BaseSprite, BaseColor, Palettes, MoveData);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(CharacterDataModel))
            {
                return false;
            }

            var objAsCharacterDataModel = (CharacterDataModel)obj;

            if (objAsCharacterDataModel.Name.Equals(Name))
            {
                if (objAsCharacterDataModel.BaseSprite.Equals(BaseSprite))
                {
                    if (objAsCharacterDataModel.BaseColor.Equals(BaseColor))
                    {
                        if (objAsCharacterDataModel.Palettes.SequenceEqual(Palettes))
                        {
                            if (objAsCharacterDataModel.MoveData.SequenceEqual(MoveData))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}
