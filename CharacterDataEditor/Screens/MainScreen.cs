using CharacterDataEditor.Constants;
using CharacterDataEditor.Helpers;
using CharacterDataEditor.Models;
using CharacterDataEditor.Services;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace CharacterDataEditor.Screens
{
    public class MainScreen : IScreen
    {
        private readonly IRecentFiles _recentFiles;
        private readonly ILogger<MainScreen> _logger;
        private float screenWidth, screenHeight;
        private List<RecentProjectModel> _recentProjects;

        public MainScreen(ILogger<MainScreen> logger, IRecentFiles recentFiles)
        {
            _logger = logger;
            _recentFiles = recentFiles;
        }

        public void Init(dynamic screenData)
        {
            screenWidth = screenData?.width ?? 1280.0f;
            screenHeight = screenData?.height ?? 720.0f;
        }

        public void Render(IScreenManager screenManager)
        {
            DrawMainMenu(screenManager.ScreenScale);
            DrawLogo(screenManager.ScreenScale);
            DrawOpenProjectWindow(screenManager.ScreenScale, screenManager);
        }

        private void DrawOpenProjectWindow(float scale, IScreenManager screenManager)
        {
            ImGui.SetNextWindowPos(new Vector2(screenWidth / 2 - (300 * scale), 300 * scale));
            ImGui.SetNextWindowSize(new Vector2(600 * scale, 200 * scale));

            if (ImGui.Begin("Open Project", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.SetWindowFontScale(scale);

                ImGui.Columns(3);
                ImGui.SetColumnWidth(0, 180 * scale);
                ImGui.SetColumnWidth(1, 220 * scale);

                if (ImGui.Button("Select GameMaker Project"))
                {
                    //open a file browser here
                    var selectedFile = Win32DialogHelper.ShowOpenFileDialog("GameMaker Studio Project File (*.yyp)\0*.yyp\0All Files (*.*)\0*.*\0", "Open GameMaker Project");
                    if (selectedFile != string.Empty)
                    {
                        var item = _recentFiles.AddRecentProjectFile(selectedFile);
                        screenManager.NavigateTo("ProjectHomeScreen", new { width = screenWidth, height = screenHeight, projectData = item });
                    }
                }

                ImGui.NextColumn();

                //recently opened projects
                ImGui.Text("Recently Opened Projects");

                if (_recentProjects == null)
                {
                    _recentProjects = _recentFiles.GetRecentProjectFiles();
                }

                foreach (var item in _recentProjects)
                {
                    if (ImGui.Button(item.ProjectFileName))
                    {
                        //open the item here
                        screenManager.NavigateTo("ProjectHomeScreen", new { width = screenWidth, height = screenHeight, projectData = item });
                    }
                }

                ImGui.NextColumn();
                ImGui.Text("Last Opened");

                foreach (var item in _recentProjects)
                {
                    ImGui.Text($"{item.LastOpened.ToShortDateString()} {item.LastOpened.ToShortTimeString()}");
                }
            }
        }

        private void DrawLogo(float scale)
        {
            var texture = Raylib.LoadTexture(ResourceConstants.LogoPath);
            Rectangle logoRectangle = new Rectangle(0.0f, 0.0f, texture.width, texture.height);

            Rectangle destinationRectangle = new Rectangle();
            destinationRectangle.width = (texture.width * 3) * scale;
            destinationRectangle.height = (texture.height * 3) * scale;
            destinationRectangle.x = (screenWidth / 2 - (destinationRectangle.width / 2));
            destinationRectangle.y = 5 * scale;

            Raylib.DrawTexturePro(texture, logoRectangle, destinationRectangle, new Vector2(0, 0), 0.0f, Color.WHITE);
        }

        private void DrawMainMenu(float scale)
        {
            ImGui.BeginMainMenuBar();

            ImGui.SetWindowFontScale(scale);

            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Exit"))
                {
                    _logger.LogInformation("Exit Menu Item Clicked");
                    Environment.Exit(0);
                }

                ImGui.End();
            }

            ImGui.EndMainMenuBar();
        }
    }
}
