using CharacterDataEditor.Constants;
using CharacterDataEditor.Models;
using CharacterDataEditor.Models.CharacterData;
using Original = CharacterDataEditor.Models.CharacterData.PreviousVersions.Original;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System;

namespace CharacterDataEditor.Services
{
    public interface ICharacterOperations
    {
        public void SaveCharacter(CharacterDataModel character, string projectPath, string path = "");
        public List<T> GetCharactersFromProject<T>(string projectPath) where T : BaseCharacter;
        public T GetCharacterByFilename<T>(string filePath) where T : BaseCharacter;
        public List<T> GetAllGameData<T>(string projectPath) where T : IBaseGameDataModel;
        public T GetGameData<T>(string dataFilePath) where T : IBaseGameDataModel;
    }

    public class CharacterOperations : ICharacterOperations
    {
        private readonly ILogger<ICharacterOperations> _logger;

        public CharacterOperations(ILogger<ICharacterOperations> logger)
        {
            _logger = logger;
        }

        public List<T> GetAllGameData<T>(string projectPath) where T : IBaseGameDataModel
        {
            var allItems = new List<T>();

            if (Directory.Exists(projectPath))
            {
                var itemPath = Path.Combine(projectPath, T.GetAssetFolder());
                var itemFiles = Directory.GetFiles(itemPath, "*.yy", SearchOption.AllDirectories);

                foreach (var itemFile in itemFiles)
                {
                    allItems.Add(GetGameData<T>(itemFile));
                }

                return allItems;
            }

            return null;
        }

        public T GetGameData<T>(string dataFilePath) where T : IBaseGameDataModel
        {
            if (!File.Exists(dataFilePath))
            {
                return default;
            }

            using StreamReader streamReader = new StreamReader(dataFilePath);
            var jsonData = streamReader.ReadToEnd();

            var itemData = JsonConvert.DeserializeObject<T>(jsonData);

            // set the filepath to make it easy to reference this item again
            itemData.FilePath = dataFilePath;

            return itemData;
        }

        public T GetCharacterByFilename<T>(string filePath) where T : BaseCharacter
        {
            if (!File.Exists(filePath))
            {
                _logger.LogError($"File {filePath} does not exist or is not accessable.");
                return null;
            }

            using StreamReader streamReader = new StreamReader(filePath);
            var jsonContents = streamReader.ReadToEnd();

            var convertedCharacter = JsonConvert.DeserializeObject<T>(jsonContents);

            if (convertedCharacter == null)
            {
                _logger.LogError($"Unable to read data in file: {filePath}");
            }

            return convertedCharacter;
        }
        
        public List<T> GetCharactersFromProject<T>(string projectPath) where T : BaseCharacter
        {
            if (!Directory.Exists(projectPath))
            {
                _logger.LogError($"Path {projectPath} does not exist or is not accessable.");
                return null;
            }

            var characterDataPath = Path.Combine(projectPath, ResourceConstants.CharacterDataPathStub);

            if (!Directory.Exists(characterDataPath))
            {
                _logger.LogInformation("Character Data path did not exist... creating...");
                Directory.CreateDirectory(characterDataPath);
                return new List<T>();
            }

            //get all the json files
            var files = Directory.GetFiles(characterDataPath, "*.json");

            var allCharacters = new List<T>();

            foreach (var file in files)
            {
                using StreamReader streamReader = new StreamReader(file);
                var jsonData = streamReader.ReadToEnd();

                var character = JsonConvert.DeserializeObject<T>(jsonData);

                if (character != null)
                {
                    if (character.Version != VersionConstants.CurrentVersion)
                    {
                        character.UpgradeNeeded = true;
                    }

                    character.FileName = file;

                    allCharacters.Add(character);
                }
            }

            return allCharacters;
        }

        public void SaveCharacter(CharacterDataModel character, string projectPath, string savePath = "")
        {
            character.LastModified = DateTime.Now;
            character.LastModifiedBy = Environment.UserName;

            var fileName = $"{character.Name}.json";
            string path;

            if (savePath == string.Empty)
            {
                path = Path.Combine(projectPath, ResourceConstants.CharacterDataPathStub);
                savePath = Path.Combine(path, fileName);
            }
            else
            {
                fileName = Path.GetFileName(savePath);
                path = savePath.Replace(Path.GetFileName(savePath), "");
            }

            if (!Directory.Exists(path))
            {
                _logger.LogInformation("Character Data save path did not exist... creating...");
                Directory.CreateDirectory(path);
            }

            _logger.LogInformation($"Saving {fileName} to {path}");

            using StreamWriter streamWriter = new StreamWriter(savePath, false);
            var json = JsonConvert.SerializeObject(character, Formatting.Indented);
            streamWriter.Write(json);
        }
    }
}
