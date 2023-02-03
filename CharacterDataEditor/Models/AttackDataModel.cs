﻿using CharacterDataEditor.Enums;
using System;

namespace CharacterDataEditor.Models
{
    public class AttackDataModel
    {
        public int Start { get; set; } = 0;
        public int Lifetime { get; set; } = 0;
        public int AttackWidth { get; set; } = 0;
        public int AttackHeight { get; set; } = 0;
        public int WidthOffset { get; set; } = 0;
        public int HeightOffset { get; set; } = 0;
        public int Group { get; set; } = 0;
        public int Damage { get; set; } = 0;
        public int AttackHitStop { get; set; } = 0;
        public int AttackHitStun { get; set; } = 0;
        public AttackType AttackType { get; set; } = AttackType.Low;
        public int BlockStun { get; set; } = 0;
        public int KnockBack { get; set; } = 0;
        public int AirKnockbackVertical { get; set; } = 0;
        public int AirKnockbackHorizontal { get; set; } = 0;
        public bool Launches { get; set; } = false;
        public int LaunchKnockbackVertical { get; set; } = 0;
        public int LaunchKnockbackHorizontal { get; set; } = 0;
        public int Pushback { get; set; } = 0;
        public int ParticleXOffset { get; set; } = 0;
        public int ParticleYOffset { get; set; } = 0;
        public string ParticleEffect { get; set; } = string.Empty;
        public int ParticleDuration { get; set; } = 0;
        public int HoldXOffset { get; set; } = 0;
        public int HoldYOffset { get; set; } = 0;

        public override int GetHashCode()
        {
            var hash = HashCode.Combine(Start, Lifetime, AttackWidth, AttackHeight, WidthOffset, HeightOffset, Group, Damage);
            hash = HashCode.Combine(hash, AttackHitStop, AttackHitStun, AttackType, BlockStun, KnockBack, AirKnockbackHorizontal, AirKnockbackVertical);
            hash = HashCode.Combine(hash, Launches, LaunchKnockbackHorizontal, LaunchKnockbackVertical, Pushback, ParticleXOffset, ParticleYOffset, ParticleEffect);
            hash = HashCode.Combine(hash, ParticleDuration, HoldXOffset, HoldYOffset);

            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(AttackDataModel))
            {
                return false;
            }

            var objAsAttackData = obj as AttackDataModel;

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
                                if (objAsAttackData.Group == Group)
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

            return false;
        }
    }
}