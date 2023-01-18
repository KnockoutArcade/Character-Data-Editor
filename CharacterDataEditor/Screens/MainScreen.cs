using CharacterDataEditor.Services;
using ImGuiNET;
using System;
using System.Collections.Generic;

namespace CharacterDataEditor.Screens
{
    public class MainScreen : IScreen
    {
        public MainScreen()
        {
        }

        public void Init(dynamic screenData)
        {
        }

        public void Render(IScreenManager screenManager)
        {
            ImGui.BeginMainMenuBar();

            ImGui.SetWindowFontScale(screenManager.ScreenScale);

            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Exit"))
                {
                    Environment.Exit(0);
                }

                ImGui.End();
            }

            ImGui.EndMainMenuBar();

            if (ImGui.Begin("Open Project", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.SetWindowFontScale(screenManager.ScreenScale);

                if (ImGui.Button("Select GameMaker Project"))
                {
                    //open a file browser here
                }

                //recently opened projects
                ImGui.SameLine(0.0f, 100.0f);
                ImGui.Text("Recently Opened Projects");
            }
        }
    }
}
