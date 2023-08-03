using System;

namespace CharacterDataEditor.Enums
{
    [Flags]
    public enum MoveType
    {
        None = 0x000000,
        BackwardThrow = 0x000001,
        ForwardThrow = 0x000002,
        Grab = 0x000004,
        SideSpecial = 0x000008,
        NeutralSpecial = 0x000010,
        StandingLight = 0x000020,
        StandingLight2 = 0x000040,
        StandingLight3 = 0x000080,
        StandingMedium = 0x000100,
        StandingHeavy = 0x000200,
        CrouchingLight = 0x000400,
        CrouchingMedium = 0x000800,
        CrouchingHeavy = 0x001000,
        JumpingLight = 0x002000,
        JumpingMedium = 0x004000,
        JumpingHeavy = 0x008000,
        UpSpecial = 0x010000,
        DownSpecial = 0x020000,
        CommandGrab = 0x040000,
        CommandNormal1 = 0x080000,
        CommandNormal2 = 0x100000,
        CommandNormal3 = 0x200000,
        Super = 0x400000
    }
}