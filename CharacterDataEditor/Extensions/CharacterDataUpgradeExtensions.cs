using Newtonsoft.Json;
using CharacterDataEditor.Models.CharacterData;
using OriginalVersion = CharacterDataEditor.Models.CharacterData.PreviousVersions.Original;
using CharacterDataEditor.Constants;
using CharacterDataEditor.Models;

namespace CharacterDataEditor.Extensions
{
    public static class CharacterDataUpgradeExtensions
    {
        public static UpgradeResults Upgrade<T>(this T originalCharacter) where T : BaseCharacter
        {
            switch (originalCharacter.Version)
            {
                case VersionConstants.CurrentVersion:
                    return new UpgradeResults 
                    { 
                        IsDataLossSuspected = false,
                        Message = "No new version",
                        Success = false,
                        UpgradedCharacterData = (originalCharacter as CharacterDataModel)
                    };
                case VersionConstants.Ver094:
                    return (originalCharacter as CharacterDataModel).Upgrade094to095();
                case VersionConstants.Original:
                default:
                    return (originalCharacter as OriginalVersion.CharacterDataModel).UpgradeOrigTo094();
            }
        }

        //original to 0.9.4 upgrade
        private static UpgradeResults UpgradeOrigTo094(this OriginalVersion.CharacterDataModel previous)
        {
            // convert to json string
            var characterAsJson = JsonConvert.SerializeObject(previous, Formatting.Indented);
            // convert back to object as new model
            var newCharacter = JsonConvert.DeserializeObject<CharacterDataModel>(characterAsJson);

            // any special conversions go here...
            newCharacter.Version = VersionConstants.Ver094;

            return newCharacter.Upgrade094to095(new UpgradeResults
            {
                UpgradedCharacterData = newCharacter,
                IsDataLossSuspected = true,
                Message = MessageConstants.OriginalTo094UpgradeMessage,
                Success = true
            });
        }

        private static UpgradeResults Upgrade094to095(this CharacterDataModel previous, UpgradeResults previousOperationResults = null)
        {
            //094 and 095 have no breaking changes, this just manually creates the missing data structures
            foreach (var move in previous.MoveData)
            {
                if (move.CounterData.Count < move.NumberOfHitboxes)
                {
                    while (move.CounterData.Count < move.NumberOfHitboxes)
                    {
                        move.CounterData.Add(new CounterHitDataModel());
                    }
                }
            }

            previous.Version = VersionConstants.Ver095;

            return new UpgradeResults
            {
                UpgradedCharacterData = previous,
                IsDataLossSuspected = (previousOperationResults != null) ? previousOperationResults.IsDataLossSuspected : false,
                Message = (previousOperationResults != null) ? previousOperationResults.Message : string.Empty,
                Success = true
            };
        }
    }
}
