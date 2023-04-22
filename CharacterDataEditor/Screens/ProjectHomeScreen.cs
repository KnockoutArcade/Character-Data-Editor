using CharacterDataEditor.Constants;
using CharacterDataEditor.Extensions;
using CharacterDataEditor.Helpers;
using CharacterDataEditor.Models;
using CharacterDataEditor.Models.CharacterData;
using OriginalVersion = CharacterDataEditor.Models.CharacterData.PreviousVersions.Original;
using Ver095 = CharacterDataEditor.Models.CharacterData.PreviousVersions.Ver095;
using Ver103 = CharacterDataEditor.Models.CharacterData.PreviousVersions.Ver103;
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

        private bool upgradeMessageShown;
        private string upgradeMessageText;

        private delegate void AfterConfirmAction(int keyCode, IScreenManager screenManager);
        private AfterConfirmAction afterConfirmAction;

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

            // initially attempt to get characters with the current version of the project data structure
            characters = _characterOperations.GetCharactersFromProject<CharacterDataModel>(projectData.ProjectPathOnly);
            if (characters == null)
            {
                _logger.LogCritical("Unable to load characters, path may be inaccessable. Throwing Exception");
                throw new FileNotFoundException("Unable to load characters, path may be inaccessable.", projectData.FullPath);
            }

            // if any of the characters need an upgrade, generate an error message and display with options for the user
            var charactersNeedingUpgrade = characters.Where(x => x.UpgradeNeeded).ToList();

            // remove the characters that need an upgrade from the open list until they're upgraded
            charactersNeedingUpgrade.ForEach(x => characters.Remove(x));
            
            if (charactersNeedingUpgrade.Any())
            {
                string fullErrorMessage = string.Empty;
                List<CharacterDataModel> upgradedCharacters = new List<CharacterDataModel>();

                foreach (var characterNeedingUpgrade in charactersNeedingUpgrade)
                {
                    UpgradeResults results;

                    switch (characterNeedingUpgrade.Version)
                    {
                        case VersionConstants.Ver094:
                        case VersionConstants.Ver095:
                            {
                                var oldCharacter = _characterOperations.GetCharacterByFilename<Ver095.CharacterDataModel>(characterNeedingUpgrade.FileName);
                                results = oldCharacter.Upgrade();
                                break;
                            }
                        case VersionConstants.Ver096:
                        case VersionConstants.Ver1:
                        case VersionConstants.Ver101:
                        case VersionConstants.Ver102:
                        case VersionConstants.Ver103:
                            {
                                var oldCharacter = _characterOperations.GetCharacterByFilename<Ver103.CharacterDataModel>(characterNeedingUpgrade.FileName);
                                results = oldCharacter.Upgrade();
                                break;
                            }
                        case VersionConstants.Original:
                            {
                                var oldCharacter = _characterOperations.GetCharacterByFilename<OriginalVersion.CharacterDataModel>(characterNeedingUpgrade.FileName);
                                results = oldCharacter.Upgrade();
                                break;
                            }
                        case VersionConstants.Ver110:
                        default:
                            {
                                results = characterNeedingUpgrade.Upgrade();
                                break;
                            }
                    }

                    if (results.IsDataLossSuspected)
                    {
                        fullErrorMessage += $"\nCharacter {characterNeedingUpgrade.Name}'s data needs to be upgraded. Upgrade specific messages follow:\n{results.Message}\n";
                    }

                    // add the upgraded character to the list
                    upgradedCharacters.Add(results.UpgradedCharacterData);
                }

                if (fullErrorMessage != string.Empty)
                {
                    upgradeMessageText = fullErrorMessage;
                    upgradeMessageShown = true;

                    afterConfirmAction = (keycode, screenManager) =>
                    {
                        if (keycode == (int)KeyboardKey.KEY_C)
                        {
                            screenManager.NavigateTo(typeof(MainScreen), new { height, width });
                        }
                        else if (keycode == (int)KeyboardKey.KEY_U)
                        {
                            // add characters to the list
                            characters.AddRange(upgradedCharacters);

                            // save the updated characters
                            upgradedCharacters.ForEach(x => _characterOperations.SaveCharacter(x, projectData.ProjectPathOnly));

                            // resize the flags for selection
                            itemSelected = new bool[characters.Count];

                            // disable the message
                            upgradeMessageShown = false;
                        }
                    };
                }
                else if(upgradedCharacters.Count > 0)
                {
                    // add characters to the list
                    characters.AddRange(upgradedCharacters);

                    // save the updated characters
                    upgradedCharacters.ForEach(x => _characterOperations.SaveCharacter(x, projectData.ProjectPathOnly));

                    // resize the flags for selection
                    itemSelected = new bool[characters.Count];
                }
            }

            itemSelected = new bool[characters.Count];

            allSprites = _characterOperations.GetAllGameData<SpriteDataModel>(projectData.ProjectPathOnly);
        }

        public void RenderImGui(IScreenManager screenManager)
        {
            DrawMainMenu(screenManager.ScreenScale, screenManager);
            DrawNewCharacterPanel(screenManager.ScreenScale, screenManager);
            DrawExistingCharacterPanel(screenManager.ScreenScale, screenManager);
        }

        public void RenderAfterImGui(IScreenManager screenManager)
        {
            if (upgradeMessageShown)
            {
                var pressedKey = Raylib.GetKeyPressed();
                if(pressedKey == (int)KeyboardKey.KEY_U || pressedKey == (int)KeyboardKey.KEY_C)
                {
                    if (afterConfirmAction != null)
                    {
                        afterConfirmAction(pressedKey, screenManager);
                    }
                }

                var fontSize = (int)(18.0f * screenManager.ScreenScale);
                var fontSpacing = 5.5f;

                var completeMessage = $"{MessageConstants.UpgradeNeededMessage}Messages from upgrade process follow:\n{upgradeMessageText}";

                var defaultFont = Raylib.GetFontDefault();
                //var messageWidth = Raylib.MeasureText(completeMessage, fontSize);
                var messageSize = Raylib.MeasureTextEx(defaultFont, completeMessage, fontSize, fontSpacing);

                var messageXCoord = (int)((width / 2.0f) - (messageSize.X / 2.0f));
                var messageYCoord = (int)((height / 2.0f) - (messageSize.Y / 2.0f));

                var messageRect = new Rectangle();
                messageRect.x = 0.0f;
                messageRect.height = messageSize.Y + (20.0f * screenManager.ScreenScale);
                messageRect.width = width;
                messageRect.y = (height / 2.0f) - messageRect.height / 2.0f;

                Raylib.DrawRectanglePro(messageRect, Vector2.Zero, 0.0f, Color.BLACK);

                Raylib.DrawTextEx(defaultFont, completeMessage,
                    new Vector2(messageXCoord, messageYCoord), fontSize, fontSpacing, Color.WHITE);
            }
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
                            var sprite = allSprites.Where(x => x.Name == character.CharacterSprites?.Idle).FirstOrDefault();
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

            spriteDrawer.DrawSpriteToScreen(new SpriteDrawDataModel
            {
                SpriteData = spriteData,
                BaseColor = null,
                SwapColor = null,
                DrawPosition = drawPos,
                Scale = scale,
                DefaultTexture = ResourceConstants.LogoPath,
                Logger = _logger,
                MaxDrawSize = Vector2.Zero,
                Flags = SpriteDrawFlags.CenterVertical
            });
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
