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
        public SpecialMoveType SpecialMoveType { get; set; } = SpecialMoveType.None;
        public SpecialMoveType SpecialMoveCanCancelInto { get; set; } = SpecialMoveType.None;
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
        public List<ProjectileDataModel> ProjectileData { get; set; } = new List<ProjectileDataModel>();
        public RehitDataModel RehitData { get; set; } = new RehitDataModel();
        public SupplimentaryMovementDataModel GroundMovementData { get; set; } = new SupplimentaryMovementDataModel();
        public SupplimentaryMovementDataModel AirMovementData { get; set; } = new SupplimentaryMovementDataModel();
        public string SupplimentaryScript { get; set; } = string.Empty;

        public MoveDataModel GetDuplicate()
        {
            var tempSerializedMoveData = JsonConvert.SerializeObject(this);
            var deserializedMoveData = JsonConvert.DeserializeObject<MoveDataModel>(tempSerializedMoveData);

            deserializedMoveData.UID = Guid.NewGuid().ToString();
            return deserializedMoveData;
        }

        public override int GetHashCode()
        {
            var hash = HashCode.Combine(UID, MoveType, SpecialMoveType, SpriteName, FrameData, AttackData, IsThrow, HurtboxData);
            hash = HashCode.Combine(hash, RehitData, OpponentPositionData, CounterData, CommandNormalData, SpecialData, GroundMovementData, AirMovementData);
            hash = HashCode.Combine(hash, SupplimentaryScript, Duration, ProjectileData, MoveCanCancelInto, SpecialMoveCanCancelInto);

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

            if (objAsMoveData.UID == UID)
            {
                if (objAsMoveData.MoveType == MoveType)
                {
                    if (objAsMoveData.SpecialMoveType == SpecialMoveType)
                    {
                        if (objAsMoveData.SpriteName == SpriteName)
                        {
                            if (objAsMoveData.FrameData.SequenceEqual(FrameData))
                            {
                                if (objAsMoveData.AttackData.SequenceEqual(AttackData))
                                {
                                    if (objAsMoveData.IsThrow == IsThrow)
                                    {
                                        if (objAsMoveData.HurtboxData.SequenceEqual(HurtboxData))
                                        {
                                            if (objAsMoveData.OpponentPositionData.Equals(OpponentPositionData))
                                            {
                                                if (objAsMoveData.RehitData.Equals(RehitData))
                                                {
                                                    if (objAsMoveData.GroundMovementData.Equals(GroundMovementData))
                                                    {
                                                        if (objAsMoveData.AirMovementData.Equals(AirMovementData))
                                                        {
                                                            if (objAsMoveData.CounterData.SequenceEqual(CounterData))
                                                            {
                                                                if (objAsMoveData.Duration.Equals(Duration))
                                                                {
                                                                    if (objAsMoveData.SupplimentaryScript.Equals(SupplimentaryScript))
                                                                    {
                                                                        if (objAsMoveData.ProjectileData.SequenceEqual(ProjectileData))
                                                                        {
                                                                            if (objAsMoveData.MoveCanCancelInto.Equals(MoveCanCancelInto))
                                                                            {
                                                                                if (objAsMoveData.SpecialMoveCanCancelInto.Equals(SpecialMoveCanCancelInto))
                                                                                {
                                                                                    if (objAsMoveData.SpecialData.Equals(SpecialData))
                                                                                    {
                                                                                        if (objAsMoveData.CommandNormalData.Equals(CommandNormalData))
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

            return false;
        }
    }
}