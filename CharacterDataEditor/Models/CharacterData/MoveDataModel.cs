using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CharacterDataEditor.Enums;
using Newtonsoft.Json;

namespace CharacterDataEditor.Models.CharacterData
{
    public class MoveDataModel
    {
        public string UID { get; set; } = Guid.NewGuid().ToString();
        public MoveType MoveType { get; set; } = MoveType.None;
        public MoveType MoveCanCancelInto { get; set; } = MoveType.None;
        public EnhanceMoveType EnhanceMoveType { get; set; } = EnhanceMoveType.None;
        public EnhanceMoveType EnhanceMoveCanCancelInto { get; set; } = EnhanceMoveType.None;
        public List<int> InMovesets { get; set; } = new List<int>();
        public bool SwitchMoveset { get; set; } = false;
        public int SwitchToMoveset { get; set; } = 0;
        public string SpriteName { get; set; } = string.Empty;
        public int Duration { get; set; } = 0;
        public int NumberOfFrames { get { return FrameData?.Count ?? 0; } }
        public List<FrameDataModel> FrameData { get; set; } = new List<FrameDataModel>();
        public int NumberOfHitboxes { get { return AttackData?.Count ?? 0; } }
        public List<AttackDataModel> AttackData { get; set; } = new List<AttackDataModel>();
        public List<CounterHitDataModel> CounterData { get; set; } = new List<CounterHitDataModel>();
        public CommandNormalDataModel CommandNormalData { get; set; } = new CommandNormalDataModel();
        public int NumberOfEnhancements { get { return SpecialData?.Count ?? 0; } }
        public List<SpecialDataModel> SpecialData { get; set; } = new List<SpecialDataModel>();
        public bool IsThrow { get; set; } = false;
        public OpponentPositionDataModel OpponentPositionData { get; set; } = new OpponentPositionDataModel();
        public int NumberOfHurtboxes { get { return HurtboxData?.Count ?? 0; } }
        public List<HurtboxDataModel> HurtboxData { get; set; } = new List<HurtboxDataModel>();
        public int NumberOfProjectiles { get { return ProjectileData?.Count ?? 0; } }
        public List<CharacterProjectileDataModel> ProjectileData { get; set; } = new List<CharacterProjectileDataModel>();
        public RehitDataModel RehitData { get; set; } = new RehitDataModel();
        public SupplementaryMovementDataModel GroundMovementData { get; set; } = new SupplementaryMovementDataModel();
        public SupplementaryMovementDataModel AirMovementData { get; set; } = new SupplementaryMovementDataModel();
        public bool UseMoveScript { get; set; } = false;
        public string SupplementaryMoveScript { get; set; } = string.Empty;
        public int NumberOfSounds { get { return MoveSoundData?.Count ?? 0; } }
        public List<MoveSoundDataModel> MoveSoundData { get; set; } = new List<MoveSoundDataModel>();
        public SpiritDataModel SpiritData { get; set; } = new SpiritDataModel();
        public SuperDataModel SuperData { get; set; } = new SuperDataModel();

        public MoveDataModel GetDuplicate()
        {
            var tempSerializedMoveData = JsonConvert.SerializeObject(this);
            var deserializedMoveData = JsonConvert.DeserializeObject<MoveDataModel>(tempSerializedMoveData);

            deserializedMoveData.UID = Guid.NewGuid().ToString();
            return deserializedMoveData;
        }

        public override int GetHashCode()
        {
            var hash = HashCode.Combine(UID, MoveType, EnhanceMoveType, InMovesets, SwitchMoveset, SwitchToMoveset, SpriteName, FrameData);
            hash = HashCode.Combine(hash, AttackData, IsThrow, HurtboxData, RehitData, OpponentPositionData, CounterData, CommandNormalData);
            hash = HashCode.Combine(hash, SpecialData, GroundMovementData, AirMovementData, UseMoveScript, SupplementaryMoveScript, Duration, ProjectileData);
            hash = HashCode.Combine(hash, MoveCanCancelInto, EnhanceMoveCanCancelInto, NumberOfSounds, MoveSoundData, SpiritData, SuperData);

            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(MoveDataModel))
            {
                return false;
            }

            var objAsMoveData = obj as MoveDataModel;

            if (objAsMoveData.UID == UID &&
                 objAsMoveData.MoveType == MoveType &&
                 objAsMoveData.EnhanceMoveType == EnhanceMoveType &&
                 objAsMoveData.InMovesets.SequenceEqual(InMovesets) &&
                 objAsMoveData.SwitchMoveset.Equals(SwitchMoveset) &&
                 objAsMoveData.SwitchToMoveset.Equals(SwitchToMoveset) &&
                 objAsMoveData.SpriteName == SpriteName &&
                 objAsMoveData.FrameData.SequenceEqual(FrameData) &&
                 objAsMoveData.AttackData.SequenceEqual(AttackData) &&
                 objAsMoveData.IsThrow == IsThrow &&
                 objAsMoveData.HurtboxData.SequenceEqual(HurtboxData) &&
                 objAsMoveData.OpponentPositionData.Equals(OpponentPositionData) &&
                 objAsMoveData.RehitData.Equals(RehitData) &&
                 objAsMoveData.GroundMovementData.Equals(GroundMovementData) &&
                 objAsMoveData.AirMovementData.Equals(AirMovementData) &&
                 objAsMoveData.CounterData.SequenceEqual(CounterData) &&
                 objAsMoveData.Duration.Equals(Duration) &&
                 objAsMoveData.UseMoveScript.Equals(UseMoveScript) &&
                 objAsMoveData.SupplementaryMoveScript.Equals(SupplementaryMoveScript) &&
                 objAsMoveData.ProjectileData.SequenceEqual(ProjectileData) &&
                 objAsMoveData.MoveCanCancelInto.Equals(MoveCanCancelInto) &&
                 objAsMoveData.EnhanceMoveCanCancelInto.Equals(EnhanceMoveCanCancelInto) &&
                 objAsMoveData.SpecialData.SequenceEqual(SpecialData) &&
                 objAsMoveData.CommandNormalData.Equals(CommandNormalData) &&
                 objAsMoveData.NumberOfSounds.Equals(NumberOfSounds) &&
                 objAsMoveData.MoveSoundData.SequenceEqual(MoveSoundData) &&
                 objAsMoveData.SpiritData.Equals(SpiritData) &&
                 objAsMoveData.SuperData.Equals(SuperData))
            {
                return true;
            }

            return false;
        }
    }
}