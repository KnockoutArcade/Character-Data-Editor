using System.Numerics;

namespace CharacterDataEditor.Models
{
    public class AnimatedSpriteReturnDataModel
    {
        public int CurrentFrame { get; set; }
        public Vector2 DrawOrigin { get; set; }
        public Vector2 ScaledDrawSize { get; set; }
    }
}
