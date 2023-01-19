using CharacterDataEditor.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace CharacterDataEditor.Services
{
    public interface ICharacterOperations
    {
        public List<CharacterDataModel> GetCharactersFromProject(string projectPath);
        public SpriteDataModel GetSpriteData(string spriteFilePath);
    }

    public class CharacterOperations : ICharacterOperations
    {
        private readonly ILogger<ICharacterOperations> _logger;

        public CharacterOperations(ILogger<ICharacterOperations> logger)
        {
            _logger = logger;
        }

        public List<CharacterDataModel> GetCharactersFromProject(string projectPath)
        {
            if (!Directory.Exists(projectPath))
            {
                _logger.LogError($"Path {projectPath} does not exist or is not accessable.");
                return null;
            }

            var characterDataPath = Path.Combine(projectPath, "characterdata");

            if (!Directory.Exists(characterDataPath))
            {
                _logger.LogInformation("Character Data path did not exist... creating...");
                Directory.CreateDirectory(characterDataPath);
                return new List<CharacterDataModel>();
            }

            //get all the json files
            var files = Directory.GetFiles(characterDataPath, "*.json");

            var allCharacters = new List<CharacterDataModel>();

            foreach (var file in files)
            {
                using StreamReader streamReader = new StreamReader(file);
                var jsonData = streamReader.ReadToEnd();

                var character = JsonConvert.DeserializeObject<CharacterDataModel>(jsonData);

                if (character != null)
                {
                    allCharacters.Add(character);
                }
            }

            return allCharacters;
        }

        public SpriteDataModel GetSpriteData(string spriteFilePath)
        {
            if (!File.Exists(spriteFilePath))
            {
                return null;
            }
            
            using StreamReader streamReader = new StreamReader(spriteFilePath);
            var jsonData = streamReader.ReadToEnd();

            var spriteData = JsonConvert.DeserializeObject<SpriteDataModel>(jsonData);

            // modify the sprite data so everything is in order by default... just in case it isn't already


            return spriteData;
        }
    }
}
