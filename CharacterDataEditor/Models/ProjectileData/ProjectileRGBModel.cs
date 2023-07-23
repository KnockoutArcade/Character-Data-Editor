using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharacterDataEditor.Models.ProjectileData
{
    public record ProjectileRGBModel
    {
        public ProjectileRGBModel() { }
        public ProjectileRGBModel(float r, float g, float b)
        {
            Red = r;
            Green = g;
            Blue = b;
        }

        public float Red { get; set; } = 0;
        public float Blue { get; set; } = 0;
        public float Green { get; set; } = 0;
    }
}
