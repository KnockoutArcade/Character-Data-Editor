using Newtonsoft.Json;
using CharacterDataEditor.Models.ProjectileData;
using CharacterDataEditor.Constants;
using CharacterDataEditor.Models;
using System.Collections.Generic;
using System.Linq;
using CharacterDataEditor.Enums;
using System;
using CharacterDataEditor.Models.CharacterData;

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
                case VersionConstants.Ver120:
                    return (originalProjectile as ProjectileDataModel).Upgrade120to121();
                case VersionConstants.Ver114:
                default:
                    return (originalProjectile as ProjectileDataModel).Upgrade114to120();
            }
        }

        private static UpgradeResults Upgrade114to120(this ProjectileDataModel previous, UpgradeResults previousOperationResults = null)
        {
            previous.Version = VersionConstants.Ver120;

            return new UpgradeResults
            {
                UpgradedProjectileData = previous,
                IsDataLossSuspected = (previousOperationResults != null) ? previousOperationResults.IsDataLossSuspected : false,
                Message = (previousOperationResults != null) ? previousOperationResults.Message : string.Empty,
                Success = true
            };
        }

        private static UpgradeResults Upgrade120to121(this ProjectileDataModel previous, UpgradeResults previousOperationResults = null)
        {
            previous.Version = VersionConstants.Ver121;

            return new UpgradeResults
            {
                UpgradedProjectileData = previous,
                IsDataLossSuspected = (previousOperationResults != null) ? previousOperationResults.IsDataLossSuspected : false,
                Message = (previousOperationResults != null) ? previousOperationResults.Message : string.Empty,
                Success = true
            };
        }
    }
}
