using Newtonsoft.Json;
using CharacterDataEditor.Models.CharacterData;
using OriginalVersion = CharacterDataEditor.Models.CharacterData.PreviousVersions.Original;
using Ver095 = CharacterDataEditor.Models.CharacterData.PreviousVersions.Ver095;
using Ver102 = CharacterDataEditor.Models.CharacterData.PreviousVersions.Ver102;
using CharacterDataEditor.Constants;
using CharacterDataEditor.Models;
using System.Collections.Generic;
using System.Linq;
using CharacterDataEditor.Enums;
using System;

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
                case VersionConstants.Ver102:
                    return (originalCharacter as Ver102.CharacterDataModel).Upgrade102to110();
                case VersionConstants.Ver101:
                    return (originalCharacter as Ver102.CharacterDataModel).Upgrade101to102();
                case VersionConstants.Ver1:
                    return (originalCharacter as Ver102.CharacterDataModel).Upgrade100to101();
                case VersionConstants.Ver096:
                    return (originalCharacter as Ver102.CharacterDataModel).Upgrade096to100();
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

            if (previousOperationResults == null)
            {
                previousOperationResults = new UpgradeResults
                {
                    UpgradedCharacterData = null,
                    IsDataLossSuspected = false,
                    Message = string.Empty,
                    Success = true
                };
            }

            return previous.Upgrade095to096(previousOperationResults);
        }

        private static UpgradeResults Upgrade095to096(this Ver095.CharacterDataModel previous, UpgradeResults previousOperationResults = null)
        {
            // only breaking change between versions is the removal of "Base sprite" and the addition of the sprite list
            // so we only need to migrate base sprite into the idle animation

            var characterAsJson = JsonConvert.SerializeObject(previous, Formatting.Indented);
            // convert back to object as new model
            var newCharacter = JsonConvert.DeserializeObject<Ver102.CharacterDataModel>(characterAsJson);

            newCharacter.CharacterSprites.Idle = previous.BaseSprite;
            newCharacter.Version = VersionConstants.Ver096;

            return newCharacter.Upgrade096to100(new UpgradeResults
            {
                UpgradedCharacterData = null,
                IsDataLossSuspected = (previousOperationResults != null) ? previousOperationResults.IsDataLossSuspected : false,
                Message = (previousOperationResults != null) ? previousOperationResults.Message : string.Empty,
                Success = true
            });
        }

        private static UpgradeResults Upgrade096to100(this Ver102.CharacterDataModel previous, UpgradeResults previousOperationResults = null)
        {
            foreach (var move in previous.MoveData)
            {
                if (move.IsThrow && move.OpponentPositionData != null)
                {
                    move.OpponentPositionData.ThrowOffset = 0;
                }
            }

            previous.Version = VersionConstants.Ver1;

            return previous.Upgrade100to101(new UpgradeResults
            {
                UpgradedCharacterData = null,
                IsDataLossSuspected = (previousOperationResults != null) ? previousOperationResults.IsDataLossSuspected : false,
                Message = (previousOperationResults != null) ? previousOperationResults.Message : string.Empty,
                Success = true
            });
        }

        private static UpgradeResults Upgrade100to101(this Ver102.CharacterDataModel previous, UpgradeResults previousOperationResults = null)
        {
            foreach (var move in previous.MoveData)
            {
                move.ProjectileData = new List<ProjectileDataModel>();
            }

            previous.Version = VersionConstants.Ver101;

            return previous.Upgrade101to102(new UpgradeResults
            {
                UpgradedCharacterData = null,
                IsDataLossSuspected = (previousOperationResults != null) ? previousOperationResults.IsDataLossSuspected : false,
                Message = (previousOperationResults != null) ? previousOperationResults.Message : string.Empty,
                Success = true
            });
        }

        private static UpgradeResults Upgrade101to102(this Ver102.CharacterDataModel previous, UpgradeResults previousOperationResults = null)
        {
            //convert the rgb 255 values to rgb 1 values
            foreach (var color in previous.BaseColor.ColorPalette)
            {
                color.Red = color.Red / 255f;
                color.Green = color.Green / 255f;
                color.Blue = color.Blue / 255f;
            }

            foreach (var palette in previous.Palettes)
            {
                foreach (var color in palette.ColorPalette)
                {
                    color.Red = color.Red / 255f;
                    color.Green = color.Green / 255f;
                    color.Blue = color.Blue / 255f;
                }
            }

            previous.Version = VersionConstants.Ver102;

            return previous.Upgrade102to110(new UpgradeResults
            {
                UpgradedCharacterData = null,
                IsDataLossSuspected = (previousOperationResults != null) ? previousOperationResults.IsDataLossSuspected : false,
                Message = (previousOperationResults != null) ? previousOperationResults.Message : string.Empty,
                Success = true
            });
        }

        private static UpgradeResults Upgrade102to110(this Ver102.CharacterDataModel previous, UpgradeResults previousOperationResults = null)
        {
            // convert to json string
            var characterAsJson = JsonConvert.SerializeObject(previous, Formatting.Indented);
            // convert back to object as new model
            var newCharacter = JsonConvert.DeserializeObject<CharacterDataModel>(characterAsJson);

            // the breaking change this time was with the move types, since the values are different
            // we'll compare by the string version... which should match
            foreach (var move in previous.MoveData)
            {
                var oldMoveType = move.MoveType.ToString();

                var newMove = newCharacter.MoveData.First(x => x.UID == move.UID);

                if (newMove != null)
                {
                    // this feels unsafe, but is apparently the correct way to do it
                    newMove.MoveType = (MoveType)Enum.Parse(typeof(MoveType), oldMoveType);
                }
            }

            newCharacter.Version = VersionConstants.Ver110;

            return new UpgradeResults
            {
                UpgradedCharacterData = newCharacter,
                IsDataLossSuspected = (previousOperationResults != null) ? previousOperationResults.IsDataLossSuspected : false,
                Message = (previousOperationResults != null) ? previousOperationResults.Message : string.Empty,
                Success = true
            };
        }
    }
}
