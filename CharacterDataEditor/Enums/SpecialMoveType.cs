using System;

namespace CharacterDataEditor.Enums
{
    [Flags]
    public enum SpecialMoveType
    {
        None = 0x000000,
        EnhancedNeutral = 0x000001,
        EnhancedSide = 0x000002,
        EnhancedUp = 0x000004,
        EnhancedDown = 0x000008,
        EnhancedNeutral2 = 0x000010,
        EnhancedSide2 = 0x000020,
        EnhancedUp2 = 0x000040,
        EnhancedDown2 = 0x000080,
        RekkaLauncher = 0x000100,
        RekkaFinisher = 0x000200,
        RekkaConnector = 0x000400,
        RekkaLow = 0x000800,
        RekkaHigh = 0x001000
    }
}
