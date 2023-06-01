using System;

namespace CharacterDataEditor.Enums
{
    [Flags]
    public enum SpecialMoveType
    {
        None = 0x000000,
        EnhancedNSpecial = 0x000001,
        EnhancedSSpecial = 0x000002,
        EnhancedUSpecial = 0x000004,
        EnhancedDSpecial = 0x000008,
        EnhancedNSpecial2 = 0x000010,
        EnhancedSSpecial2 = 0x000020,
        EnhancedUSpecial2 = 0x000040,
        EnhancedDSpecial2 = 0x000080,
        RekkaLauncher = 0x000100,
        RekkaFinisher = 0x000200,
        RekkaConnector = 0x000400,
        RekkaLow = 0x000800,
        RekkaHigh = 0x001000
    }
}
