using CharacterDataEditor.Enums;
using CharacterDataEditor.Models.CharacterData;
using CharacterDataEditor.Models.ProjectileData;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Numerics;

namespace CharacterDataEditor.Models
{
    public class SpriteDrawDataModel
    {
        public Vector2 DrawPosition { get; set; }
        public Vector2 Origin { get; set; }
        public float Scale { get; set; }
        public Vector2 MaxDrawSize { get; set; }
        public SpriteDrawFlags Flags { get; set; }
        public PaletteModel BaseColor { get; set; }
        public PaletteModel SwapColor { get; set; }
        public SpriteDataModel SpriteData { get; set; }
        public ILogger Logger { get; set; }
        public FrameAdvance FrameAdvance { get; set; }
        public string DefaultTexture { get; set; }
        public bool EnableFrameDataDraw { get; set; } = false;
        public List<FrameDataModel> FrameDrawData { get; set; }
    }
}
