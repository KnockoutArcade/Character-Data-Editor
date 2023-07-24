using Newtonsoft.Json;
using CharacterDataEditor.Models.ProjectileData;
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
                // Right now, this is the original version of the projectile data
                case VersionConstants.CurrentVersion:
                default:
                    return new UpgradeResults
                    {
                        IsDataLossSuspected = false,
                        Message = "No new version",
                        Success = false,
                        UpgradedProjectileData = (originalProjectile as ProjectileDataModel)
                    };
            }
        }
    }
}
