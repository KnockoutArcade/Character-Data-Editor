using System;
using System.Collections.Generic;
using System.Linq;

namespace CharacterDataEditor.Models
{
    public class SupplimentaryMovementDataModel
    {
        public int NumberOfWindows { get { return Windows.Count; } }
        public List<MovementDataModel> Windows { get; set; } = new List<MovementDataModel>();
        public float GravityScale { get; set; } = 0.0f;
        public float FallScale { get; set; } = 0.0f;

        public override int GetHashCode()
        {
            return HashCode.Combine(Windows, GravityScale, FallScale);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(SupplimentaryMovementDataModel))
            {
                return false;
            }

            var objAsSupplimentaryData = obj as SupplimentaryMovementDataModel;

            if (GravityScale == objAsSupplimentaryData.GravityScale)
            {
                if (FallScale == objAsSupplimentaryData.FallScale)
                {
                    if (Windows.SequenceEqual(objAsSupplimentaryData.Windows))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}