using CharacterDataEditor.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CharacterDataEditor.Services
{
    public interface IRecentFiles
    {
        List<RecentProjectModel> GetRecentProjectFiles();
        RecentProjectModel AddRecentProjectFile(string fileFullPath);
    }

    public class RecentFiles : IRecentFiles
    {
        private readonly ILogger<IRecentFiles> _logger;

        public RecentFiles(ILogger<IRecentFiles> logger)
        {
            _logger = logger;
        }

        public RecentProjectModel AddRecentProjectFile(string fileFullPath)
        {
            var recentProjects = GetRecentProjectFiles();

            if (recentProjects.Any(x => x.FullPath == fileFullPath))
            {
                var project = recentProjects.Where(x => x.FullPath == fileFullPath).FirstOrDefault();

                if (project != null)
                {
                    project.LastOpened = DateTime.Now;
                }

                SaveRecentProjects(recentProjects);
                return project;
            }

            //create a RecentProjectModel from the full path
            var recentProject = new RecentProjectModel();

            recentProject.FullPath = fileFullPath;
            recentProject.LastOpened = DateTime.Now;

            var pathSplit = fileFullPath.Split(new string[] { "/", "\\" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            recentProject.ProjectFileName = pathSplit.Last();
            recentProjects.Add(recentProject);

            SaveRecentProjects(recentProjects);
            return recentProject;
        }

        public List<RecentProjectModel> GetRecentProjectFiles()
        {
            //load the file we saved next to the exe with this data...
            if (File.Exists(".EditorRecentFiles"))
            {
                _logger.LogInformation("Loading Recent Files...");

                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".EditorRecentFiles");

                using (StreamReader streamReader = new StreamReader(path))
                {
                    var recentFileData = streamReader.ReadToEnd();
                    //parse into a list of recent projects
                    return JsonConvert.DeserializeObject<List<RecentProjectModel>>(recentFileData) ?? new List<RecentProjectModel>();
                }
            }
            else
            {
                _logger.LogInformation("Recent Files file does not exist... creating it...");
                //create the file
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".EditorRecentFiles");
                using (StreamWriter streamWriter = new StreamWriter(path))
                {
                    streamWriter.WriteLine();
                    return new List<RecentProjectModel>();
                }
            }
        }

        private void SaveRecentProjects(List<RecentProjectModel> recentProjects)
        {
            //save new file
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".EditorRecentFiles");
            using (StreamWriter streamWriter = new StreamWriter(path, false))
            {
                var json = JsonConvert.SerializeObject(recentProjects);
                streamWriter.Write(json);
            }
        }
    }
}
