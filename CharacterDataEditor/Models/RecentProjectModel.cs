using System;

namespace CharacterDataEditor.Models
{
    public class RecentProjectModel
    {
        public string ProjectFileName { get; set; }
        public string FullPath { get; set; }
        public DateTime LastOpened { get; set; }
    }
}