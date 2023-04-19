using System;

namespace CharacterDataEditor.Models.CharacterData
{
    public record RGBModel
    {
        public RGBModel() { }
        public RGBModel(float r, float g, float b)
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