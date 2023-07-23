using CharacterDataEditor.Models.CharacterData;
using CharacterDataEditor.Models.ProjectileData;

namespace CharacterDataEditor.Models
{
    public class UpgradeResults
    {
        public CharacterDataModel UpgradedCharacterData { get; set; }
        public ProjectileDataModel UpgradedProjectileData { get; set; }
        public bool IsDataLossSuspected { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
