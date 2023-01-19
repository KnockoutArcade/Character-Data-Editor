using CharacterDataEditor.Constants;
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

        private string spriteToDraw;
        private string prevSpriteToDraw;
        private int currentFrame;
        private int frameCounter;
        private float height;
        private float width;
        private RecentProjectModel projectData;
        private List<CharacterDataModel> characters;
        private SpriteDataModel spriteData;

        public ProjectHomeScreen(ILogger<IScreen> logger, ICharacterOperations characterOperations)
        {
            _logger = logger;
            _characterOperations = characterOperations;
        }

        public void Init(dynamic screenData)
        {
            width = screenData?.width ?? 1280.0f;
            height = screenData?.height ?? 720.0f;
            projectData = screenData.projectData;
            spriteToDraw = string.Empty;
            frameCounter = 0;

            var projectPath = projectData.FullPath.Replace(projectData.ProjectFileName, string.Empty);

            characters = _characterOperations.GetCharactersFromProject(projectPath);
            if (characters == null)
            {
                _logger.LogCritical("Unable to load characters, path may be inaccessable. Throwing Exception");
                throw new FileNotFoundException("Unable to load characters, path may be inaccessable.", projectData.FullPath);
            }
        }

        public void Render(IScreenManager screenManager)
        {
            DrawMainMenu(screenManager.ScreenScale, screenManager);
            DrawNewCharacterPanel(screenManager.ScreenScale);
            DrawExistingCharacterPanel(screenManager.ScreenScale, screenManager);
        }

        private void CreateNewCharacter()
        {
        }

        private void DrawSelectedCharacterSprite(string spritePath, float scale)
        {
            Texture2D texture;

            if (string.IsNullOrWhiteSpace(spritePath))
            {
                texture = Raylib.LoadTexture(ResourceConstants.LogoPath);
            }
            else
            {
                var projectPath = projectData.FullPath.Replace(projectData.ProjectFileName, string.Empty);
                var fullSpritePath = Path.Combine(projectPath, spritePath);

                if (prevSpriteToDraw != spriteToDraw)
                {
                    prevSpriteToDraw = spriteToDraw;
                    spriteData = _characterOperations.GetSpriteData(fullSpritePath);
                    currentFrame = 0;
                }

                var spriteDataPathFragments = fullSpritePath.Split(new string[] { "/", "\\" }, StringSplitOptions.RemoveEmptyEntries);
                var spriteImagePath = fullSpritePath.Replace(spriteDataPathFragments.Last(), string.Empty);

                var currentFrameName = spriteData.Sequence.tracks[0].keyframes.Frames
                    .Where(x => (int)x.Key == currentFrame)
                    .Select(x => x.Channels._0.Id.name)
                    .FirstOrDefault();
                
                if (currentFrameName == null)
                {
                    _logger.LogError($"Frame data not found for current frame {currentFrame}");
                }

                Frame currentFrameData = spriteData.Frames.Where(x => x.name == currentFrameName).FirstOrDefault();

                if (currentFrameData == null)
                {
                    _logger.LogError($"Frame data not found for current frame {currentFrame}");
                }

                // right now we're only supporting PNG
                spriteImagePath = Path.Combine(spriteImagePath, currentFrameData.name + ".png");
                frameCounter++;
                
                if (frameCounter >= 2100000000)
                {
                    // prevent overflow
                    frameCounter = 0;
                }

                //determine number of frames to allow each image to be displayed
                //speed is stored as frames per second, the UI runs at 60fps
                //60 / playbackspeed = number of frames to display each image
                var playbackSpeed = 60.0f / spriteData.Sequence.playbackSpeed;

                if (frameCounter % playbackSpeed == 0.0f)
                {
                    currentFrame++;

                    if (currentFrame >= spriteData.Frames.Count())
                    {
                        currentFrame = 0;
                    }
                }

                texture = Raylib.LoadTexture(spriteImagePath);
            }
            
            Rectangle textureSourceRectangle = new Rectangle(0.0f, 0.0f, texture.width, texture.height);

            //destination rectangle determines the size to scale it to and the position on screen
            Rectangle destinationRectangle = new Rectangle();
            destinationRectangle.width = (texture.width * 3) * scale;
            destinationRectangle.height = (texture.height * 3) * scale;
            destinationRectangle.x = 650 * scale;
            destinationRectangle.y = (height / 2 - (destinationRectangle.height / 2));

            // Origin determines where everything is based, passing 0x0y keeps it default
            // Color.White is used to not tint the texture at all
            Raylib.DrawTexturePro(texture, textureSourceRectangle, destinationRectangle, new Vector2(0, 0), 0.0f, Color.WHITE);
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
                        if (ImGui.Selectable(character.Name))
                        {
                            spriteToDraw = character.BaseSprite;
                        }

                        ImGui.SameLine();

                        if (ImGui.Button("Open"))
                        {
                            // go to edit character screen
                            screenManager.NavigateTo(typeof(EditCharacterScreen), new { width = width, height = height, character = character, projectData = projectData });
                        }
                    }
                }
            }

            DrawSelectedCharacterSprite(spriteToDraw, scale);
        }

        private void DrawNewCharacterPanel(float scale)
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
                    CreateNewCharacter();
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
                    CreateNewCharacter();
                }
                ImGui.Separator();

                if (ImGui.MenuItem("Close Project"))
                {
                    _logger.LogInformation("Project closed");
                    screenManager.NavigateTo(typeof(MainScreen), new { height = height, width = width });
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
    }
}
