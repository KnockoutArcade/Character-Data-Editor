using Newtonsoft.Json;
using System.Collections.Generic;

namespace CharacterDataEditor.Models
{
    public record CharacterDataModel
    {
        public string Name { get; set; } = string.Empty;
        public string BaseSprite { get; set; } = string.Empty;
        public PaletteModel BaseColor { get; set; } = new PaletteModel();
        public int NumberOfPalettes { get { return Palettes?.Count ?? 0; } }
        public List<PaletteModel> Palettes { get; set; } = new List<PaletteModel>();
        public List<MoveDataModel> MoveData { get; set; } = new List<MoveDataModel>();
    }
}
