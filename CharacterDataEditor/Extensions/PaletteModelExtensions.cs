using CharacterDataEditor.Models;
using System.Numerics;

namespace CharacterDataEditor.Extensions
{
    public static class PaletteModelExtensions
    {
        public static Vector4[] ToShaderVec4Array(this PaletteModel paletteModel)
        {
            Vector4[] retval = new Vector4[paletteModel.NumberOfReplacableColors];

            //grab each rgb and convert to vector4. A channel is always 1
            for (int i = 0; i < paletteModel.NumberOfReplacableColors; i++)
            {
                var vec4Palette = new Vector4(paletteModel.ColorPalette[i].ToVector3RGB(), 1.0f);

                retval[i] = vec4Palette;
            }

            return retval;
        }
    }
}
