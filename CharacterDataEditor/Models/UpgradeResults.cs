using CharacterDataEditor.Models.CharacterData;

namespace CharacterDataEditor.Models
{
    public class UpgradeResults
    {
        public CharacterDataModel UpgradedCharacterData { get; set; }
        public bool IsDataLossSuspected { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
