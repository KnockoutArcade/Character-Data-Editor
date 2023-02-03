using CharacterDataEditor.Constants;
using CharacterDataEditor.Helpers;
using CharacterDataEditor.Models;
using CharacterDataEditor.Services;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace CharacterDataEditor.Screens
{
    public class ProjectHomeScreen : IScreen
    {
        private readonly ILogger<IScreen> _logger;
        private readonly ICharacterOperations _characterOperations;

        SpriteDrawingHelper spriteDrawer;
        private float height;
        private float width;
        private RecentProjectModel projectData;
        private List<CharacterDataModel> characters;
        private SpriteDataModel spriteData;
        private List<SpriteDataModel> allSprites;

        private bool[] itemSelected;

        public ProjectHomeScreen(ILogger<IScreen> logger, ICharacterOperations characterOperations)
        {
            _logger = logger;
            _characterOperations = characterOperations;
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
            Raylib.SetWindowTitle(TitleConstants.ProjectHomeTitle);

            width = screenData?.width ?? 1280.0f;
            height = screenData?.height ?? 720.0f;
            projectData = screenData.projectData;
            spriteDrawer = new SpriteDrawingHelper();
            spriteData = null;

            characters = _characterOperations.GetCharactersFromProject(projectData.ProjectPathOnly);
            if (characters == null)
            {
                _logger.LogCritical("Unable to load characters, path may be inaccessable. Throwing Exception");
                throw new FileNotFoundException("Unable to load characters, path may be inaccessable.", projectData.FullPath);
            }

            itemSelected = new bool[characters.Count];

            allSprites = _characterOperations.GetAllSprites(projectData.ProjectPathOnly);
        }

        public void RenderImGui(IScreenManager screenManager)
        {
            DrawMainMenu(screenManager.ScreenScale, screenManager);
            DrawNewCharacterPanel(screenManager.ScreenScale, screenManager);
            DrawExistingCharacterPanel(screenManager.ScreenScale, screenManager);
        }

        public void RenderAfterImGui(IScreenManager screenManager)
        {
        }

        private void CreateNewCharacter(IScreenManager screenManager)
        {
            screenManager.NavigateTo(typeof(EditCharacterScreen), new { width, height, projectData, action = "new" });
        }

        private void DrawExistingCharacterPanel(float scale, IScreenManager screenManager)
        {
            ImGui.SetNextWindowPos(new Vector2(20 * scale, 80 * scale));
            var windowSize = new Vector2(400 * scale, 550 * scale);
            ImGui.SetNextWindowSize(windowSize);

            if (ImGui.Begin("Existing Characters", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.SetWindowFontScale(scale);

                if (characters.Count == 0)
                {
                    ImGui.Text("No Characters found...");
                }
                else
                {
                    foreach (var character in characters)
                    {
                        if (ImGui.Selectable(character.Name, itemSelected[characters.IndexOf(character)], ImGuiSelectableFlags.AllowDoubleClick))
                        {
                            var sprite = allSprites.Where(x => x.Name == character.BaseSprite).FirstOrDefault();
                            spriteData = sprite;

                            SetItemAsSelected(characters.IndexOf(character));

                            if (ImGui.IsMouseDoubleClicked(0))
                            {
                                screenManager.NavigateTo(typeof(EditCharacterScreen), new { width, height, character, projectData, action = "edit" });
                            }
                        }
                    }
                }
            }

            var drawPos = new Vector2
            {
                X = 650,
                Y = (height / 2) / scale
            };

            spriteDrawer.DrawSpriteToScreen(
                spriteData,
                null,
                null,
                drawPos, 
                scale, 
                ResourceConstants.LogoPath, 
                _logger, 
                Vector2.Zero, 
                SpriteDrawFlags.CenterVertical);
        }

        private void DrawNewCharacterPanel(float scale, IScreenManager screenManager)
        {
            ImGui.SetNextWindowPos(new Vector2(20 * scale, 20 * scale));
            var windowSize = ImGui.CalcTextSize("Create New Character");
            windowSize.X = (windowSize.X + 18) * scale;
            windowSize.Y = (windowSize.Y + 25) * scale;
            ImGui.SetNextWindowSize(windowSize);

            if (ImGui.Begin("New", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.SetWindowFontScale(scale);

                if (ImGui.Button("Create New Character"))
                {
                    // go to create character screen
                    CreateNewCharacter(screenManager);
                }
            }
        }

        private void DrawMainMenu(float scale, IScreenManager screenManager)
        {
            ImGui.BeginMainMenuBar();

            ImGui.SetWindowFontScale(scale);

            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("New Character"))
                {
                    _logger.LogInformation("New Character Menu Item Clicked");
                    CreateNewCharacter(screenManager);
                }
                ImGui.Separator();

                if (ImGui.MenuItem("Close Project"))
                {
                    _logger.LogInformation("Project closed");
                    screenManager.NavigateTo(typeof(MainScreen), new { height, width });
                }

                ImGui.Separator();
                if (ImGui.MenuItem("Exit"))
                {
                    _logger.LogInformation("Exit Menu Item Clicked");
                    Environment.Exit(0);
                }

                ImGui.End();
            }

            ImGui.EndMainMenuBar();
        }

        private void SetItemAsSelected(int index)
        {
            for (int i = 0; i < itemSelected.Length; i++)
            {
                itemSelected[i] = false;
            }

            itemSelected[index] = true;
        }
    }
}
