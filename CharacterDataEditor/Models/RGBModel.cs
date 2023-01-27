using System;

namespace CharacterDataEditor.Models
{
    public record RGBModel
    {
        public RGBModel() { }
        public RGBModel(float r, float g, float b)
        {
            //convert to 0-255 by multiplying by 255
            r *= 255.0f;
            g *= 255.0f;
            b *= 255.0f;

            //round away from zero... this gave me the most consistent results
            //so that there wouldn't be a "jump" in the numbers when using
            //the color picker
            var red = Math.Round(r, 0, MidpointRounding.AwayFromZero);
            var green = Math.Round(g, 0, MidpointRounding.AwayFromZero);
            var blue = Math.Round(b, 0, MidpointRounding.AwayFromZero);

            //convert to int
            Red = (int)red;
            Green = (int)green;
            Blue = (int)blue;
        }

        public int Red { get; set; } = 0;
        public int Blue { get; set; } = 0;
        public int Green { get; set; } = 0;
    }
}