using Newtonsoft.Json;
using System.Collections.Generic;

namespace CharacterDataEditor.Models
{
    public class SpriteDataModel : IBaseGameDataModel
    {
        [JsonProperty(PropertyName = "width")]
        public int Width { get; set; }
        [JsonProperty(PropertyName = "height")]
        public int Height { get; set; }
        [JsonProperty(PropertyName = "frames")]
        public List<Frame> Frames { get; set; }
        [JsonProperty(PropertyName = "sequence")]
        public Sequence Sequence { get; set; }
        [JsonProperty(PropertyName = "resourceVersion")]
        public string ResourceVersion { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonIgnore]
        public string FilePath { get; set; }

        public static string GetAssetFolder()
        {
            return "sprites";
        }
    }

    public class Sequence
    {
        public int timeUnits { get; set; }
        public int playback { get; set; }
        public float playbackSpeed { get; set; }
        public int playbackSpeedType { get; set; }
        public bool autoRecord { get; set; }
        public float volume { get; set; }
        public float length { get; set; }
        public List<Track> tracks { get; set; }
        public int xorigin { get; set; }
        public int yorigin { get; set; }
        public string resourceVersion { get; set; }
        public string name { get; set; }
    }

    public class Spriteid
    {
        public string name { get; set; }
        public string path { get; set; }
    }

    public class Events
    {
        public object[] Keyframes { get; set; }
        public string resourceVersion { get; set; }
        public string resourceType { get; set; }
        public string elementType { get; set; }
    }

    public class Moments
    {
        public object[] Keyframes { get; set; }
        public string resourceVersion { get; set; }
        public string resourceType { get; set; }
        public string elementType { get; set; }
    }

    public class Track
    {
        public string name { get; set; }
        public Keyframes keyframes { get; set; }
        public string resourceVersion { get; set; }
    }

    public class Keyframes
    {
        [JsonProperty(PropertyName = "Keyframes")]
        public List<Keyframe> Frames { get; set; }
        public string resourceVersion { get; set; }
        public string resourceType { get; set; }
        public string elementType { get; set; }
    }

    public class Keyframe
    {
        public string id { get; set; }
        public float Key { get; set; }
        public float Length { get; set; }
        public bool Stretch { get; set; }
        public bool Disabled { get; set; }
        public Channels Channels { get; set; }
        public string resourceVersion { get; set; }
    }

    public class Frame
    {
        public string resourceVersion { get; set; }
        public string name { get; set; }
    }

    public class Channels
    {
        [JsonProperty(PropertyName = "0")]
        public Channel _0 { get; set; }
    }

    public class Channel
    {
        public ChannelId Id { get; set; }
    }

    public class ChannelId
    {
        public string name { get; set; }
    }
}