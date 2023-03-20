using Newtonsoft.Json;
using CharacterDataEditor.Models.CharacterData;
using OriginalVersion = CharacterDataEditor.Models.CharacterData.PreviousVersions.Original;
using Ver095 = CharacterDataEditor.Models.CharacterData.PreviousVersions.Ver095;
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
                case VersionConstants.Ver096:
                    return (originalCharacter as CharacterDataModel).Upgrade096to100();
                case VersionConstants.Ver095:
                    return (originalCharacter as Ver095.CharacterDataModel).Upgrade095to096();
                case VersionConstants.Ver094:
                    return (originalCharacter as Ver095.CharacterDataModel).Upgrade094to095();
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
            var newCharacter = JsonConvert.DeserializeObject<Ver095.CharacterDataModel>(characterAsJson);

            // any special conversions go here...
            newCharacter.Version = VersionConstants.Ver094;

            return newCharacter.Upgrade094to095(new UpgradeResults
            {
                UpgradedCharacterData = null,
                IsDataLossSuspected = true,
                Message = MessageConstants.OriginalTo094UpgradeMessage,
                Success = true
            });
        }

        private static UpgradeResults Upgrade094to095(this Ver095.CharacterDataModel previous, UpgradeResults previousOperationResults = null)
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

            return previous.Upgrade095to096(previousOperationResults);
        }

        private static UpgradeResults Upgrade095to096(this Ver095.CharacterDataModel previous, UpgradeResults previousOperationResults = null)
        {
            // only breaking change between versions is the removal of "Base sprite" and the addition of the sprite list
            // so we only need to migrate base sprite into the idle animation

            var characterAsJson = JsonConvert.SerializeObject(previous, Formatting.Indented);
            // convert back to object as new model
            var newCharacter = JsonConvert.DeserializeObject<CharacterDataModel>(characterAsJson);

            newCharacter.CharacterSprites.Idle = previous.BaseSprite;
            newCharacter.Version = VersionConstants.Ver096;

            return new UpgradeResults
            {
                UpgradedCharacterData = newCharacter,
                IsDataLossSuspected = (previousOperationResults != null) ? previousOperationResults.IsDataLossSuspected : false,
                Message = (previousOperationResults != null) ? previousOperationResults.Message : string.Empty,
                Success = true
            };
        }

        private static UpgradeResults Upgrade096to100(this CharacterDataModel previous, UpgradeResults previousOperationResults = null)
        {
            foreach (var move in previous.MoveData)
            {
                if (move.IsThrow && move.OpponentPositionData != null)
                {
                    move.OpponentPositionData.ThrowOffset = 0;
                }
            }

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
