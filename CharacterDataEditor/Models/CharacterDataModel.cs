using Newtonsoft.Json;
using System.Collections.Generic;

namespace CharacterDataEditor.Models
{
    public record CharacterDataModel
    {
        public string Name { get; set; }
        public string BaseSprite { get; set; }
        public PaletteModel BaseColor { get; set; }
        public List<PaletteModel> Palettes { get; set; }
        public List<MoveDataModel> MoveData { get; set; }
    }
}
