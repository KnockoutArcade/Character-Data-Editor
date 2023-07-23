using CharacterDataEditor.Enums;
using System;

namespace CharacterDataEditor.Models.ProjectileData
{
    public class ProjectileAttackDataModel
    {
        public int Start { get; set; } = 0;
        public int Lifetime { get; set; } = 0;
        public int AttackWidth { get; set; } = 0;
        public int AttackHeight { get; set; } = 0;
        public int WidthOffset { get; set; } = 0;
        public int HeightOffset { get; set; } = 0;
        public int Damage { get; set; } = 0;
        public float MeterGain { get; set; } = 0.0f;
        public float ComboScaling { get; set; } = 0.0f;
        public int AttackHitStop { get; set; } = 0;
        public int AttackHitStun { get; set; } = 0;
        public AttackType AttackType { get; set; } = AttackType.Low;
        public float BlockStun { get; set; } = 0;
        public float KnockBack { get; set; } = 0;
        public float AirKnockbackVertical { get; set; } = 0.0f;
        public float AirKnockbackHorizontal { get; set; } = 0.0f;
        public bool Launches { get; set; } = false;
        public float LaunchKnockbackVertical { get; set; } = 0.0f;
        public float LaunchKnockbackHorizontal { get; set; } = 0.0f;
        public float GravityScaling { get; set; } = 0.0f;
        public float Pushback { get; set; } = 0;
        public int ParticleXOffset { get; set; } = 0;
        public int ParticleYOffset { get; set; } = 0;
        public string ParticleEffect { get; set; } = string.Empty;
        public int ParticleDuration { get; set; } = 0;
        public int HoldXOffset { get; set; } = 0;
        public int HoldYOffset { get; set; } = 0;
        public bool CausesWallbounce { get; set; } = false;
        public string HitSound { get; set; } = "";

        public override int GetHashCode()
        {
            var hash = HashCode.Combine(Start, Lifetime, AttackWidth, AttackHeight, WidthOffset, HeightOffset, Damage, MeterGain);
            hash = HashCode.Combine(hash, ComboScaling, AttackHitStop, AttackHitStun, AttackType, BlockStun, KnockBack, AirKnockbackHorizontal);
            hash = HashCode.Combine(hash, AirKnockbackVertical, Launches, LaunchKnockbackHorizontal, LaunchKnockbackVertical, GravityScaling, Pushback, ParticleXOffset);
            hash = HashCode.Combine(hash, ParticleYOffset, ParticleEffect, ParticleDuration, HoldXOffset, HoldYOffset, CausesWallbounce, HitSound);

            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(ProjectileAttackDataModel))
            {
                return false;
            }

            var objAsAttackData = obj as ProjectileAttackDataModel;

            if (objAsAttackData.Start == Start)
            {
                if (objAsAttackData.Lifetime == Lifetime)
                {
                    if (objAsAttackData.AttackWidth == AttackWidth)
                    {
                        if (objAsAttackData.AttackHeight == AttackHeight)
                        {
                            if (objAsAttackData.WidthOffset == WidthOffset)
                            {
                                if (objAsAttackData.Damage == Damage)
                                {
                                    if (objAsAttackData.AttackHitStop == AttackHitStop)
                                    {
                                        if (objAsAttackData.AttackHitStun == AttackHitStun)
                                        {
                                            if (objAsAttackData.AttackType == AttackType)
                                            {
                                                if (objAsAttackData.BlockStun == BlockStun)
                                                {
                                                    if (objAsAttackData.KnockBack == KnockBack)
                                                    {
                                                        if (objAsAttackData.AirKnockbackHorizontal == AirKnockbackHorizontal)
                                                        {
                                                            if (objAsAttackData.AirKnockbackVertical == AirKnockbackVertical)
                                                            {
                                                                if (objAsAttackData.Launches == Launches)
                                                                {
                                                                    if (objAsAttackData.LaunchKnockbackHorizontal == LaunchKnockbackHorizontal)
                                                                    {
                                                                        if (objAsAttackData.LaunchKnockbackVertical == LaunchKnockbackVertical)
                                                                        {
                                                                            if (objAsAttackData.Pushback == Pushback)
                                                                            {
                                                                                if (objAsAttackData.ParticleDuration == ParticleDuration)
                                                                                {
                                                                                    if (objAsAttackData.ParticleEffect == ParticleEffect)
                                                                                    {
                                                                                        if (objAsAttackData.ParticleXOffset == ParticleXOffset)
                                                                                        {
                                                                                            if (objAsAttackData.ParticleYOffset == ParticleYOffset)
                                                                                            {
                                                                                                if (objAsAttackData.HoldXOffset == HoldXOffset)
                                                                                                {
                                                                                                    if (objAsAttackData.HoldYOffset == HoldYOffset)
                                                                                                    {
                                                                                                        if (objAsAttackData.MeterGain == MeterGain)
                                                                                                        {
                                                                                                            if (objAsAttackData.ComboScaling == ComboScaling)
                                                                                                            {
                                                                                                                if (objAsAttackData.CausesWallbounce == CausesWallbounce)
                                                                                                                {
                                                                                                                    if (objAsAttackData.HitSound == HitSound)
                                                                                                                    {
                                                                                                                        if (objAsAttackData.GravityScaling == GravityScaling)
                                                                                                                        {
                                                                                                                            return true;
                                                                                                                        }
                                                                                                                    }
                                                                                                                }
                                                                                                            }
                                                                                                        }
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}
