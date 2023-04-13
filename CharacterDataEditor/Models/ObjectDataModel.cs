using Newtonsoft.Json;

namespace CharacterDataEditor.Models
{
    public class ObjectDataModel : IBaseGameDataModel
    {
        [JsonProperty("resourceVersion")]
        public string ResourceVersion { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("resourceType")]
        public string ResourceType { get; set; }
        [JsonProperty("parent")]
        public ObjectDataParentModel ContainerInfo { get; set; }

        [JsonIgnore]
        public string FilePath { get; set; }


        public static string GetAssetFolder()
        {
            return "objects";
        }
    }

    public class ObjectDataParentModel
    {
        [JsonProperty("name")]
        public string ContainingFolder { get; set; }
    }
}