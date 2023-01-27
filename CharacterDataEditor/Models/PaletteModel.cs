using System;
using System.Collections.Generic;

namespace CharacterDataEditor.Models
{
    public record PaletteModel
    {
        public string UID { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public int NumberOfReplacableColors { get { return ColorPalette?.Count ?? 0; } }
        public List<RGBModel> ColorPalette { get; set; } = new List<RGBModel>();
    }
}