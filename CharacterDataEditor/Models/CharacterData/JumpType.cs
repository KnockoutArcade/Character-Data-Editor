using System;

namespace CharacterDataEditor.Models.CharacterData
{
    [Flags]
    public enum JumpType
    {
        None = 0x00,
        DoubleJump = 0x01,
        SuperJump = 0x02,
        ShortHop = 0x04
    }
}