using System;

namespace CharacterDataEditor.Models.CharacterData
{
    public class BaseCharacter
    {
        public string Version { get; set; } = string.Empty;
        public string UID { get; set; } = Guid.NewGuid().ToString();
        public DateTime LastModified { get; set; } = DateTime.Now;
        public string LastModifiedBy { get; set; } = Environment.UserName;
    }
}
