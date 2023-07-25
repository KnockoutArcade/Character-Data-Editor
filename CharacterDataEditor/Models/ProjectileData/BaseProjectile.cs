using CharacterDataEditor.Constants;
using Newtonsoft.Json;
using System;

namespace CharacterDataEditor.Models.ProjectileData
{
    public class BaseProjectile
    {
        public string Version { get; set; } = VersionConstants.CurrentVersion;
        public string UID { get; set; } = Guid.NewGuid().ToString();
        public DateTime LastModified { get; set; } = DateTime.Now;
        public string LastModifiedBy { get; set; } = Environment.UserName;
        [JsonIgnore]
        public bool UpgradeNeeded { get; set; } = false;
        [JsonIgnore]
        public string FileName { get; set; } = string.Empty;
    }
}
