﻿using CharacterDataEditor.Enums;
using CharacterDataEditor.Models.CharacterData;
using CharacterDataEditor.Models.ProjectileData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CharacterDataEditor.Models.ProjectileData
{
    public class ProjectileDataModel : BaseProjectile
    {
        public string Name { get; set; } = string.Empty;
        #region Base Stats
        public float HorizontalSpeed { get; set; } = 0.0f;
        public float VerticalSpeed { get; set; } = 0.0f;
        public int EnvironmentalDisplacement { get; set; } = 0;
        public float FallSpeed { get; set; } = 0.0f;
        public float GroundTraction { get; set; } = 0.0f;
        public float AirTraction { get; set; } = 0.0f;
        public bool DestroyOnFloor { get; set; } = false;
        public bool DestroyOnWall { get; set; } = false;
        public bool BounceOnFloor { get; set; } = false;
        public bool BounceOnWall { get; set; } = false;
        public int NumberOfBounces { get; set; } = 0;
        public float Bounciness { get; set; } = 0.0f;
        public bool Transcendent { get; set; } = false;
        public int Health { get; set; } = 0;
        public string Sprite { get; set; } = string.Empty;
        public string ImpactSprite { get; set; } = string.Empty;
        #endregion
        public ProjectilePaletteModel BaseColor { get; set; } = new ProjectilePaletteModel();
        public int NumberOfPalettes { get { return Palettes?.Count ?? 0; } }
        public List<ProjectilePaletteModel> Palettes { get; set; } = new List<ProjectilePaletteModel>();
        public int NumberOfHitboxes { get { return AttackData?.Count ?? 0; } }
        public List<ProjectileAttackDataModel> AttackData { get; set; } = new List<ProjectileAttackDataModel>();
        public List<ProjectileCounterHitDataModel> CounterData { get; set; } = new List<ProjectileCounterHitDataModel>();
        public ProjectileRehitDataModel RehitData { get; set; } = new ProjectileRehitDataModel();

        public override int GetHashCode()
        {
            var hash = HashCode.Combine(Name, HorizontalSpeed, VerticalSpeed, EnvironmentalDisplacement, FallSpeed, GroundTraction, AirTraction, DestroyOnFloor);
            hash = HashCode.Combine(hash, DestroyOnWall, BounceOnFloor, BounceOnWall, NumberOfBounces, Bounciness, Transcendent, Health);
            hash = HashCode.Combine(hash, Sprite, ImpactSprite, BaseColor, Palettes, NumberOfHitboxes, AttackData, CounterData);
            hash = HashCode.Combine(hash, RehitData);

            return hash;
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

            var objAsProjectileDataModel = (ProjectileDataModel)obj;

            if (objAsProjectileDataModel.Name.Equals(Name) &&
                objAsProjectileDataModel.HorizontalSpeed.Equals(HorizontalSpeed) &&
                objAsProjectileDataModel.VerticalSpeed.Equals(VerticalSpeed) &&
                objAsProjectileDataModel.EnvironmentalDisplacement.Equals(EnvironmentalDisplacement) &&
                objAsProjectileDataModel.FallSpeed.Equals(FallSpeed) &&
                objAsProjectileDataModel.GroundTraction.Equals(GroundTraction) &&
                objAsProjectileDataModel.AirTraction.Equals(AirTraction) &&
                objAsProjectileDataModel.DestroyOnFloor.Equals(DestroyOnFloor) &&
                objAsProjectileDataModel.DestroyOnWall.Equals(DestroyOnWall) &&
                objAsProjectileDataModel.BounceOnFloor.Equals(BounceOnFloor) &&
                objAsProjectileDataModel.BounceOnWall.Equals(BounceOnWall) &&
                objAsProjectileDataModel.NumberOfBounces.Equals(NumberOfBounces) &&
                objAsProjectileDataModel.Bounciness.Equals(Bounciness) &&
                objAsProjectileDataModel.Transcendent.Equals(Transcendent) &&
                objAsProjectileDataModel.Health.Equals(Health) &&
                objAsProjectileDataModel.Sprite.Equals(Sprite) &&
                objAsProjectileDataModel.ImpactSprite.Equals(ImpactSprite) &&
                objAsProjectileDataModel.BaseColor.Equals(BaseColor) &&
                objAsProjectileDataModel.Palettes.SequenceEqual(Palettes) &&
                objAsProjectileDataModel.NumberOfHitboxes.Equals(NumberOfHitboxes) &&
                objAsProjectileDataModel.AttackData.SequenceEqual(AttackData) &&
                objAsProjectileDataModel.CounterData.SequenceEqual(CounterData) &&
                objAsProjectileDataModel.RehitData.Equals(RehitData))
            {
                return true;
            }

            return false;
        }
    }
}
