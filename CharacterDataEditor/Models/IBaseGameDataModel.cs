namespace CharacterDataEditor.Models
{
    public interface IBaseGameDataModel
    {
        public static abstract string GetAssetFolder();
        public string FilePath { get; set; }
    }
}
