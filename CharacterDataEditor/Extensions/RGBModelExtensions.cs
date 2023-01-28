using CharacterDataEditor.Models;
using System.Numerics;

namespace CharacterDataEditor.Extensions
{
    public static class RGBModelExtensions
    {
        public static Vector3 ToVector3RGB(this RGBModel input)
        {
            var vec3 = new Vector3(
                input.Red / 255.0f,
                input.Green / 255.0f,
                input.Blue / 255.0f);

            return vec3;
        }
    }
}
