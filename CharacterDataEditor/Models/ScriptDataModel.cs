using Newtonsoft.Json;

namespace CharacterDataEditor.Models
{
    public class ScriptDataModel : IBaseGameDataModel
    {
        [JsonProperty("resourceVersion")]
        public string ResourceVersion { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("resourceType")]
        public string ResourceType { get; set; }

        [JsonIgnore]
        public string FilePath { get; set; }


        public static string GetAssetFolder()
        {
            return "scripts";
        }
    }
}