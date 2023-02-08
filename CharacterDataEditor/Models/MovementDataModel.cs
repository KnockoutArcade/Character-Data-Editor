using System;

namespace CharacterDataEditor.Models
{
    public class MovementDataModel
    {
        public int StartingFrame { get; set; } = 0;
        public float HorizontalSpeed { get; set; } = 0.0f;
        public float VerticalSpeed { get; set; } = 0.0f;
        public bool OverwriteSpeed { get; set; } = false;

        public override int GetHashCode()
        {
            return HashCode.Combine(StartingFrame, HorizontalSpeed, VerticalSpeed, OverwriteSpeed);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(MovementDataModel))
            {
                return false;
            }

            var objAsMovementData = obj as MovementDataModel;

            if (objAsMovementData.StartingFrame == StartingFrame)
            {
                if (objAsMovementData.HorizontalSpeed == HorizontalSpeed)
                {
                    if (objAsMovementData.VerticalSpeed == VerticalSpeed)
                    {
                        if (objAsMovementData.OverwriteSpeed == OverwriteSpeed)
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