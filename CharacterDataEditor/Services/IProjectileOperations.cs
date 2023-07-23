using CharacterDataEditor.Constants;
using CharacterDataEditor.Models;
using CharacterDataEditor.Models.ProjectileData;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System;

namespace CharacterDataEditor.Services
{
    public interface IProjectileOperations
    {
        public void SaveProjectile(ProjectileDataModel projectile, string projectPath, string path = "");
        public List<T> GetProjectilesFromProject<T>(string projectPath) where T : BaseProjectile;
        public T GetProjectileByFilename<T>(string filePath) where T : BaseProjectile;
        public List<T> GetAllGameData<T>(string projectPath) where T : IBaseGameDataModel;
        public T GetGameData<T>(string dataFilePath) where T : IBaseGameDataModel;
    }

    public class ProjectileOperations : IProjectileOperations
    {
        private readonly ILogger<IProjectileOperations> _logger;

        public ProjectileOperations(ILogger<IProjectileOperations> logger)
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

        public T GetProjectileByFilename<T>(string filePath) where T : BaseProjectile
        {
            if (!File.Exists(filePath))
            {
                _logger.LogError($"File {filePath} does not exist or is not accessable.");
                return null;
            }

            using StreamReader streamReader = new StreamReader(filePath);
            var jsonContents = streamReader.ReadToEnd();

            var convertedProjectile = JsonConvert.DeserializeObject<T>(jsonContents);

            if (convertedProjectile == null)
            {
                _logger.LogError($"Unable to read data in file: {filePath}");
            }

            return convertedProjectile;
        }

        public List<T> GetProjectilesFromProject<T>(string projectPath) where T : BaseProjectile
        {
            if (!Directory.Exists(projectPath))
            {
                _logger.LogError($"Path {projectPath} does not exist or is not accessable.");
                return null;
            }

            var projectileDataPath = Path.Combine(projectPath, ResourceConstants.ProjectileDataPathStub);

            if (!Directory.Exists(projectileDataPath))
            {
                _logger.LogInformation("Projectile Data path did not exist... creating...");
                Directory.CreateDirectory(projectileDataPath);
                return new List<T>();
            }

            //get all the json files
            var files = Directory.GetFiles(projectileDataPath, "*.json");

            var allProjectiles = new List<T>();

            foreach (var file in files)
            {
                using StreamReader streamReader = new StreamReader(file);
                var jsonData = streamReader.ReadToEnd();

                var projectile = JsonConvert.DeserializeObject<T>(jsonData);

                if (projectile != null)
                {
                    if (projectile.Version != VersionConstants.CurrentVersion)
                    {
                        projectile.UpgradeNeeded = true;
                    }

                    projectile.FileName = file;

                    allProjectiles.Add(projectile);
                }
            }

            return allProjectiles;
        }

        public void SaveProjectile(ProjectileDataModel projectile, string projectPath, string savePath = "")
        {
            projectile.LastModified = DateTime.Now;
            projectile.LastModifiedBy = Environment.UserName;

            var fileName = $"{projectile.Name}.json";
            string path;

            if (savePath == string.Empty)
            {
                path = Path.Combine(projectPath, ResourceConstants.ProjectileDataPathStub);
                savePath = Path.Combine(path, fileName);
            }
            else
            {
                fileName = Path.GetFileName(savePath);
                path = savePath.Replace(Path.GetFileName(savePath), "");
            }

            if (!Directory.Exists(path))
            {
                _logger.LogInformation("Projectile Data save path did not exist... creating...");
                Directory.CreateDirectory(path);
            }

            _logger.LogInformation($"Saving {fileName} to {path}");

            using StreamWriter streamWriter = new StreamWriter(savePath, false);
            var json = JsonConvert.SerializeObject(projectile, Formatting.Indented);
            streamWriter.Write(json);
        }
    }
}
