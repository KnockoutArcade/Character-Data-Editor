using System;

namespace CharacterDataEditor.Models.CharacterData
{
    public class CounterHitDataModel
    {
        public int CounterHitLevel { get; set; } = 0;
        public int Group { get; set; } = 0;
        public int Damage { get; set; } = 0;
        public float MeterGain { get; set; } = 0;
        public int AttackHitStop { get; set; } = 0;
        public int AttackHitStun { get; set; } = 0;
        public float KnockBack { get; set; } = 0;
        public float AirKnockbackVertical { get; set; } = 0;
        public float AirKnockbackHorizontal { get; set; } = 0;
        public float Pushback { get; set; } = 0;
        public int ParticleXOffset { get; set; } = 0;
        public int ParticleYOffset { get; set; } = 0;
        public string ParticleEffect { get; set; } = string.Empty;
        public int ParticleDuration { get; set; } = 0;
        public bool Launches { get; set; } = false;
        public float LaunchKnockbackVertical { get; set; } = 0.0f;
        public float LaunchKnockbackHorizontal { get; set; } = 0.0f;
        public float ComboScaling { get; set; } = 0.0f;
        public bool CausesWallbounce { get; set; } = false;

        public override int GetHashCode()
        {
            var hash = HashCode.Combine(CounterHitLevel, Group, Damage, MeterGain, AttackHitStop, AttackHitStun, KnockBack, AirKnockbackVertical);
            hash = HashCode.Combine(hash, AirKnockbackHorizontal, Pushback, ParticleXOffset, ParticleYOffset, ParticleEffect, ParticleDuration, Launches);
            hash = HashCode.Combine(hash, LaunchKnockbackVertical, LaunchKnockbackHorizontal, ComboScaling, CausesWallbounce);

            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(CounterHitDataModel))
            {
                return false;
            }

            var objAsCounterHitData = obj as CounterHitDataModel;

            if (CounterHitLevel == objAsCounterHitData.CounterHitLevel)
            {
                if (Group == objAsCounterHitData.Group)
                {
                    if (Damage == objAsCounterHitData.Damage)
                    {
                        if (MeterGain == objAsCounterHitData.MeterGain)
                        {
                            if (AttackHitStop == objAsCounterHitData.AttackHitStop)
                            {
                                if (AttackHitStun == objAsCounterHitData.AttackHitStun)
                                {
                                    if (KnockBack == objAsCounterHitData.KnockBack)
                                    {
                                        if (AirKnockbackVertical == objAsCounterHitData.AirKnockbackVertical)
                                        {
                                            if (AirKnockbackHorizontal == objAsCounterHitData.AirKnockbackHorizontal)
                                            {
                                                if (Pushback == objAsCounterHitData.Pushback)
                                                {
                                                    if (ParticleXOffset == objAsCounterHitData.ParticleXOffset)
                                                    {
                                                        if (ParticleYOffset == objAsCounterHitData.ParticleYOffset)
                                                        {
                                                            if (ParticleEffect == objAsCounterHitData.ParticleEffect)
                                                            {
                                                                if (ParticleDuration == objAsCounterHitData.ParticleDuration)
                                                                {
                                                                    if (Launches == objAsCounterHitData.Launches)
                                                                    {
                                                                        if (LaunchKnockbackVertical == objAsCounterHitData.LaunchKnockbackVertical)
                                                                        {
                                                                            if (LaunchKnockbackHorizontal == objAsCounterHitData.LaunchKnockbackHorizontal)
                                                                            {
                                                                                if (ComboScaling == objAsCounterHitData.ComboScaling)
                                                                                {
                                                                                    if (CausesWallbounce == objAsCounterHitData.CausesWallbounce)
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

            return false;
        }
    }
}