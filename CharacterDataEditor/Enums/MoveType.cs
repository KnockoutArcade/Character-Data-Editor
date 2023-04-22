using System;

namespace CharacterDataEditor.Enums
{
    [Flags]
    public enum MoveType
    {
        None = 0x00000,
        BackwardThrow = 0x00001,
        ForwardThrow = 0x00002,
        Grab = 0x00004,
        SideSpecial = 0x00008,
        NeutralSpecial = 0x00010,
        StandingLight = 0x00020,
        StandingLight2 = 0x00040,
        StandingLight3 = 0x00080,
        StandingMedium = 0x00100,
        StandingHeavy = 0x00200,
        CrouchingLight = 0x00400,
        CrouchingMedium = 0x00800,
        CrouchingHeavy = 0x01000,
        JumpingLight = 0x02000,
        JumpingMedium = 0x04000,
        JumpingHeavy = 0x08000,
        UpSpecial = 0x10000,
        DownSpecial = 0x20000,
        CommandGrab = 0x40000
    }
}