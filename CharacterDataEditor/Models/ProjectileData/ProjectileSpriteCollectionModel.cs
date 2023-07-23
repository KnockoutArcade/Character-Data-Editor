using CharacterDataEditor.Models.CharacterData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CharacterDataEditor.Models.ProjectileData
{
    public class ProjectileSpriteCollectionModel
    {
        public string Sprite { get; set; } = string.Empty;
        public string Impact { get; set; } = string.Empty;

        public override int GetHashCode()
        {
            var hash = HashCode.Combine(Sprite, Impact);

            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(ProjectileSpriteCollectionModel))
            {
                return false;
            }

            var objAsCharacterSpriteCollection = obj as ProjectileSpriteCollectionModel;

            if (objAsCharacterSpriteCollection.Sprite.Equals(Sprite) &&
                objAsCharacterSpriteCollection.Impact.Equals(Impact))
            {
                return true;
            }

            return false;
        }
    }
}
