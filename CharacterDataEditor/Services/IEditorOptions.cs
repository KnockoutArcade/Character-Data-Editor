using CharacterDataEditor.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CharacterDataEditor.Services
{
    public interface IEditorOptions
    {
        EditorOptionsModel GetEditorOptions();
        void SetEditorOptions(EditorOptionsModel editorOptions);
    }

    public class EditorOptions : IEditorOptions
    {
        private readonly ILogger<IEditorOptions> _logger;

        public EditorOptions(ILogger<IEditorOptions> logger)
        {
            _logger = logger;
        }

        public EditorOptionsModel GetEditorOptions()
        {
            if (File.Exists(".EditorOptions"))
            {
                _logger.LogInformation("Loading Editor Options...");

                var path = Path.Combine(AppContext.BaseDirectory, ".EditorOptions");

                using (StreamReader streamReader = new StreamReader(path))
                {
                    var editorOptionsData = streamReader.ReadToEnd();

                    return JsonConvert.DeserializeObject<EditorOptionsModel>(editorOptionsData);
                }
            }
            else
            {
                _logger.LogInformation("Editor Options file does not exist... creating it...");
                var options = new EditorOptionsModel
                {
                    LastUpdated = DateTime.Now,
                    ThemeName = "Dark"
                };

                SetEditorOptions(options);
                return options;
            }
        }

        public void SetEditorOptions(EditorOptionsModel editorOptions)
        {
            _logger.LogInformation("Saving Editor Options...");
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".EditorOptions");
            using (StreamWriter streamWriter = new StreamWriter(path, false))
            {
                var json = JsonConvert.SerializeObject(editorOptions);
                streamWriter.Write(json);
            }
        }
    }
}
