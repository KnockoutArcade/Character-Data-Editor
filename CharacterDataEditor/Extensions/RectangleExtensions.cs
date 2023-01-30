using Raylib_cs;
using System.Numerics;

namespace CharacterDataEditor.Extensions
{
    public static class RectangleExtensions
    {
        public static Vector4 ToVector4(this Rectangle rect) =>
            new Vector4(rect.x, rect.y, rect.width, rect.height);
    }
}
