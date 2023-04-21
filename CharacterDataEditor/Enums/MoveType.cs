using System;

namespace CharacterDataEditor.Enums
{
    [Flags]
    public enum MoveType
    {
        BackwardThrow = 0x00000,
        ForwardThrow = 0x00001,
        Grab = 0x00002,
        SideSpecial = 0x00004,
        NeutralSpecial = 0x00008,
        StandingLight = 0x00010,
        StandingLight2 = 0x00020,
        StandingLight3 = 0x00040,
        StandingMedium = 0x00080,
        StandingHeavy = 0x00100,
        CrouchingLight = 0x00200,
        CrouchingMedium = 0x00400,
        CrouchingHeavy = 0x00800,
        JumpingLight = 0x01000,
        JumpingMedium = 0x02000,
        JumpingHeavy = 0x04000,
        UpSpecial = 0x08000,
        DownSpecial = 0x10000,
        CommandGrab = 0x20000
    }
}