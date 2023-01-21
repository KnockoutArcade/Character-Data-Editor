using CharacterDataEditor.Constants;
using CharacterDataEditor.Enums;
using CharacterDataEditor.Extensions;
using CharacterDataEditor.Helpers;
using CharacterDataEditor.Models;
using CharacterDataEditor.Services;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace CharacterDataEditor.Screens
{
    public class EditCharacterScreen : IScreen
    {
        private readonly ILogger<IScreen> _logger;
        private readonly ICharacterOperations _characterOperations;

        private float width;
        private float height;
        private CharacterDataModel character;
        private RecentProjectModel projectData;
        private string action;

        private Texture2D playButtonTexture;
        private Texture2D pauseButtonTexture;
        private Texture2D advanceOneFrameForwardTexture;
        private Texture2D advanceOneFrameBackTexture;

        private int selectedMoveType = 0;
        private MoveDataModel moveInEditor;
        private PaletteModel paletteInEditor;
        List<string> moveTypesList = new List<string>();

        List<SpriteDataModel> allSprites;

        private string spriteToDraw;
        private string prevSpriteToDraw;
        private SpriteDataModel spriteData;
        private SpriteDrawingHelper spriteDrawer;
        private Vector2 spriteDrawPosition;

        private bool animationPaused;
        private FrameAdvance frameAdvance;

        private EditorMode editorMode;

        private int currentFrame;

        private const int buttonSpacing = 8;

        public EditCharacterScreen(ILogger<IScreen> logger, ICharacterOperations characterOperations)
        {
            _logger = logger;
            _characterOperations = characterOperations;
        }

        public void Init(dynamic screenData)
        {
            spriteToDraw = string.Empty;
            prevSpriteToDraw = string.Empty;
            spriteData = null;
            editorMode = EditorMode.None;

            moveInEditor = null;
            paletteInEditor = null;

            width = screenData?.width ?? 1280;
            height = screenData?.height ?? 720;
            projectData = screenData?.projectData ?? new RecentProjectModel();
            action = screenData?.action ?? "new";
            character = action == "edit" ? screenData.character : new CharacterDataModel();

            allSprites = _characterOperations.GetAllSprites(projectData.ProjectPathOnly);

            if (action == "edit" && !string.IsNullOrWhiteSpace(character.BaseSprite))
            {
                spriteData = allSprites.FirstOrDefault(x => x.Name == character.BaseSprite);
            }

            playButtonTexture = Raylib.LoadTexture(ResourceConstants.PlayButtonPath);
            pauseButtonTexture = Raylib.LoadTexture(ResourceConstants.PauseButtonPath);
            advanceOneFrameForwardTexture = Raylib.LoadTexture(ResourceConstants.AdvanceOneFrameButtonPath);
            advanceOneFrameBackTexture = Raylib.LoadTexture(ResourceConstants.AdvanceOneFrameBackButtonPath);

            spriteDrawer = new SpriteDrawingHelper();

            var moveTypes = Enum.GetValues(typeof(MoveType));
            moveTypesList = new List<string>();

            foreach (MoveType item in moveTypes)
            {
                var itemAsString = item.ToString().AddSpacesToCamelCase();

                moveTypesList.Add(itemAsString);
            }

            moveTypesList.Sort();

            //init spritedrawposition
            spriteDrawPosition = new Vector2(0, 40);

            currentFrame = 0;
            frameAdvance = FrameAdvance.None;
        }

        public void Render(IScreenManager screenManager)
        {
            RenderMainMenu(screenManager.ScreenScale, screenManager);
            RenderCharacterDataWindow(screenManager.ScreenScale);
            RenderSpriteDisplayArea(screenManager.ScreenScale, screenManager);
            RenderEditor(screenManager.ScreenScale);
            RenderPaletteWindow(screenManager.ScreenScale);

            var maxSpriteSize = new Vector2(200, 200);

            var animationFlags = SpriteDrawFlags.CenterHorizontal | SpriteDrawFlags.ShowSpriteOutline;

            if (animationPaused)
            {
                animationFlags |= SpriteDrawFlags.Pause;
            }
            else
            {
                animationFlags &= ~SpriteDrawFlags.Pause;
            }

            if (frameAdvance != FrameAdvance.None)
            {
                currentFrame = spriteDrawer.DrawSpecificFrameSpriteToScreen(spriteData, spriteDrawPosition, screenManager.ScreenScale, _logger, maxSpriteSize, frameAdvance, animationFlags);
                frameAdvance = FrameAdvance.None;
            }
            else
            {
                currentFrame = spriteDrawer.DrawSpriteToScreen(spriteData, spriteDrawPosition, screenManager.ScreenScale, ResourceConstants.LogoPath, _logger, maxSpriteSize, animationFlags);
            }
        }

        private void RenderPaletteWindow(float scale)
        {
            var windowSize = new Vector2();
            windowSize.X = 320 * scale;
            windowSize.Y = 620 * scale;

            ImGui.SetNextWindowPos(new Vector2(10 * scale, 20 * scale));
            ImGui.SetNextWindowSize(windowSize);

            if (ImGui.Begin("Palette Editor", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.SetWindowFontScale(scale);

                if (ImGui.CollapsingHeader("Base Palette", ImGuiTreeNodeFlags.DefaultOpen))
                {
                }

                if (ImGui.CollapsingHeader("All Palettes", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    if (ImGui.Button("Add New Palette"))
                    {
                    }
                    ImGui.Separator();
                    if (character.Palettes != null && character.Palettes.Count > 0)
                    {
                        foreach (var palette in character.Palettes)
                        {

                        }
                    }
                    else
                    {
                        ImGui.Text("No Palettes...");
                    }
                }

                ImGui.End();
            }
        }

        private void RenderMoveEditor(float scale)
        {
            if (moveInEditor == null)
            {
                ImGui.Text("No move selected...");
            }
            else
            {
                ImGui.Text("Move Type");
                ImGui.SameLine();
                ImGui.Combo("##MoveType", ref selectedMoveType, moveTypesList.ToArray(), moveTypesList.Count);
                //file open for sprite... or should we enumerate them and display a list...?
                ImGui.Text("Sprite ID");
                ImGui.SameLine();

                var spriteId = moveInEditor.SpriteName ?? string.Empty;
                //var allSprites = _characterOperations.GetAllSprites(projectData.ProjectPathOnly);

                var selectedSpriteIndex = spriteId != string.Empty ? allSprites.IndexOf(allSprites.First(x => x.Name == moveInEditor.SpriteName)) : -1;
                ImGui.Combo("##SpriteSelection", ref selectedSpriteIndex, allSprites.Select(x => x.Name).ToArray(), allSprites.Count);
                if (selectedSpriteIndex > -1)
                {
                    if (moveInEditor.SpriteName != allSprites[selectedSpriteIndex].Name)
                    {
                        moveInEditor.SpriteName = allSprites[selectedSpriteIndex].Name;
                        spriteData = allSprites[selectedSpriteIndex];
                    }
                }
                //ImGui.InputText("##spriteID", ref spriteId, 200);
                //moveInEditor.SpriteName = spriteId;

                if (ImGui.CollapsingHeader("Windows"))
                {
                    ImGui.Button("Add Window");
                    ImGui.Text("No windows...");
                }

                if (ImGui.CollapsingHeader("Attack Properties"))
                {
                    ImGui.Text("No properties");
                }

                bool isThrow = moveInEditor.IsThrow;
                ImGui.Text("Is move a throw?");
                ImGui.SameLine();
                ImGui.Checkbox("##isThrow", ref isThrow);
                moveInEditor.IsThrow = isThrow;

                if (ImGui.CollapsingHeader("Hurtboxes"))
                {
                    ImGui.Button("Add Hurtbox");
                    ImGui.Text("No hurtboxes...");
                }
            }
        }

        private void RenderPaletteEditor(float scale)
        {
        }

        private void RenderEditor(float scale)
        {
            var windowSize = new Vector2();
            windowSize.X = 450 * scale;
            windowSize.Y = 250 * scale;

            float windowYPos = height - 24 - windowSize.Y;

            var windowPos = new Vector2(width / 2 - windowSize.X / 2, windowYPos);

            ImGui.SetNextWindowPos(windowPos);
            ImGui.SetNextWindowSize(windowSize);

            if (ImGui.Begin("Editor", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.SetWindowFontScale(scale);

                switch (editorMode)
                {
                    case EditorMode.Move:
                        RenderMoveEditor(scale);
                        break;
                    case EditorMode.Palette:
                        RenderPaletteEditor(scale);
                        break;
                    case EditorMode.None:
                    default:
                        break;
                }

                ImGui.End();
            }
        }

        private void RenderSpriteDisplayArea(float scale, IScreenManager screenManager)
        {
            var windowSize = new Vector2();
            windowSize.X = 350 * scale;
            windowSize.Y = 375 * scale;

            ImGui.SetNextWindowPos(new Vector2(width / 2 - windowSize.X / 2, 20 * scale));
            ImGui.SetNextWindowSize(windowSize);

            if (ImGui.Begin("Sprite Animation Viewer", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoBackground))
            {
                ImGui.SetWindowFontScale(scale);

                var currentAnimationSpeed = spriteData != null ? spriteData.Sequence.playbackSpeed : 0.0f;

                //160 * screenManager.ScreenScale
                var cursorPos = new Vector2(0, spriteDrawPosition.Y + (screenManager.ScreenScale * 200));
                ImGui.SetCursorPos(cursorPos);

                var currentAnimationSpeedLabel =
                    currentAnimationSpeed == 0 ? "10 (default)" :
                    currentAnimationSpeed.ToString();

                ImGui.Text(" ");
                ImGui.Text($"Current Sprite: {spriteData.Name}");
                ImGui.Text($"Sprite Animation Speed (fps): { currentAnimationSpeedLabel }");
                ImGui.Text($"Current Frame: {currentFrame}");

                var imageButtonSize = new Vector2((advanceOneFrameBackTexture.width / 2) * scale, (advanceOneFrameBackTexture.height / 2) * scale);

                cursorPos = new Vector2(((windowSize.X / 2) - imageButtonSize.X * 2) - buttonSpacing * 3.75f, (310 * scale) - imageButtonSize.Y);

                ImGui.SetCursorPos(cursorPos);

                if (ImGui.ImageButton("##ADVBack", (IntPtr)advanceOneFrameBackTexture.id, imageButtonSize))
                {
                    animationPaused = true;
                    frameAdvance = FrameAdvance.Backward;
                }
                
                ImGui.SameLine();
                
                if (ImGui.ImageButton("##Play", (IntPtr)playButtonTexture.id, imageButtonSize))
                {
                    animationPaused = false;
                }
                
                ImGui.SameLine();

                if (ImGui.ImageButton("##Pause", (IntPtr)pauseButtonTexture.id, imageButtonSize))
                {
                    animationPaused = true;
                }
                
                ImGui.SameLine();

                if (ImGui.ImageButton("##ADVFwd", (IntPtr)advanceOneFrameForwardTexture.id, imageButtonSize))
                {
                    animationPaused = true;
                    frameAdvance = FrameAdvance.Forward;
                }

                ImGui.End();
            }
        }

        private void RenderCharacterDataWindow(float scale)
        {
            var windowSize = new Vector2();
            windowSize.X = 320 * scale;
            windowSize.Y = 620 * scale;
            
            ImGui.SetNextWindowPos(new Vector2(width - windowSize.X - 10 * scale, 20 * scale));
            ImGui.SetNextWindowSize(windowSize);

            if (ImGui.Begin("Character Properties", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.SetWindowFontScale(scale);
                if (ImGui.CollapsingHeader("Character Data", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    var characterName = character.Name ?? string.Empty;
                    
                    ImGui.Text("Character Name");
                    ImGui.SameLine();
                    ImGui.InputText("##CharacterName", ref characterName, 50);

                    character.Name = characterName;

                    ImGui.Text("Base Sprite");
                    ImGui.SameLine();
                    int selectedBaseSpriteIndex = string.IsNullOrWhiteSpace(character.BaseSprite) ? -1 : allSprites.IndexOf(allSprites.First(x => x.Name == character.BaseSprite));
                    ImGui.Combo("##BaseSpriteCombo", ref selectedBaseSpriteIndex, allSprites.Select(x => x.Name).ToArray(), allSprites.Count);
                    if (selectedBaseSpriteIndex > -1)
                    {
                        if (character.BaseSprite != allSprites[selectedBaseSpriteIndex].Name)
                        {
                            character.BaseSprite = allSprites[selectedBaseSpriteIndex].Name;
                            spriteData = allSprites[selectedBaseSpriteIndex];
                        }
                    }

                }
                if (ImGui.CollapsingHeader("Move Data", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    if (ImGui.Button("Add New Move"))
                    {
                        selectedMoveType = 0;
                        moveInEditor = new MoveDataModel();
                        editorMode = EditorMode.Move;
                    }
                    ImGui.Separator();
                    if (character.MoveData != null && character.MoveData.Count > 0)
                    {
                        foreach (var move in character.MoveData)
                        {

                        }
                    }
                    else
                    {
                        ImGui.Text("No Moves...");
                    }
                }

                ImGui.End();
            }
        }

        private void RenderMainMenu(float scale, IScreenManager screenManager)
        {
            ImGui.BeginMainMenuBar();

            ImGui.SetWindowFontScale(scale);

            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Save"))
                {
                    //exports json to the characterdata folder in the game
                    _characterOperations.SaveCharacter(character, projectData.ProjectPathOnly);
                }

                if (ImGui.MenuItem("Export..."))
                {
                    //shows windows modal to allow saving json to anywhere
                    var saveFile = Win32DialogHelper.ShowSaveFileDialog(
                        "JSON File (*.json)\0*.json\0All Files (*.*)\0*.*\0",
                        "Save Character JSON Output",
                        projectData.ProjectPathOnly,
                        ".json");

                    if (saveFile != string.Empty)
                    {
                        //export json to the destination
                        _characterOperations.SaveCharacter(character, projectData.ProjectPathOnly, saveFile);
                    }
                }

                ImGui.Separator();

                if (ImGui.MenuItem("Clear all data (reset)"))
                {
                    Init(new { width, height, projectData, action, character = new CharacterDataModel() });
                }

                ImGui.Separator();

                if (ImGui.MenuItem("Close Character"))
                {
                    _logger.LogInformation("Project closed");
                    screenManager.NavigateTo(typeof(ProjectHomeScreen), new { height = height, width = width, projectData = projectData });
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

            if (ImGui.BeginMenu("Edit"))
            {
                ImGui.End();
            }

            ImGui.EndMainMenuBar();
        }
    }
}
