using Newtonsoft.Json;
using CharacterDataEditor.Models.CharacterData;
using OriginalVersion = CharacterDataEditor.Models.CharacterData.PreviousVersions.Original;
using CharacterDataEditor.Constants;
using CharacterDataEditor.Models;

namespace CharacterDataEditor.Extensions
{
    public static class CharacterDataUpgradeExtensions
    {
        //original to 0.9.4 upgrade
        public static UpgradeResults Upgrade(this OriginalVersion.CharacterDataModel previous)
        {
            // convert to json string
            var characterAsJson = JsonConvert.SerializeObject(previous, Formatting.Indented);
            // convert back to object as new model
            var newCharacter = JsonConvert.DeserializeObject<CharacterDataModel>(characterAsJson);

            // any special conversions go here...
            newCharacter.Version = VersionConstants.Ver094;

            return new UpgradeResults
            {
                UpgradedCharacterData= newCharacter,
                IsDataLossSuspected = true,
                Message = MessageConstants.OriginalTo094UpgradeMessage,
                Success = true
            };
        }
    }
}
