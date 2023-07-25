using System;

namespace CharacterDataEditor.Models.ProjectileData
{
    public class ProjectileFrameDataModel
    {
        public int Length { get; set; } = 0;
        public int ImageIndex { get; set; } = 0;

        public override int GetHashCode()
        {
            return HashCode.Combine(Length, ImageIndex);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(ProjectileFrameDataModel))
            {
                return false;
            }

            var objAsFrameData = obj as ProjectileFrameDataModel;

            if (objAsFrameData.Length == Length)
            {
                if (objAsFrameData.ImageIndex == ImageIndex)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
