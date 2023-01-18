using CharacterDataEditor.Services;
using ImGuiNET;
using System.Collections.Generic;

namespace CharacterDataEditor.Screens
{
    public class EditCharacterScreen : IScreen
    {
        private string characterName;
        private List<string> filePaths;

        public EditCharacterScreen()
        {
        }

        public void Init(dynamic screenData)
        {
            characterName = screenData.characterName;
            filePaths = screenData.filePaths;
        }

        public void Render(IScreenManager screenManager)
        {
            ImGui.BeginMainMenuBar();

            if (ImGui.MenuItem("File"))
            {
            }

            if (ImGui.MenuItem("Edit"))
            {
            }

            ImGui.EndMainMenuBar();
        }
    }
}
