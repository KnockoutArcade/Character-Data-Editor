using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CharacterDataEditor.Models.ProjectileData
{
    public class ProjectilePaletteModel
    {
        public string UID { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public int NumberOfReplacableColors { get { return ColorPalette?.Count ?? 0; } }
        public List<ProjectileRGBModel> ColorPalette { get; set; } = new List<ProjectileRGBModel>();

        public ProjectilePaletteModel GetDuplicate()
        {
            var tempSerializedPaletteData = JsonConvert.SerializeObject(this);
            var deserializedPaletteData = JsonConvert.DeserializeObject<ProjectilePaletteModel>(tempSerializedPaletteData);

            deserializedPaletteData.UID = Guid.NewGuid().ToString();
            return deserializedPaletteData;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, UID, ColorPalette);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(ProjectilePaletteModel))
            {
                return false;
            }

            var objAsPaletteModel = (ProjectilePaletteModel)obj;

            if (objAsPaletteModel.UID.Equals(UID))
            {
                if (objAsPaletteModel.Name.Equals(Name))
                {
                    if (objAsPaletteModel.ColorPalette.SequenceEqual(ColorPalette))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
