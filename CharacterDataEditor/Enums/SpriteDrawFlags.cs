using System;

namespace CharacterDataEditor.Models
{
    [Flags]
    public enum SpriteDrawFlags
    {
        None = 0x00,
        CenterVertical = 0x01,
        CenterHorizontal = 0x02,
        ShowSpriteOutline = 0x04,
        NotAnimated = 0x08,
        Pause = 0x10,
        PaletteSwapActive = 0x20
    }
}
