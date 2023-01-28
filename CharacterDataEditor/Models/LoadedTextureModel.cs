using Raylib_cs;

namespace CharacterDataEditor.Models
{
    public class LoadedTextureModel
    {
        public LoadedTextureModel(string path)
        {
            TexturePath = path;
            Texture = Raylib.LoadTexture(TexturePath);
        }

        public Texture2D Texture { get; set; }
        public string TexturePath { get; set; }
    }
}
