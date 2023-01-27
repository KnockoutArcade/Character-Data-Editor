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

        private MoveDataModel moveInEditor;
        private PaletteModel paletteInEditor;
        private List<string> moveTypesList = new List<string>();
        private List<string> attackTypesList = new List<string>();
        private List<SpriteDataModel> allSprites;

        private string spriteToDraw;
        private string prevSpriteToDraw;
        private SpriteDataModel spriteData;
        private SpriteDrawingHelper spriteDrawer;
        private Vector2 spriteDrawPosition;

        private bool animationPaused;
        private FrameAdvance frameAdvance;

        private EditorMode editorMode;

        private int currentFrame;
        private AnimatedSpriteReturnDataModel spriteDrawData;

        private const int buttonSpacing = 8;

        private string editorWindowTitle;

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
                var sprite = allSprites.FirstOrDefault(x => x.Name == character.BaseSprite);
                ChangeAnimatedSprite(sprite);
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

            var attackTypes = Enum.GetValues(typeof(AttackType));
            attackTypesList = new List<string>();

            foreach (AttackType item in attackTypes)
            {
                var itemAsString = item.ToString().AddSpacesToCamelCase();

                attackTypesList.Add(itemAsString);
            }

            //init spritedrawposition
            spriteDrawPosition = new Vector2(0, 40);

            currentFrame = 0;
            frameAdvance = FrameAdvance.None;

            editorWindowTitle = "Editor";
        }

        public void RenderImGui(IScreenManager screenManager)
        {
            RenderMainMenu(screenManager.ScreenScale, screenManager);
            RenderCharacterDataWindow(screenManager.ScreenScale);
            RenderSpriteDisplayArea(screenManager.ScreenScale, screenManager);
            RenderEditor(screenManager.ScreenScale);
            RenderPaletteWindow(screenManager.ScreenScale);
        }

        public void RenderAfterImGui(IScreenManager screenManager)
        {
            RenderAnimatedSprite(screenManager.ScreenScale);
        }

        private void ChangeAnimatedSprite(SpriteDataModel sprite)
        {
            spriteData = sprite;
        }

        private void RenderAnimatedSprite(float scale)
        {
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
                var frameData = spriteDrawer.DrawSpecificFrameSpriteToScreen(spriteData, spriteDrawPosition, scale, _logger, maxSpriteSize, frameAdvance, animationFlags);

                currentFrame = frameData.CurrentFrame;
                spriteDrawData = frameData;

                frameAdvance = FrameAdvance.None;
            }
            else
            {
                var frameData = spriteDrawer.DrawSpriteToScreen(spriteData, spriteDrawPosition, scale, ResourceConstants.LogoPath, _logger, maxSpriteSize, animationFlags);

                currentFrame = frameData.CurrentFrame;
                spriteDrawData = frameData;
            }
        }

        private void RenderPaletteWindow(float scale)
        {
            //set the window size, scale as necessary
            var windowSize = new Vector2();
            windowSize.X = 320 * scale;
            windowSize.Y = 620 * scale;

            ImGui.SetNextWindowPos(new Vector2(10 * scale, 20 * scale));
            ImGui.SetNextWindowSize(windowSize);

            if (ImGui.Begin("Palette Editor", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.SetWindowFontScale(scale);

                var baseSelected = paletteInEditor == character.BaseColor;
                if (ImGui.Selectable("Base Palette##base-100", baseSelected))
                {
                    if (character.BaseColor == null)
                    {
                        character.BaseColor = new PaletteModel();
                        character.BaseColor.Name = "Base Palette";
                    }

                    paletteInEditor = character.BaseColor;
                    moveInEditor = null;
                    editorMode = EditorMode.BasePalette;
                }

                ImguiDrawingHelper.DrawVerticalSpacing(scale, 5.0f);

                //load each of the palettes in by name...
                if (ImGui.CollapsingHeader("Alternate Palettes", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    if (ImGui.Button("New Palette"))
                    {
                        var newPalette = new PaletteModel();
                        character.Palettes.Add(newPalette);
                        paletteInEditor = newPalette;
                        moveInEditor = null;
                        editorMode = EditorMode.Palette;
                    }

                    ImguiDrawingHelper.DrawVerticalSpacing(scale, 5.0f);

                    for (int i = 0; i < character.Palettes.Count; i++)
                    {
                        var palette = character.Palettes[i];

                        var paletteSelected = paletteInEditor == palette;

                        if (ImguiDrawingHelper.DrawSelectableWithRemove(() =>
                            {
                                paletteInEditor = palette;
                                moveInEditor = null;
                                editorMode = EditorMode.Palette;
                            }, palette.Name, paletteSelected, i))
                        {
                            if (paletteInEditor == palette)
                            {
                                paletteInEditor = null;
                                editorMode = EditorMode.None;
                            }

                            character.Palettes.RemoveAt(i);
                        }
                    }
                }

                ImGui.End();
            }
        }

        private void RenderMoveEditor(float scale)
        {
            editorWindowTitle = "Move Editor";

            if (moveInEditor == null)
            {
                ImGui.Text("No move selected...");
            }
            else
            {
                int selectedMoveType = (int)moveInEditor.MoveType;

                ImGui.Text("Move Type");
                ImGui.SameLine();

                ImGui.Combo("##MoveType", ref selectedMoveType, moveTypesList.ToArray(), moveTypesList.Count);

                moveInEditor.MoveType = (MoveType)selectedMoveType;
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
                        var sprite = allSprites[selectedSpriteIndex];
                        ChangeAnimatedSprite(sprite);
                    }
                }
                
                if (ImGui.CollapsingHeader("Windows"))
                {
                    int windowCount = moveInEditor.NumberOfFrames;

                    ImguiDrawingHelper.DrawIntInput("numberOfWindows", ref windowCount);

                    if (windowCount < 0)
                    {
                        windowCount = 0;
                    }

                    if (windowCount < moveInEditor.NumberOfFrames)
                    {
                        while (windowCount < moveInEditor.NumberOfFrames)
                        {
                            moveInEditor.FrameData.RemoveAt(moveInEditor.NumberOfFrames - 1);
                        }
                    }
                    else
                    {
                        while (windowCount > moveInEditor.NumberOfFrames)
                        {
                            moveInEditor.FrameData.Add(new FrameDataModel());
                        }
                    }

                    if (windowCount == 0)
                    {
                        ImGui.Text("No windows");
                    }
                    else
                    {
                        for (int i = 0; i < moveInEditor.NumberOfFrames; i++)
                        {
                            var windowItem = moveInEditor.FrameData[i];

                            if (ImGui.TreeNode($"Window [{i}]"))
                            {
                                int imageIndex = windowItem.ImageIndex;
                                int length = windowItem.Length;

                                ImguiDrawingHelper.DrawIntInput("imageIndex", ref imageIndex);
                                ImguiDrawingHelper.DrawIntInput("length", ref length);

                                windowItem.ImageIndex = imageIndex;
                                windowItem.Length = length;

                                ImGui.TreePop();
                            }
                        }
                    }
                }

                if (character.MoveData == null)
                {
                    character.MoveData = new List<MoveDataModel>();
                }

                if (ImGui.CollapsingHeader("Attack Properties"))
                {
                    int hitboxCount = moveInEditor.NumberOfHitboxes;

                    ImguiDrawingHelper.DrawIntInput("numberOfHitboxes", ref hitboxCount);

                    if (hitboxCount < 0)
                    {
                        hitboxCount = 0;
                    }

                    if (hitboxCount < moveInEditor.NumberOfHitboxes)
                    {
                        while (hitboxCount < moveInEditor.NumberOfHitboxes)
                        {
                            moveInEditor.AttackData.RemoveAt(moveInEditor.NumberOfHitboxes - 1);
                        }
                    }
                    else
                    {
                        while (hitboxCount > moveInEditor.NumberOfHitboxes)
                        {
                            moveInEditor.AttackData.Add(new AttackDataModel());
                        }
                    }

                    if (hitboxCount == 0)
                    {
                        ImGui.Text("No properties");
                    }
                    else
                    {
                        for (int i = 0; i < moveInEditor.NumberOfHitboxes; i++)
                        {
                            var attackDataItem = moveInEditor.AttackData[i];

                            if (ImGui.TreeNode($"Hitbox [{i}]"))
                            {
                                int start = attackDataItem.Start;
                                int lifetime = attackDataItem.Lifetime;
                                int attackWidth = attackDataItem.AttackWidth;
                                int attackHeight = attackDataItem.AttackHeight;
                                int widthOffset = attackDataItem.WidthOffset;
                                int heightOffset = attackDataItem.HeightOffset;
                                int group = attackDataItem.Group;
                                int damage = attackDataItem.Damage;
                                int attackHitstop = attackDataItem.AttackHitStop;
                                int attackHitstun = attackDataItem.AttackHitStun;
                                AttackType attackType = attackDataItem.AttackType;
                                int blockStun = attackDataItem.BlockStun;
                                int knockBack = attackDataItem.KnockBack;
                                int airKnockbackH = attackDataItem.AirKnockbackHorizontal;
                                int airKnockbackV = attackDataItem.AirKnockbackVertical;
                                bool launches = attackDataItem.Launches;
                                int launchKnockbackV = attackDataItem.LaunchKnockbackVertical;
                                int launchKnockbackH = attackDataItem.LaunchKnockbackHorizontal;
                                int pushback = attackDataItem.Pushback;
                                int particleOffsetX = attackDataItem.ParticleXOffset;
                                int particleOffsetY = attackDataItem.ParticleYOffset;
                                string particleEffect = attackDataItem.ParticleEffect;
                                int particleDuration = attackDataItem.ParticleDuration;
                                int holdOffsetX = attackDataItem.HoldXOffset;
                                int holdOffsetY = attackDataItem.HoldYOffset;

                                ImguiDrawingHelper.DrawIntInput("start", ref start);
                                ImguiDrawingHelper.DrawIntInput("lifetime", ref lifetime);
                                ImguiDrawingHelper.DrawIntInput("attackWidth", ref attackWidth);
                                ImguiDrawingHelper.DrawIntInput("attackHeight", ref attackHeight);
                                ImguiDrawingHelper.DrawIntInput("widthOffset", ref widthOffset);
                                ImguiDrawingHelper.DrawIntInput("heightOffset", ref heightOffset);
                                ImguiDrawingHelper.DrawIntInput("group", ref group);
                                ImguiDrawingHelper.DrawIntInput("damage", ref damage);
                                ImguiDrawingHelper.DrawIntInput("attackHitStop", ref attackHitstop);
                                ImguiDrawingHelper.DrawIntInput("attackHitStun", ref attackHitstun);

                                int selectedAttackType = (int)attackType;
                                ImguiDrawingHelper.DrawComboInput("attackType", attackTypesList.ToArray(), ref selectedAttackType);

                                ImguiDrawingHelper.DrawIntInput("blockStun", ref blockStun);
                                ImguiDrawingHelper.DrawIntInput("knockback", ref knockBack);
                                ImguiDrawingHelper.DrawIntInput("airKnockbackVertical", ref airKnockbackV);
                                ImguiDrawingHelper.DrawIntInput("airKnockbackHorizontal", ref airKnockbackH);
                                ImguiDrawingHelper.DrawBoolInput("launches", ref launches);
                                ImguiDrawingHelper.DrawIntInput("launchKnockbackVertical", ref launchKnockbackV);
                                ImguiDrawingHelper.DrawIntInput("launchKnockbackHorizontal", ref launchKnockbackH);
                                ImguiDrawingHelper.DrawIntInput("pushback", ref pushback);
                                ImguiDrawingHelper.DrawIntInput("particleOffsetX", ref particleOffsetX);
                                ImguiDrawingHelper.DrawIntInput("particleOffsetY", ref particleOffsetY);

                                int selectedParticleEffect = string.IsNullOrWhiteSpace(particleEffect) ? 0 : allSprites.IndexOf(allSprites.Where(x => x.Name == particleEffect).First());
                                ImguiDrawingHelper.DrawComboInput("particleEffect", allSprites.Select(x => x.Name).ToArray(), ref selectedParticleEffect);
                                
                                ImguiDrawingHelper.DrawIntInput("particleDuration", ref particleDuration);
                                ImguiDrawingHelper.DrawIntInput("holdOffsetX", ref holdOffsetX);
                                ImguiDrawingHelper.DrawIntInput("holdOffsetY", ref holdOffsetY);

                                attackDataItem.Start = start;
                                attackDataItem.Lifetime = lifetime;
                                attackDataItem.AttackWidth = attackWidth;
                                attackDataItem.AttackHeight = attackHeight;
                                attackDataItem.WidthOffset = widthOffset;
                                attackDataItem.HeightOffset = heightOffset;
                                attackDataItem.Group = group;
                                attackDataItem.Damage = damage;
                                attackDataItem.AttackHitStop = attackHitstop;
                                attackDataItem.AttackHitStun = attackHitstun;
                                attackDataItem.AttackType = (AttackType)selectedAttackType;
                                attackDataItem.BlockStun = blockStun;
                                attackDataItem.KnockBack = knockBack;
                                attackDataItem.AirKnockbackHorizontal = airKnockbackH;
                                attackDataItem.AirKnockbackVertical = airKnockbackV;
                                attackDataItem.Launches = launches;
                                attackDataItem.LaunchKnockbackHorizontal = launchKnockbackH;
                                attackDataItem.LaunchKnockbackVertical = launchKnockbackV;
                                attackDataItem.Pushback = pushback;
                                attackDataItem.ParticleXOffset = particleOffsetX;
                                attackDataItem.ParticleYOffset = particleOffsetY;
                                attackDataItem.ParticleEffect = allSprites[selectedParticleEffect].Name;
                                attackDataItem.ParticleDuration = particleDuration;
                                attackDataItem.HoldXOffset = holdOffsetX;
                                attackDataItem.HoldYOffset = holdOffsetY;

                                ImGui.TreePop();
                            }
                        }
                    }
                }

                bool isThrow = moveInEditor.IsThrow;
                ImGui.Text("Is move a throw?");
                ImGui.SameLine();
                ImGui.Checkbox("##isThrow", ref isThrow);
                moveInEditor.IsThrow = isThrow;

                if (ImGui.CollapsingHeader("Hurtboxes"))
                {
                    int hurtboxCount = moveInEditor.NumberOfHurtboxes;

                    ImguiDrawingHelper.DrawIntInput("numberOfHurtboxes", ref hurtboxCount);

                    if (hurtboxCount < 0)
                    {
                        hurtboxCount = 0;
                    }

                    if (hurtboxCount < moveInEditor.NumberOfHurtboxes)
                    {
                        while (hurtboxCount < moveInEditor.NumberOfHurtboxes)
                        {
                            moveInEditor.HurtboxData.RemoveAt(moveInEditor.NumberOfHurtboxes - 1);
                        }
                    }
                    else
                    {
                        while (hurtboxCount > moveInEditor.NumberOfHurtboxes)
                        {
                            moveInEditor.HurtboxData.Add(new HurtboxDataModel());
                        }
                    }

                    if (hurtboxCount == 0)
                    {
                        ImGui.Text("No hurtboxes");
                    }
                    else
                    {
                        for (int i = 0; i < moveInEditor.NumberOfHurtboxes; i++)
                        {
                            var currentHurtbox = moveInEditor.HurtboxData[i];

                            if (ImGui.TreeNode($"Hurtbox [{i}]"))
                            {
                                int start = currentHurtbox.Start;
                                int lifetime = currentHurtbox.Lifetime;
                                int attackWidth = currentHurtbox.AttackWidth;
                                int attackHeight = currentHurtbox.AttackHeight;
                                int widthOffset = currentHurtbox.WidthOffset;
                                int heightOffset = currentHurtbox.HeightOffset;

                                ImguiDrawingHelper.DrawIntInput("start", ref start);
                                ImguiDrawingHelper.DrawIntInput("lifetime", ref lifetime);
                                ImguiDrawingHelper.DrawIntInput("attackWidth", ref attackWidth);
                                ImguiDrawingHelper.DrawIntInput("attackHeight", ref attackHeight);
                                ImguiDrawingHelper.DrawIntInput("widthOffset", ref widthOffset);
                                ImguiDrawingHelper.DrawIntInput("heightOffset", ref heightOffset);

                                currentHurtbox.Start = start;
                                currentHurtbox.Lifetime = lifetime;
                                currentHurtbox.AttackWidth = attackWidth;
                                currentHurtbox.AttackHeight = attackHeight;
                                currentHurtbox.WidthOffset = widthOffset;
                                currentHurtbox.HeightOffset = heightOffset;

                                ImGui.TreePop();
                            }
                        }
                    }
                }
            }
        }

        private void RenderPaletteEditor(float scale)
        {
            var basePalette = editorMode == EditorMode.BasePalette;

            editorWindowTitle = "Palette Editor";

            if (paletteInEditor == null)
            {
                ImGui.Text("No palette selected...");
            }
            else
            {
                if (!basePalette)
                {
                    var name = paletteInEditor.Name;

                    ImguiDrawingHelper.DrawStringInput("paletteName", ref name);

                    paletteInEditor.Name = name;
                }
                else
                {
                    ImGui.Columns(2);
                    ImGui.Text("Palette Name");
                    ImGui.NextColumn();
                    ImGui.Text(paletteInEditor.Name);
                    ImGui.Columns(1);
                }

                if (paletteInEditor.ColorPalette == null)
                {
                    paletteInEditor.ColorPalette = new List<RGBModel>();
                }

                //if it's the base palette, this is can be changed... otherwise its the same as the base palette
                int numberOfReplacableColors = basePalette ? paletteInEditor.NumberOfReplacableColors : character.BaseColor.NumberOfReplacableColors;

                if (basePalette)
                {
                    ImguiDrawingHelper.DrawIntInput("numberOfPalettes", ref numberOfReplacableColors);
                }

                if (numberOfReplacableColors < 0)
                {
                    numberOfReplacableColors = 0;
                }

                if (numberOfReplacableColors < paletteInEditor.NumberOfReplacableColors)
                {
                    while (numberOfReplacableColors < paletteInEditor.NumberOfReplacableColors)
                    {
                        paletteInEditor.ColorPalette.RemoveAt(paletteInEditor.NumberOfReplacableColors - 1);
                    }
                }
                else
                {
                    while (numberOfReplacableColors > paletteInEditor.NumberOfReplacableColors)
                    {
                        paletteInEditor.ColorPalette.Add(new RGBModel());
                    }
                }

                ImguiDrawingHelper.DrawVerticalSpacing(scale, 15.0f);

                if (numberOfReplacableColors == 0)
                {
                    ImGui.Text("No replacable colors");
                }
                else
                {
                    var palettes = paletteInEditor.ColorPalette;

                    ImguiDrawingHelper.DrawPaletteEditor(ref palettes, scale);

                    paletteInEditor.ColorPalette = palettes;
                }
            }
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

            if (ImGui.Begin($"{editorWindowTitle}##EditorWindow", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.SetWindowFontScale(scale);

                switch (editorMode)
                {
                    case EditorMode.Move:
                        RenderMoveEditor(scale);
                        break;
                    case EditorMode.Palette:
                    case EditorMode.BasePalette:
                        RenderPaletteEditor(scale);
                        break;
                    case EditorMode.None:
                    default:
                        editorWindowTitle = "Editor";
                        break;
                }

                ImGui.End();
            }
        }

        private void RenderSpriteDisplayArea(float scale, IScreenManager screenManager)
        {
            var windowSize = new Vector2();
            windowSize.X = 350 * scale;
            windowSize.Y = 333 * scale;

            ImGui.SetNextWindowPos(new Vector2(width / 2 - windowSize.X / 2, 20 * scale));
            ImGui.SetNextWindowSize(windowSize);

            if (ImGui.Begin("Sprite Animation Viewer", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize))
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

                var moveInAmount = 35.0f;
                var curPos = ImGui.GetCursorPos();
                curPos.X += moveInAmount * scale;
                ImGui.SetCursorPos(curPos);

                ImGui.Text($"Current Sprite: {spriteData.Name}");

                curPos = ImGui.GetCursorPos();
                curPos.X += moveInAmount * scale;
                ImGui.SetCursorPos(curPos);

                ImGui.Text($"Sprite Animation Speed (fps): { currentAnimationSpeedLabel }");

                curPos = ImGui.GetCursorPos();
                curPos.X += moveInAmount * scale;
                ImGui.SetCursorPos(curPos);

                ImGui.Text($"Current Frame: {currentFrame}");

                curPos = ImGui.GetCursorPos();
                curPos.X += moveInAmount * scale;
                ImGui.SetCursorPos(curPos);

                ImGui.Text($"Total Frames: {spriteData.Frames.Count}");

                var imageButtonSize = new Vector2((advanceOneFrameBackTexture.width / 2) * scale, (advanceOneFrameBackTexture.height / 2) * scale);

                cursorPos = new Vector2(((windowSize.X / 2) - imageButtonSize.X * 2) - buttonSpacing * 3.75f, (315 * scale) - imageButtonSize.Y);

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

                    ImGui.BeginTable("characterDataTable", 2);
                    var tableFlags = ImGuiTableColumnFlags.NoSort;
                    ImGui.TableSetupColumn("", tableFlags, ImGui.CalcTextSize("Character Name ").X);
                    ImGui.TableSetupColumn("", tableFlags | ImGuiTableColumnFlags.WidthStretch);

                    ImGui.TableNextColumn();

                    ImGui.Text("Character Name");
                    ImGui.TableNextColumn();
                    ImGui.InputText("##CharacterName", ref characterName, 50);

                    character.Name = characterName;
                    
                    ImGui.TableNextColumn();

                    var baseSpriteData = allSprites.Where(x => x.Name == character.BaseSprite).FirstOrDefault();
                    var baseSpritePlaying = spriteData == baseSpriteData ? "*" : "";

                    if (ImGui.Selectable($"Base Sprite{baseSpritePlaying}"))
                    {
                        if (baseSpriteData != null && spriteData != baseSpriteData)
                        {
                            ChangeAnimatedSprite(baseSpriteData);
                        }
                    }

                    ImGui.TableNextColumn();
                    int selectedBaseSpriteIndex = string.IsNullOrWhiteSpace(character.BaseSprite) ? -1 : allSprites.IndexOf(allSprites.First(x => x.Name == character.BaseSprite));
                    ImGui.Combo("##BaseSpriteCombo", ref selectedBaseSpriteIndex, allSprites.Select(x => x.Name).ToArray(), allSprites.Count);
                    if (selectedBaseSpriteIndex > -1)
                    {
                        if (character.BaseSprite != allSprites[selectedBaseSpriteIndex].Name)
                        {
                            character.BaseSprite = allSprites[selectedBaseSpriteIndex].Name;
                            var sprite = allSprites[selectedBaseSpriteIndex];
                            ChangeAnimatedSprite(sprite);
                        }
                    }

                    ImGui.EndTable();

                }

                ImguiDrawingHelper.DrawVerticalSpacing(scale, 5.0f);
                
                if (ImGui.CollapsingHeader("Move Data", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    if (ImGui.Button("Add New Move"))
                    {
                        moveInEditor = new MoveDataModel();
                        paletteInEditor = null;
                        
                        if (character.MoveData == null)
                        {
                            character.MoveData = new List<MoveDataModel>();
                        }

                        character.MoveData.Add(moveInEditor);
                        editorMode = EditorMode.Move;
                    }
                    ImguiDrawingHelper.DrawVerticalSpacing(scale, 5.0f);
                    if (character.MoveData != null && character.MoveData.Count > 0)
                    {
                        for (int i = 0; i < character.MoveData.Count; i++)
                        {
                            var selected = character.MoveData[i] == moveInEditor;
                            var moveSpriteIndex = string.IsNullOrWhiteSpace(character.MoveData[i].SpriteName) ? -1 : allSprites.IndexOf(allSprites.First(x => x.Name == character.MoveData[i].SpriteName));
                            var playingIndicator = spriteData == allSprites[moveSpriteIndex] ? "*" : "";

                            if (ImguiDrawingHelper.DrawSelectableWithRemove(() =>
                                {
                                    moveInEditor = character.MoveData[i];
                                    paletteInEditor = null;
                                    editorMode = EditorMode.Move;
                                    
                                    if (moveSpriteIndex > -1)
                                    {
                                        var sprite = allSprites[moveSpriteIndex];
                                        ChangeAnimatedSprite(sprite);
                                    }
                                }, $"{character.MoveData[i].MoveType.ToString().AddSpacesToCamelCase()}{playingIndicator}", selected, i))
                            {
                                //if selected, unselect
                                if (moveInEditor == character.MoveData[i])
                                {
                                    moveInEditor = null;
                                    editorMode = EditorMode.None;
                                }

                                //remove it here
                                character.MoveData.RemoveAt(i);
                            }
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
