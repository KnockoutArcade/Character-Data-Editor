using System.Collections.Generic;

namespace CharacterDataEditor.Models
{
    public record PaletteModel
    {
        public string Name { get; set; } = string.Empty;
        public List<RGBModel> ColorPalette { get; set; }
    }
}