using System.Numerics;
using Raylib_cs;

namespace CharacterDataEditor.Extensions;

public static class RectangleExtensions
{
    public static Vector4 ToVector4(this Rectangle rect) =>
        new(rect.X, rect.Y, rect.Width, rect.Height);
}
