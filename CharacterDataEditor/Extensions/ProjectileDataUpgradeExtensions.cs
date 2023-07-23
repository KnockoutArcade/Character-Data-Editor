using Newtonsoft.Json;
using CharacterDataEditor.Models.ProjectileData;
using OriginalVersion = CharacterDataEditor.Models.CharacterData.PreviousVersions.Original;
using Ver095 = CharacterDataEditor.Models.CharacterData.PreviousVersions.Ver095;
using Ver103 = CharacterDataEditor.Models.CharacterData.PreviousVersions.Ver103;
using CharacterDataEditor.Constants;
using CharacterDataEditor.Models;
using System.Collections.Generic;
using System.Linq;
using CharacterDataEditor.Enums;
using System;

namespace CharacterDataEditor.Extensions
{
    public static class ProjectileDataUpgradeExtensions
    {
        public static UpgradeResults Upgrade<T>(this T originalProjectile) where T : BaseProjectile
        {
            switch (originalProjectile.Version)
            {
                case VersionConstants.CurrentVersion:
                    return new UpgradeResults
                    {
                        IsDataLossSuspected = false,
                        Message = "No new version",
                        Success = false,
                        UpgradedProjectileData = (originalProjectile as ProjectileDataModel)
                    };
                case VersionConstants.Ver113:
                    return (originalCharacter as CharacterDataModel).Upgrade113to114();
                case VersionConstants.Ver112:
                    return (originalCharacter as CharacterDataModel).Upgrade112to113();
                case VersionConstants.Ver111:
                    return (originalCharacter as CharacterDataModel).Upgrade111to112();
                case VersionConstants.Ver110:
                    return (originalCharacter as CharacterDataModel).Upgrade110to111();
                case VersionConstants.Ver103:
                    return (originalCharacter as Ver103.CharacterDataModel).Upgrade103to110();
                case VersionConstants.Ver102:
                    return (originalCharacter as Ver103.CharacterDataModel).Upgrade102to103();
                case VersionConstants.Ver101:
                    return (originalCharacter as Ver103.CharacterDataModel).Upgrade101to102();
                case VersionConstants.Ver1:
                    return (originalCharacter as Ver103.CharacterDataModel).Upgrade100to101();
                case VersionConstants.Ver096:
                    return (originalCharacter as Ver103.CharacterDataModel).Upgrade096to100();
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

            return new UpgradeResults
            {
                UpgradedProjectileData = ProjectileDataModel,
                IsDataLossSuspected = (previousOperationResults != null) ? previousOperationResults.IsDataLossSuspected : false,
                Message = (previousOperationResults != null) ? previousOperationResults.Message : string.Empty,
                Success = true
            };
        }
    }
}
