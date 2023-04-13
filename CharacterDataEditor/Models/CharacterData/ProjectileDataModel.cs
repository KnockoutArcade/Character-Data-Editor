﻿using System;

namespace CharacterDataEditor.Models.CharacterData
{
    public class ProjectileDataModel
    {
        public int SpawnFrame { get; set; } = 0;
        public int SpawnXOffset { get; set; } = 0;
        public int SpawnYOffset { get; set; } = 0;
        public string ProjectileObject { get; set; } = string.Empty;

        public override int GetHashCode()
        {
            return HashCode.Combine(SpawnFrame, SpawnXOffset, SpawnYOffset, ProjectileObject);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(ProjectileDataModel))
            {
                return false;
            }

            var objAsProjectileData = obj as ProjectileDataModel;

            if (objAsProjectileData.ProjectileObject == ProjectileObject)
            {
                if (objAsProjectileData.SpawnFrame == SpawnFrame)
                {
                    if (objAsProjectileData.SpawnYOffset == SpawnYOffset)
                    {
                        if (objAsProjectileData.SpawnXOffset == SpawnXOffset)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}