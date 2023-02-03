using System;
using System.Collections.Generic;
using System.Linq;

namespace CharacterDataEditor.Models
{
    public class PaletteModel
    {
        public string UID { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public int NumberOfReplacableColors { get { return ColorPalette?.Count ?? 0; } }
        public List<RGBModel> ColorPalette { get; set; } = new List<RGBModel>();

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, UID, ColorPalette);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(PaletteModel))
            {
                return false;
            }

            var objAsPaletteModel = (PaletteModel)obj;

            if (objAsPaletteModel.UID.Equals(UID))
            {
                if (objAsPaletteModel.Name.Equals(Name))
                {
                    if (objAsPaletteModel.ColorPalette.SequenceEqual(ColorPalette))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}