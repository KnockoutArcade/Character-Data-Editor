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
        private SpriteDrawingHelper spriteDrawer;

        public MainScreen(ILogger<MainScreen> logger, IRecentFiles recentFiles)
        {
            _logger = logger;
            _recentFiles = recentFiles;
        }

        public void Init(dynamic screenData)
        {
            screenWidth = screenData?.width ?? 1280.0f;
            screenHeight = screenData?.height ?? 720.0f;
            _recentProjects = _recentFiles.GetRecentProjectFiles();
            spriteDrawer = new SpriteDrawingHelper();
        }

        public void RenderImGui(IScreenManager screenManager)
        {
            DrawMainMenu(screenManager.ScreenScale);
            DrawOpenProjectWindow(screenManager.ScreenScale, screenManager);
            DrawLogo(screenManager.ScreenScale);
        }

        public void RenderAfterImGui(IScreenManager screenManager)
        {
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

                foreach (var item in _recentProjects)
                {
                    if (ImGui.Button(item.ProjectFileName))
                    {
                        //open the item here
                        _recentFiles.AddRecentProjectFile(item.FullPath);
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
            spriteDrawer.DrawSpriteToScreen(
                null,
                null,
                null,
                new Vector2(0, 0),
                scale,
                ResourceConstants.LogoPath,
                _logger,
                new Vector2(400, 400),
                SpriteDrawFlags.CenterHorizontal | SpriteDrawFlags.NotAnimated);
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
