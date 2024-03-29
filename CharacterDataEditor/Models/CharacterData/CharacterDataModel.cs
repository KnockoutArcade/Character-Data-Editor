﻿using CharacterDataEditor.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CharacterDataEditor.Models.CharacterData
{
    public class CharacterDataModel : BaseCharacter
    {
        public string Name { get; set; } = string.Empty;
        #region Base Stats
        // health
        public int MaxHitPoints { get; set; } = 100;
        public float RegenSpeed { get; set; } = 0.0f;
        public float KORegenSpeed { get; set; } = 0.0f;
        // movement
        public float HorizontalSpeed { get; set; } = 0.0f;
        public float VerticalSpeed { get; set; } = 0.0f;
        public int EnvironmentalDisplacement { get; set; } = 0;
        public float WalkSpeed { get; set; } = 0.0f;
        public float RunSpeed { get; set; } = 0.0f;
        public float Traction { get; set; } = 0.0f;
        public float JumpSpeed { get; set; } = 0.0f;
        public float FallSpeed { get; set; } = 0.0f;
        // backdash
        public int BackDashDuration { get; set; } = 0;
        public int BackDashInvincibility { get; set; } = 0;
        public float BackDashSpeed { get; set; } = 0.0f;
        public float BackDashStartup { get; set; } = 0.0f;
        // misc
        public float FastFallSpeed { get; set; } = 0.0f;
        public JumpType JumpType { get; set; } = JumpType.None;
        public float JumpHorizontalSpeed { get; set; } = 0.0f;
        public CharacterSpriteCollectionModel CharacterSprites { get; set; } = new CharacterSpriteCollectionModel();
        public float SuperMeterBuildRate { get; set; } = 0.0f;
        #endregion
        public PaletteModel BaseColor { get; set; } = new PaletteModel();
        public int NumberOfPalettes { get { return Palettes?.Count ?? 0; } }
        public List<PaletteModel> Palettes { get; set; } = new List<PaletteModel>();
        public List<MoveDataModel> MoveData { get; set; } = new List<MoveDataModel>();
        public UniqueDataModel UniqueData { get; set; } = new UniqueDataModel();
        public NonmoveSoundDataModel NonmoveSoundData { get; set; } = new NonmoveSoundDataModel();

        public override int GetHashCode()
        {
            var hash = HashCode.Combine(Name, HorizontalSpeed, VerticalSpeed, EnvironmentalDisplacement, WalkSpeed, BaseColor, Palettes, UniqueData);
            hash = HashCode.Combine(hash, MoveData, RunSpeed, Traction, JumpSpeed, FallSpeed, BackDashDuration, BackDashInvincibility);
            hash = HashCode.Combine(hash, BackDashSpeed, BackDashStartup, FastFallSpeed, JumpType, JumpHorizontalSpeed, CharacterSprites, SuperMeterBuildRate);
            hash = HashCode.Combine(hash, MaxHitPoints, NonmoveSoundData, RegenSpeed, KORegenSpeed);

            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(CharacterDataModel))
            {
                return false;
            }

            var objAsCharacterDataModel = (CharacterDataModel)obj;

            if (objAsCharacterDataModel.Name.Equals(Name))
            {
                if (objAsCharacterDataModel.BaseColor.Equals(BaseColor))
                {
                    if (objAsCharacterDataModel.Palettes.SequenceEqual(Palettes))
                    {
                        if (objAsCharacterDataModel.UniqueData.Equals(UniqueData))
                        {
                            if (objAsCharacterDataModel.MoveData.SequenceEqual(MoveData))
                            {
                                if (objAsCharacterDataModel.HorizontalSpeed.Equals(HorizontalSpeed) &&
                                    objAsCharacterDataModel.VerticalSpeed.Equals(VerticalSpeed))
                                {
                                    if (objAsCharacterDataModel.EnvironmentalDisplacement.Equals(EnvironmentalDisplacement))
                                    {
                                        if (objAsCharacterDataModel.WalkSpeed.Equals(WalkSpeed) &&
                                            objAsCharacterDataModel.RunSpeed.Equals(RunSpeed) &&
                                            objAsCharacterDataModel.JumpSpeed.Equals(JumpSpeed) &&
                                            objAsCharacterDataModel.FallSpeed.Equals(FallSpeed) &&
                                            objAsCharacterDataModel.BackDashSpeed.Equals(BackDashSpeed) &&
                                            objAsCharacterDataModel.FastFallSpeed.Equals(FastFallSpeed) &&
                                            objAsCharacterDataModel.JumpHorizontalSpeed.Equals(JumpHorizontalSpeed))
                                        {
                                            if (objAsCharacterDataModel.Traction.Equals(Traction))
                                            {
                                                if (objAsCharacterDataModel.BackDashDuration.Equals(BackDashDuration) &&
                                                    objAsCharacterDataModel.BackDashInvincibility.Equals(BackDashInvincibility) &&
                                                    objAsCharacterDataModel.BackDashStartup.Equals(BackDashStartup) &&
                                                    objAsCharacterDataModel.JumpType.Equals(JumpType) &&
                                                    objAsCharacterDataModel.CharacterSprites.Equals(CharacterSprites) &&
                                                    objAsCharacterDataModel.SuperMeterBuildRate.Equals(SuperMeterBuildRate))
                                                {
                                                    if (objAsCharacterDataModel.MaxHitPoints.Equals(MaxHitPoints) &&
                                                        objAsCharacterDataModel.RegenSpeed.Equals(RegenSpeed) &&
                                                        objAsCharacterDataModel.KORegenSpeed.Equals(KORegenSpeed))
                                                    {
                                                        if (objAsCharacterDataModel.NonmoveSoundData.Equals(NonmoveSoundData))
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
            

            return false;
        }
    }
}
