using ImGuiNET;
using System;

namespace CharacterDataEditor.Screens
{
    public class MainScreen : IScreen
    {
        public MainScreen()
        {
            Init();
        }

        public void Init()
        {
        }

        public void Render()
        {
            ImGui.BeginMainMenuBar();

            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Exit"))
                {
                    Environment.Exit(0);
                }

                ImGui.End();
            }

            ImGui.EndMainMenuBar();
        }
    }
}
