using CharacterDataEditor.Enums;
using CharacterDataEditor.Models.CharacterData;
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
        public bool HasLifetime { get; set; } = false;
        public int Lifetime { get; set; } = 0;
        public int AnimationSpeed { get; set; } = 0;
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
        public ProjectileSpriteCollectionModel ProjectileSprites { get; set; } = new ProjectileSpriteCollectionModel();
        #endregion
        public PaletteModel BaseColor { get; set; } = new PaletteModel();
        public bool CopyCharacterPalette { get; set; } = false;
        public string Character { get; set; } = "Russel";
        public List<PaletteModel> Palettes { get; set; } = new List<PaletteModel>();
        public int NumberOfFrames { get { return FrameData?.Count ?? 0; } }
        public List<FrameDataModel> FrameData { get; set; } = new List<FrameDataModel>();
        public int NumberOfHitboxes { get { return AttackData?.Count ?? 0; } }
        public List<ProjectileAttackDataModel> AttackData { get; set; } = new List<ProjectileAttackDataModel>();
        public List<ProjectileCounterHitDataModel> CounterData { get; set; } = new List<ProjectileCounterHitDataModel>();
        public ProjectileRehitDataModel RehitData { get; set; } = new ProjectileRehitDataModel();

        public override int GetHashCode()
        {
            var hash = HashCode.Combine(Name, HasLifetime, Lifetime, AnimationSpeed, HorizontalSpeed, VerticalSpeed, EnvironmentalDisplacement, FallSpeed);
            hash = HashCode.Combine(hash, GroundTraction, AirTraction, DestroyOnFloor, DestroyOnWall, BounceOnFloor, BounceOnWall, NumberOfBounces);
            hash = HashCode.Combine(hash, Bounciness, Transcendent, Health, ProjectileSprites, BaseColor, CopyCharacterPalette, Character);
            hash = HashCode.Combine(hash, Palettes, NumberOfFrames, FrameData, NumberOfHitboxes, AttackData, CounterData, RehitData);

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
                objAsProjectileDataModel.HasLifetime.Equals(HasLifetime) &&
                objAsProjectileDataModel.Lifetime.Equals(Lifetime) &&
                objAsProjectileDataModel.AnimationSpeed.Equals(AnimationSpeed) &&
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
                objAsProjectileDataModel.ProjectileSprites.Equals(ProjectileSprites) &&
                objAsProjectileDataModel.BaseColor.Equals(BaseColor) &&
                objAsProjectileDataModel.CopyCharacterPalette.Equals(CopyCharacterPalette) &&
                objAsProjectileDataModel.Character.Equals(Character) &&
                objAsProjectileDataModel.Palettes.SequenceEqual(Palettes) &&
                objAsProjectileDataModel.NumberOfHitboxes.Equals(NumberOfHitboxes) &&
                objAsProjectileDataModel.FrameData.SequenceEqual(FrameData) &&
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
