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
        private readonly IEditorOptions _editorOptionsService;
        private readonly ILogger<MainScreen> _logger;
        private float screenWidth, screenHeight;
        private List<RecentProjectModel> _recentProjects;
        private SpriteDrawingHelper _spriteDrawer;
        private EditorOptionsModel _editorOptions;

        private string _currentTheme;

        private bool _themeSelectorOpen;

        public MainScreen(ILogger<MainScreen> logger, IRecentFiles recentFiles, IEditorOptions editorOptionsService)
        {
            _logger = logger;
            _recentFiles = recentFiles;
            _editorOptionsService = editorOptionsService;
        }

        public void CheckForExit(IScreenManager screenManager)
        {
            if (Raylib.WindowShouldClose())
            {
                screenManager.ExitWindow = true;
            }
        }

        public void Init(dynamic screenData)
        {
            screenWidth = screenData?.width ?? 1280.0f;
            screenHeight = screenData?.height ?? 720.0f;
            _recentProjects = _recentFiles.GetRecentProjectFiles();
            _editorOptions = _editorOptionsService.GetEditorOptions();
            _spriteDrawer = new SpriteDrawingHelper();
            _currentTheme = "None";

            _themeSelectorOpen = false;
        }

        public void RenderImGui(IScreenManager screenManager)
        {
            if (_currentTheme != _editorOptions.ThemeName)
            {
                ChangeTheme(screenManager, _editorOptions.ThemeName);
            }

            DrawMainMenu(screenManager.ScreenScale);
            DrawOpenProjectWindow(screenManager.ScreenScale, screenManager);
            DrawLogo(screenManager.ScreenScale);
            DrawThemeSelector(screenManager.ScreenScale, screenManager);
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
            _spriteDrawer.DrawSpriteToScreen(new SpriteDrawDataModel
            {
                SpriteData = null,
                BaseColor = null,
                SwapColor = null,
                DrawPosition = Vector2.Zero,
                Scale = scale,
                DefaultTexture = ResourceConstants.LogoPath,
                Logger = _logger,
                MaxDrawSize = new Vector2(400, 400),
                Flags = SpriteDrawFlags.CenterHorizontal | SpriteDrawFlags.NotAnimated
            });
        }

        private void DrawThemeSelector(float scale, IScreenManager screenManager)
        {
            if (_themeSelectorOpen)
            {
                var windowSize = new Vector2(100.0f, 70.0f);
                windowSize *= scale;

                ImGui.SetNextWindowSize(windowSize, ImGuiCond.FirstUseEver);

                if (ImGui.Begin("Select Theme", ref _themeSelectorOpen, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse))
                {
                    ImGui.SetWindowFontScale(scale);
                    
                    if (ImGui.Button("Dark Theme"))
                    {
                        ChangeTheme(screenManager, "Dark");
                    }

                    ImGui.NewLine();

                    if (ImGui.Button("Light Theme"))
                    {
                        ChangeTheme(screenManager, "Light");
                    }

                    ImGui.NewLine();

                    if (ImGui.Button("Classic Theme"))
                    {
                        ChangeTheme(screenManager, "Classic");
                    }

                    ImGui.End();
                }
            }
        }

        private void ChangeTheme(IScreenManager screenManager, string themeName)
        {
            switch (themeName)
            {
                case "Light":
                    screenManager.BackgroundColor = Color.LIGHTGRAY;
                    ImGui.StyleColorsLight();
                    break;
                case "Classic":
                    screenManager.BackgroundColor = new Color(8, 1, 15, 1);
                    ImGui.StyleColorsClassic();
                    break;
                case "Dark":
                default:
                    screenManager.BackgroundColor = Color.DARKGRAY;
                    ImGui.StyleColorsDark();
                    break;
            }

            _currentTheme = themeName;
            
            if (themeName != _editorOptions.ThemeName)
            {
                _editorOptions.ThemeName = themeName;
                _editorOptions.LastUpdated = DateTime.Now;
                _editorOptionsService.SetEditorOptions(_editorOptions);
            }
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

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Edit"))
            {
                if (ImGui.MenuItem("Theme"))
                {
                    _themeSelectorOpen = true;
                }

                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }
    }
}
