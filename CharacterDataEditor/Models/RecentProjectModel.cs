using Newtonsoft.Json;
using System;

namespace CharacterDataEditor.Models
{
    public class RecentProjectModel
    {
        public string ProjectFileName { get; set; }
        public string FullPath { get; set; }
        public DateTime LastOpened { get; set; }
        [JsonIgnore]
        public string ProjectPathOnly
        {
            get
            {
                return FullPath.Replace(ProjectFileName, string.Empty);
            }
        }
    }
}