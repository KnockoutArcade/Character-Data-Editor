﻿using CharacterDataEditor.Constants;
using CharacterDataEditor.Enums;
using CharacterDataEditor.Extensions;
using CharacterDataEditor.Helpers;
using CharacterDataEditor.Models;
using CharacterDataEditor.Models.ProjectileData;
using CharacterDataEditor.Services;
using CharacterDataEditor.NAudio;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using Raylib_cs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using NAudio.Wave;
using CharacterDataEditor.Models.CharacterData;

namespace CharacterDataEditor.Screens
{
    public class EditProjectileScreen : IScreen
    {
        private readonly ILogger<IScreen> _logger;
        private readonly IProjectileOperations _projectileOperations;

        private float width;
        private float height;
        private ProjectileDataModel projectile;
        private ProjectileDataModel originalProjectile;
        private RecentProjectModel projectData;
        private string action;

        private Texture2D playButtonTexture;
        private Texture2D pauseButtonTexture;
        private Texture2D advanceOneFrameForwardTexture;
        private Texture2D advanceOneFrameBackTexture;
        private Texture2D showHitboxesTexture;
        private Texture2D hideHitboxesTexture;
        private Texture2D soundPlayTexture;
        private Texture2D soundMuteTexture;

        private List<string> positionTypesList = new List<string>();
        private List<string> attackTypesList = new List<string>();
        private ProjectilePaletteModel paletteInEditor;
        private List<SpriteDataModel> allSprites;
        private List<ScriptDataModel> allScripts;
        private List<SoundDataModel> allSounds;

        private string spriteToDraw;
        private string prevSpriteToDraw;
        private SpriteDataModel spriteData;
        private ProjectilePaletteModel paletteData;
        private SpriteDrawingHelper spriteDrawer;
        private Vector2 spriteDrawPosition;

        private bool animationPaused;
        private FrameAdvance frameAdvance;

        private EditorMode editorMode;
        private BoxDrawMode boxDrawMode;

        private List<List<Rectangle>> hitboxRects;
        private List<List<Rectangle>> hurtboxRects;
        private bool showHitHurtboxes;

        private int currentFrame;
        private int totalFrames;
        private List<int> windows;
        private bool resetAnimation; // Resets the current animation in SpriteDrawingHelper whenever a different move is selected
        private bool showingMove;
        private AnimatedSpriteReturnDataModel spriteDrawData;

        private int frameCounter; // This is used for checking unsaved work
        private bool unsaved;
        private bool exiting;

        private bool playSound;
        //private List<CachedSound> soundPlayers;

        private const int buttonSpacing = 8;

        private string editorWindowTitle;

        private bool isMoveAnimationRender;

        private delegate void AfterConfirmAction(int keyCode);
        private AfterConfirmAction exitConfirmAction;

        public EditProjectileScreen(ILogger<IScreen> logger, IProjectileOperations projectileOperations)
        {
            _logger = logger;
            _projectileOperations = projectileOperations;
        }

        public void Init(dynamic screenData)
        {
            Raylib.SetWindowTitle(TitleConstants.EditProjectileTitle);

            spriteToDraw = string.Empty;
            prevSpriteToDraw = string.Empty;
            spriteData = null;
            editorMode = EditorMode.BasePalette;

            paletteInEditor = null;
            unsaved = false;

            width = screenData?.width ?? 1280;
            height = screenData?.height ?? 720;
            projectData = screenData?.projectData ?? new RecentProjectModel();
            action = screenData?.action ?? "new";
            projectile = action == "edit" ? screenData.projectile : new ProjectileDataModel();
            //copies the data from projectile into original projectile
            originalProjectile = projectile.Clone();

            allSprites = _projectileOperations.GetAllGameData<SpriteDataModel>(projectData.ProjectPathOnly);
            allScripts = _projectileOperations.GetAllGameData<ScriptDataModel>(projectData.ProjectPathOnly);
            allSounds = _projectileOperations.GetAllGameData<SoundDataModel>(projectData.ProjectPathOnly);

            playSound = false;
            //soundPlayers = new List<CachedSound>();

            if (action == "edit" && !string.IsNullOrWhiteSpace(projectile.ProjectileSprites?.Sprite))
            {
                var sprite = allSprites.FirstOrDefault(x => x.Name == projectile.ProjectileSprites.Sprite);
                ChangeAnimatedSprite(sprite);
            }

            playButtonTexture = Raylib.LoadTexture(Path.Combine(AppContext.BaseDirectory, ResourceConstants.PlayButtonPath));
            pauseButtonTexture = Raylib.LoadTexture(Path.Combine(AppContext.BaseDirectory, ResourceConstants.PauseButtonPath));
            advanceOneFrameForwardTexture = Raylib.LoadTexture(Path.Combine(AppContext.BaseDirectory, ResourceConstants.AdvanceOneFrameButtonPath));
            advanceOneFrameBackTexture = Raylib.LoadTexture(Path.Combine(AppContext.BaseDirectory, ResourceConstants.AdvanceOneFrameBackButtonPath));
            showHitboxesTexture = Raylib.LoadTexture(Path.Combine(AppContext.BaseDirectory, ResourceConstants.ShowHitboxes));
            hideHitboxesTexture = Raylib.LoadTexture(Path.Combine(AppContext.BaseDirectory, ResourceConstants.HideHitboxes));
            soundPlayTexture = Raylib.LoadTexture(Path.Combine(AppContext.BaseDirectory, ResourceConstants.SoundPlay));
            soundMuteTexture = Raylib.LoadTexture(Path.Combine(AppContext.BaseDirectory, ResourceConstants.SoundMute));

            spriteDrawer = new SpriteDrawingHelper();
            frameCounter = 0;

            var positionTypes = Enum.GetValues(typeof(PositionType));
            positionTypesList = new List<string>();

            foreach (PositionType item in positionTypes)
            {
                var itemAsString = item.ToString().AddSpacesToCamelCase();

                positionTypesList.Add(itemAsString);
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
            totalFrames = 0;
            windows = new List<int>();
            resetAnimation = false;
            showingMove = false;
            frameAdvance = FrameAdvance.None;
            boxDrawMode = BoxDrawMode.None;
            hitboxRects = new List<List<Rectangle>>();
            hurtboxRects = new List<List<Rectangle>>();
            showHitHurtboxes = false;

            editorWindowTitle = "Editor";
        }

        public void RenderImGui(IScreenManager screenManager)
        {
            RenderMainMenu(screenManager.ScreenScale, screenManager);
            RenderProjectileDataWindow(screenManager.ScreenScale);
            RenderSpriteDisplayArea(screenManager.ScreenScale, screenManager);
            RenderEditor(screenManager.ScreenScale);
            RenderPaletteWindow(screenManager.ScreenScale);
            CheckForUnsavedWork();
        }

        public void RenderAfterImGui(IScreenManager screenManager)
        {
            RenderAnimatedSprite(screenManager.ScreenScale);

            RenderHitHurtBox(screenManager.ScreenScale);

            if (exiting)
            {
                var pressedKey = Raylib.GetKeyPressed();
                if (pressedKey == 83 || pressedKey == 88 || pressedKey == 67)
                {
                    if (exitConfirmAction != null)
                    {
                        exitConfirmAction(pressedKey);
                    }
                }

                var messageRect = new Rectangle();
                messageRect.x = 0.0f;
                messageRect.height = (200.0f * screenManager.ScreenScale);
                messageRect.width = width;
                messageRect.y = (height / 2.0f) - messageRect.height / 2.0f;

                Raylib.DrawRectanglePro(messageRect, Vector2.Zero, 0.0f, Color.BLACK);

                var fontSize = (int)(24.0f * screenManager.ScreenScale);

                var messageWidth = Raylib.MeasureText(MessageConstants.UnsavedMessage, fontSize);

                var messageXCoord = (int)((width / 2.0f) - (messageWidth / 2.0f));
                var messageYCoord = (int)((height / 2.0f) - (fontSize / 2.0f));

                Raylib.DrawText(MessageConstants.UnsavedMessage,
                    messageXCoord, messageYCoord, fontSize, Color.WHITE);
            }
        }

        private void CheckForUnsavedWork()
        {
            //check if the current frame is greater than or equal to 30 (every half second ish)
            //then increment it
            if (frameCounter++ >= 30)
            {
                frameCounter = 0;
                if (!projectile.Equals(originalProjectile))
                {
                    Raylib.SetWindowTitle($"{TitleConstants.EditProjectileTitle}{TitleConstants.UnsavedIndicator}");
                    unsaved = true;
                }
                else
                {
                    Raylib.SetWindowTitle(TitleConstants.EditProjectileTitle);
                    unsaved = false;
                }
            }
        }

        private void ChangeAnimatedSprite(SpriteDataModel sprite, bool useFrameData = false)
        {
            //disable box drawing when the change first happens
            boxDrawMode = BoxDrawMode.None;

            isMoveAnimationRender = useFrameData;

            spriteData = sprite;
        }

        private void ChangeRenderedPalette(ProjectilePaletteModel palette)
        {
            paletteData = palette;
        }

        private void RenderAnimatedSprite(float scale)
        {
            var maxSpriteSize = new Vector2(200, 200);

            var animationFlags = SpriteDrawFlags.CenterHorizontal | SpriteDrawFlags.ShowSpriteOutline | SpriteDrawFlags.DrawOrigin;

            var useShader = projectile.BaseColor != null && paletteData != null;

            if (animationPaused)
            {
                animationFlags |= SpriteDrawFlags.Pause;
            }
            else
            {
                animationFlags &= ~SpriteDrawFlags.Pause;
            }

            if (useShader)
            {
                animationFlags |= SpriteDrawFlags.PaletteSwapActive;
            }
            else
            {
                animationFlags &= ~SpriteDrawFlags.PaletteSwapActive;
            }

            if (frameAdvance != FrameAdvance.None)
            {
                var frameData = spriteDrawer.DrawSpecificFrameSpriteToScreen(new SpriteDrawDataModel
                {
                    SpriteData = spriteData,
                    ProjectileBaseColor = projectile.BaseColor,
                    ProjectileSwapColor = paletteData,
                    DrawPosition = spriteDrawPosition,
                    Scale = scale,
                    Logger = _logger,
                    MaxDrawSize = maxSpriteSize,
                    FrameAdvance = frameAdvance,
                    Flags = animationFlags
                }, false, totalFrames, windows, resetAnimation);

                currentFrame = frameData.CurrentFrame;
                spriteDrawData = frameData;
                resetAnimation = false;

                frameAdvance = FrameAdvance.None;
            }
            else
            {
                var frameData = spriteDrawer.DrawSpriteToScreen(new SpriteDrawDataModel
                {
                    SpriteData = spriteData,
                    ProjectileBaseColor = projectile.BaseColor,
                    ProjectileSwapColor = paletteData,
                    DrawPosition = spriteDrawPosition,
                    Scale = scale,
                    Logger = _logger,
                    MaxDrawSize = maxSpriteSize,
                    DefaultTexture = ResourceConstants.LogoPath,
                    Flags = animationFlags,
                    EnableFrameDataDraw = isMoveAnimationRender,
                    FrameDrawData = null
                }, false, totalFrames, windows, resetAnimation);

                currentFrame = frameData.CurrentFrame;
                spriteDrawData = frameData;
                resetAnimation = false;
            }

            // Handle playing sound effect
            if (!animationPaused && playSound)
            {
                
            }
        }

        private void RenderHitHurtBox(float scale)
        {
            Color hitboxDrawColor = Color.RED;

            if (spriteData != null)
            {
                var spriteFinalScale = spriteDrawData.ScaledDrawSize.X / spriteData.Width;

                //draw the box
                switch (boxDrawMode)
                {
                    case BoxDrawMode.Both:
                    case BoxDrawMode.Hitbox:
                        if (showingMove)
                        {
                            hitboxRects.Clear();
                            for (int i = 0; i < projectile.NumberOfHitboxes; i++)
                            {
                                hitboxRects.Add(new List<Rectangle>());

                                var attackDataItem = projectile.AttackData[i];

                                hitboxRects[i].Clear();
                                int tempLifetime = 0;
                                for (int j = 0; j <= totalFrames; j++)
                                {
                                    if (j == attackDataItem.Start)
                                    {
                                        tempLifetime = attackDataItem.Lifetime;
                                    }

                                    for (int k = 0; k < projectile.RehitData.HitOnFrames.Count; k++)
                                    {
                                        if (j == projectile.RehitData.HitOnFrames[k] && projectile.RehitData.HitBox - 1 == i)
                                        {
                                            tempLifetime = attackDataItem.Lifetime;
                                            break;
                                        }
                                    }

                                    if (tempLifetime <= 0)
                                    {
                                        hitboxRects[i].Add(new Rectangle(0, 0, 0, 0));
                                    }
                                    else
                                    {
                                        hitboxRects[i].Add(new Rectangle(attackDataItem.WidthOffset, attackDataItem.HeightOffset, attackDataItem.AttackWidth, attackDataItem.AttackHeight));
                                    }

                                    tempLifetime--;
                                }
                                hitboxRects[i].RemoveAt(0);
                                hitboxRects[i].Add(new Rectangle(0, 0, 0, 0));

                                //adjust height and width to triple then multiply by scale
                                var xOriginAdjustment = spriteData.Sequence.xorigin * spriteFinalScale;
                                var yOriginAdjustment = spriteData.Sequence.yorigin * spriteFinalScale;

                                var xOffsetAdjusted = hitboxRects[i][currentFrame - 1].x * spriteFinalScale;
                                var yOffsetAdjusted = hitboxRects[i][currentFrame - 1].y * spriteFinalScale;

                                var xDrawPos = spriteDrawData.DrawOrigin.X + xOriginAdjustment;
                                var yDrawPos = spriteDrawData.DrawOrigin.Y + yOriginAdjustment;
                                var finalWidth = hitboxRects[i][currentFrame - 1].width * spriteFinalScale;
                                var finalHeight = hitboxRects[i][currentFrame - 1].height * spriteFinalScale;

                                yDrawPos -= yOffsetAdjusted;
                                xDrawPos += xOffsetAdjusted;

                                yDrawPos -= finalHeight; //because hitboxes are drawn upside-down for some reason in GMS?

                                if (boxDrawMode == BoxDrawMode.Hitbox) //hitboxes add a 0.5 magic number to them for some reason
                                {
                                    xDrawPos += (0.5f * spriteFinalScale);
                                }

                                var destRect = new Rectangle(
                                    xDrawPos,
                                    yDrawPos,
                                    finalWidth,
                                    finalHeight);

                                // draw the hitbox
                                Raylib.DrawRectangleLinesEx(destRect, 3.0f, hitboxDrawColor);
                            }
                        }
                        break;
                    case BoxDrawMode.None:
                        break;
                }
            }
        }

        private void RenderPaletteWindow(float scale)
        {
            //set the window size, scale as necessary
            var windowSize = new Vector2();
            windowSize.X = 320 * scale;
            windowSize.Y = 560 * scale;

            ImGui.SetNextWindowPos(new Vector2(10 * scale, 20 * scale));
            ImGui.SetNextWindowSize(windowSize);

            if (ImGui.Begin("Palette Editor", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.SetWindowFontScale(scale);

                var baseSelected = paletteInEditor == projectile.BaseColor;
                if (ImGui.Selectable("Base Palette##base-100", baseSelected))
                {
                    if (projectile.BaseColor == null)
                    {
                        projectile.BaseColor = new ProjectilePaletteModel();
                        projectile.BaseColor.Name = "Base Palette";
                    }

                    paletteInEditor = projectile.BaseColor;
                    editorMode = EditorMode.BasePalette;
                    ChangeRenderedPalette(null);
                }

                ImguiDrawingHelper.DrawVerticalSpacing(scale, 5.0f);

                //load each of the palettes in by name...
                if (ImGui.CollapsingHeader("Alternate Palettes", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    if (ImGui.Button("New Palette"))
                    {
                        var newPalette = new ProjectilePaletteModel();
                        projectile.Palettes.Add(newPalette);
                        paletteInEditor = newPalette;
                        editorMode = EditorMode.Palette;
                        ChangeRenderedPalette(newPalette);
                    }

                    ImguiDrawingHelper.DrawVerticalSpacing(scale, 5.0f);

                    for (int i = 0; i < projectile.Palettes.Count; i++)
                    {
                        var palette = projectile.Palettes[i];

                        var paletteSelected = paletteInEditor == palette;

                        if (ImguiDrawingHelper.DrawSelectableWithRemove(() =>
                        {
                            paletteInEditor = palette;
                            editorMode = EditorMode.Palette;
                            ChangeRenderedPalette(palette);
                        }, () =>
                        {
                            projectile.Palettes.Add(palette.GetDuplicate());
                        }, palette.Name, paletteSelected, i))
                        {
                            if (paletteInEditor == palette)
                            {
                                paletteInEditor = null;
                                editorMode = EditorMode.None;
                                ChangeRenderedPalette(null);
                            }

                            projectile.Palettes.RemoveAt(i);
                        }
                    }
                }

                ImGui.End();
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
                    paletteInEditor.ColorPalette = new List<ProjectileRGBModel>();
                }

                //if it's the base palette, this is can be changed... otherwise its the same as the base palette
                int numberOfReplacableColors = basePalette ? paletteInEditor.NumberOfReplacableColors : projectile.BaseColor.NumberOfReplacableColors;

                if (basePalette)
                {
                    ImguiDrawingHelper.DrawIntInput("numberOfPalettes", ref numberOfReplacableColors, 0, 20);
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
                        paletteInEditor.ColorPalette.Add(new ProjectileRGBModel());
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
            windowSize.X = 350 * scale;
            windowSize.Y = 220 * scale;

            float windowYPos = height - 8 - windowSize.Y;

            var windowPos = new Vector2(width / 2 - windowSize.X / 2, windowYPos);

            ImGui.SetNextWindowPos(windowPos);
            ImGui.SetNextWindowSize(windowSize);

            if (ImGui.Begin($"{editorWindowTitle}##EditorWindow", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.SetWindowFontScale(scale);

                switch (editorMode)
                {
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
                var cursorPos = new Vector2(0, spriteDrawPosition.Y + (screenManager.ScreenScale * 180));
                ImGui.SetCursorPos(cursorPos);

                var currentAnimationSpeedLabel =
                    currentAnimationSpeed == 0 ? (isMoveAnimationRender) ? "Set By Data" : "10 (default)" :
                    currentAnimationSpeed.ToString();

                ImGui.Text(" ");

                var moveInAmount = 35.0f;
                var curPos = ImGui.GetCursorPos();
                curPos.X += moveInAmount * scale;
                ImGui.SetCursorPos(curPos);

                var spriteName = spriteData != null ? spriteData.Name : "No Selected Sprite";

                ImGui.Text($"Current Sprite: {spriteName}");

                curPos = ImGui.GetCursorPos();
                curPos.X += moveInAmount * scale;
                ImGui.SetCursorPos(curPos);

                ImGui.Text($"Sprite Animation Speed (fps): {currentAnimationSpeedLabel}");

                curPos = ImGui.GetCursorPos();
                curPos.X += moveInAmount * scale;
                ImGui.SetCursorPos(curPos);

                ImGui.Text($"Current Move Frame: {currentFrame}");

                curPos = ImGui.GetCursorPos();
                curPos.X += moveInAmount * scale;
                ImGui.SetCursorPos(curPos);

                var spriteFramesCount = spriteData != null ? spriteData.Frames.Count : 0;

                ImGui.Text($"Total Sprite Frames: {spriteFramesCount}");

                var imageButtonSize = new Vector2((advanceOneFrameBackTexture.width / 2) * scale, (advanceOneFrameBackTexture.height / 2) * scale);

                cursorPos = new Vector2(((windowSize.X / 2) - imageButtonSize.X * 2) - buttonSpacing * 7.5f, (315 * scale) - imageButtonSize.Y);

                ImGui.SetCursorPos(cursorPos);

                if (showHitHurtboxes)
                {
                    if (ImGui.ImageButton("##ShowHitboxes", (IntPtr)showHitboxesTexture.id, imageButtonSize) || !showingMove)
                    {
                        boxDrawMode = BoxDrawMode.None;
                        showHitHurtboxes = false;
                    }
                }
                else
                {
                    if (ImGui.ImageButton("##HideHitboxes", (IntPtr)hideHitboxesTexture.id, imageButtonSize) && showingMove)
                    {
                        boxDrawMode = BoxDrawMode.Hitbox;
                        showHitHurtboxes = true;
                    }
                }

                ImGui.SameLine();

                if (ImGui.ImageButton("##ADVBack", (IntPtr)advanceOneFrameBackTexture.id, imageButtonSize))
                {
                    animationPaused = true;
                    frameAdvance = FrameAdvance.Backward;
                }

                ImGui.SameLine();

                if (animationPaused)
                {
                    if (ImGui.ImageButton("##Play", (IntPtr)playButtonTexture.id, imageButtonSize))
                    {
                        animationPaused = false;
                    }
                }
                else
                {
                    if (ImGui.ImageButton("##Pause", (IntPtr)pauseButtonTexture.id, imageButtonSize))
                    {
                        animationPaused = true;
                    }
                }

                ImGui.SameLine();

                if (ImGui.ImageButton("##ADVFwd", (IntPtr)advanceOneFrameForwardTexture.id, imageButtonSize))
                {
                    animationPaused = true;
                    frameAdvance = FrameAdvance.Forward;
                }

                ImGui.SameLine();

                if (playSound)
                {
                    if (ImGui.ImageButton("##PlaySound", (IntPtr)soundPlayTexture.id, imageButtonSize))
                    {
                        playSound = false;
                    }
                }
                else
                {
                    if (ImGui.ImageButton("##MuteSound", (IntPtr)soundMuteTexture.id, imageButtonSize))
                    {
                        playSound = true;
                    }
                }

                ImGui.End();
            }
        }

        private void RenderProjectileDataWindow(float scale)
        {
            var windowSize = new Vector2();
            windowSize.X = 320 * scale;
            windowSize.Y = 560 * scale;

            ImGui.SetNextWindowPos(new Vector2(width - windowSize.X - 10 * scale, 20 * scale));
            ImGui.SetNextWindowSize(windowSize);

            if (ImGui.Begin("Projectile Properties", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize))
            {
                int spriteIndex = 0; // This variable is used for changing the sprite back to the idle animation when deleting a move to prevent crashing

                ImGui.SetWindowFontScale(scale);
                if (ImGui.CollapsingHeader("Projectile Data", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    var projectileName = projectile.Name ?? string.Empty;

                    ImGui.BeginTable("projectileDataTable", 2);
                    var tableFlags = ImGuiTableColumnFlags.NoSort;
                    ImGui.TableSetupColumn("", tableFlags, ImGui.CalcTextSize("Projectile Name ").X);
                    ImGui.TableSetupColumn("", tableFlags | ImGuiTableColumnFlags.WidthStretch);

                    ImGui.TableNextColumn();

                    ImGui.Text("Projectile Name");
                    ImGui.TableNextColumn();
                    ImGui.InputText("##ProjectileName", ref projectileName, 50);

                    projectile.Name = projectileName;

                    ImGui.TableNextColumn();
                    ImGui.EndTable();

                    var horizontalSpeed = projectile.HorizontalSpeed;
                    var verticalSpeed = projectile.VerticalSpeed;
                    var envDisplacement = projectile.EnvironmentalDisplacement;
                    var fallSpeed = projectile.FallSpeed;
                    var groundTraction = projectile.GroundTraction;
                    var airTraction = projectile.AirTraction;
                    var destroyOnFloor = projectile.DestroyOnFloor;
                    var destroyOnWall = projectile.DestroyOnWall;
                    var bounceOnFloor = projectile.BounceOnFloor;
                    var bounceOnWall = projectile.BounceOnWall;
                    var numberOfBounces = projectile.NumberOfBounces;
                    var bounciness = projectile.Bounciness;
                    var transcendent = projectile.Transcendent;
                    var health = projectile.Health;
                    var spriteCollection = projectile.ProjectileSprites;

                    ImguiDrawingHelper.DrawDecimalInput("horizontalSpeed", ref horizontalSpeed);
                    ImguiDrawingHelper.DrawDecimalInput("verticalSpeed", ref verticalSpeed);
                    ImguiDrawingHelper.DrawIntInput("environmentalDisplacement", ref envDisplacement);
                    ImguiDrawingHelper.DrawDecimalInput("fallSpeed", ref fallSpeed);
                    ImguiDrawingHelper.DrawDecimalInput("groundTraction", ref groundTraction);
                    ImguiDrawingHelper.DrawDecimalInput("airTraction", ref airTraction);
                    ImguiDrawingHelper.DrawBoolInput("destroyOnFloor", ref destroyOnFloor);
                    ImguiDrawingHelper.DrawBoolInput("destroyOnWall", ref destroyOnWall);
                    if (!destroyOnFloor)
                    {
                        ImguiDrawingHelper.DrawBoolInput("bounceOnFloor", ref bounceOnFloor);
                    }
                    else
                    {
                        bounceOnFloor = false;
                    }
                    if (!destroyOnWall)
                    {
                        ImguiDrawingHelper.DrawBoolInput("bounceOnWall", ref bounceOnWall);
                    }
                    else
                    {
                        bounceOnWall = false;
                    }
                    if (bounceOnFloor || bounceOnWall)
                    {
                        ImguiDrawingHelper.DrawIntInput("numberOfBounces", ref numberOfBounces);
                        ImguiDrawingHelper.DrawDecimalInput("bounciness", ref bounciness);
                    }
                    else
                    {
                        numberOfBounces = 0;
                        bounciness = 0;
                    }
                    ImguiDrawingHelper.DrawBoolInput("transcendent", ref transcendent);
                    ImguiDrawingHelper.DrawIntInput("health", ref health, int.MinValue, null, "The number of hits a projectile takes before disappearing.");

                    projectile.HorizontalSpeed = horizontalSpeed;
                    projectile.VerticalSpeed = verticalSpeed;
                    projectile.EnvironmentalDisplacement = envDisplacement;
                    projectile.FallSpeed = fallSpeed;
                    projectile.GroundTraction = groundTraction;
                    projectile.AirTraction = airTraction;
                    projectile.DestroyOnFloor = destroyOnFloor;
                    projectile.DestroyOnWall = destroyOnWall;
                    projectile.BounceOnFloor = bounceOnFloor;
                    projectile.BounceOnWall = bounceOnWall;
                    projectile.NumberOfBounces = numberOfBounces;
                    projectile.Bounciness = bounciness;
                    projectile.Transcendent = transcendent;
                    projectile.Health = health;
                    projectile.ProjectileSprites = spriteCollection;
                }
                ImguiDrawingHelper.DrawVerticalSpacing(scale, 5.0f);

                if (ImGui.CollapsingHeader("Sprite Data", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    var Sprite = projectile.ProjectileSprites.Sprite;
                    var Impact = projectile.ProjectileSprites.Impact;

                    int spriteSelected = string.IsNullOrWhiteSpace(Sprite) ? -1 : allSprites.IndexOf(allSprites.First(x => x.Name == Sprite));
                    int impactSelected = string.IsNullOrWhiteSpace(Impact) ? -1 : allSprites.IndexOf(allSprites.First(x => x.Name == Impact));

                    var selectionAction = (int selectedIndex) =>
                    {
                        if (selectedIndex > -1)
                        {
                            var sprite = allSprites[selectedIndex];
                            ChangeAnimatedSprite(sprite);
                        }

                        if (selectedIndex == 0)
                        {
                            showingMove = true;
                        }
                        else
                        {
                            showingMove = false;
                        }
                        totalFrames = 0;
                        windows.Clear();
                    };

                    var changeAction = (int selectedIndex) =>
                    {
                        if (selectedIndex > -1 && spriteData != allSprites[selectedIndex])
                        {
                            var sprite = allSprites[selectedIndex];
                            ChangeAnimatedSprite(sprite);
                        }
                    };

                    string isPlaying(int index) =>
                        index != -1 && spriteData == allSprites[index] ? "*" : "";

                    ImguiDrawingHelper.DrawSelectableComboInput($"sprite{isPlaying(spriteSelected)}", allSprites.Select(x => x.Name).ToArray(), ref spriteSelected, selectionAction, changeAction);
                    ImguiDrawingHelper.DrawSelectableComboInput($"impact{isPlaying(impactSelected)}", allSprites.Select(x => x.Name).ToArray(), ref impactSelected, selectionAction, changeAction);

                    projectile.ProjectileSprites.Sprite = spriteSelected != -1 ? allSprites[spriteSelected].Name : string.Empty;
                    projectile.ProjectileSprites.Impact = impactSelected != -1 ? allSprites[impactSelected].Name : string.Empty;

                    spriteIndex = spriteSelected;
                }

                ImguiDrawingHelper.DrawVerticalSpacing(scale, 5.0f);

                // Hit Properties Dropdown
                if (ImGui.CollapsingHeader("Hit Properties"))
                {
                    int hitboxCount = projectile.NumberOfHitboxes;

                    ImguiDrawingHelper.DrawIntInput("numberOfHitboxes", ref hitboxCount);
                    hitboxRects.Clear();
                    for (int i = 0; i < hitboxCount; i++)
                    {
                        hitboxRects.Add(new List<Rectangle>());
                    }

                    if (hitboxCount < 0)
                    {
                        hitboxCount = 0;
                    }

                    if (hitboxCount < projectile.NumberOfHitboxes)
                    {
                        while (hitboxCount < projectile.NumberOfHitboxes)
                        {
                            projectile.CounterData.RemoveAt(projectile.NumberOfHitboxes - 1);
                            projectile.AttackData.RemoveAt(projectile.NumberOfHitboxes - 1);
                        }
                    }
                    else
                    {
                        while (hitboxCount > projectile.NumberOfHitboxes)
                        {
                            projectile.AttackData.Add(new ProjectileAttackDataModel());
                            projectile.CounterData.Add(new ProjectileCounterHitDataModel());
                        }
                    }

                    if (hitboxCount == 0)
                    {
                        ImGui.Text("No properties");
                    }
                    else
                    {
                        for (int i = 0; i < projectile.NumberOfHitboxes; i++)
                        {
                            var attackDataItem = projectile.AttackData[i];

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
                                float meterGain = attackDataItem.MeterGain;
                                float comboScaling = attackDataItem.ComboScaling;
                                int attackHitstop = attackDataItem.AttackHitStop;
                                int attackHitstun = attackDataItem.AttackHitStun;
                                AttackType attackType = attackDataItem.AttackType;
                                float blockStun = attackDataItem.BlockStun;
                                float knockBack = attackDataItem.KnockBack;
                                float airKnockbackH = attackDataItem.AirKnockbackHorizontal;
                                float airKnockbackV = attackDataItem.AirKnockbackVertical;
                                bool launches = attackDataItem.Launches;
                                float launchKnockbackV = attackDataItem.LaunchKnockbackVertical;
                                float launchKnockbackH = attackDataItem.LaunchKnockbackHorizontal;
                                float gravityScaling = attackDataItem.GravityScaling;
                                float pushback = attackDataItem.Pushback;
                                int particleOffsetX = attackDataItem.ParticleXOffset;
                                int particleOffsetY = attackDataItem.ParticleYOffset;
                                string particleEffect = attackDataItem.ParticleEffect;
                                int particleDuration = attackDataItem.ParticleDuration;
                                int holdOffsetX = attackDataItem.HoldXOffset;
                                int holdOffsetY = attackDataItem.HoldYOffset;
                                bool causesWallbounce = attackDataItem.CausesWallbounce;
                                string hitSound = attackDataItem.HitSound;

                                ImguiDrawingHelper.DrawIntInput("start", ref start);
                                ImguiDrawingHelper.DrawIntInput("lifetime", ref lifetime);
                                ImguiDrawingHelper.DrawBoolInput("causesWallbounce", ref causesWallbounce);
                                ImguiDrawingHelper.DrawIntInput("attackWidth", ref attackWidth);
                                ImguiDrawingHelper.DrawIntInput("attackHeight", ref attackHeight);
                                ImguiDrawingHelper.DrawIntInput("widthOffset", ref widthOffset);
                                ImguiDrawingHelper.DrawIntInput("heightOffset", ref heightOffset);
                                ImguiDrawingHelper.DrawIntInput("group", ref group);
                                ImguiDrawingHelper.DrawIntInput("damage", ref damage);
                                ImguiDrawingHelper.DrawDecimalInput("meterGain", ref meterGain);
                                ImguiDrawingHelper.DrawDecimalInput("comboScaling", ref comboScaling);
                                ImguiDrawingHelper.DrawIntInput("attackHitStop", ref attackHitstop);
                                ImguiDrawingHelper.DrawIntInput("attackHitStun", ref attackHitstun);

                                int selectedAttackType = (int)attackType;
                                ImguiDrawingHelper.DrawComboInput("attackType", attackTypesList.ToArray(), ref selectedAttackType);

                                ImguiDrawingHelper.DrawDecimalInput("blockStun", ref blockStun);
                                ImguiDrawingHelper.DrawDecimalInput("knockback", ref knockBack);
                                ImguiDrawingHelper.DrawDecimalInput("airKnockbackVertical", ref airKnockbackV);
                                ImguiDrawingHelper.DrawDecimalInput("airKnockbackHorizontal", ref airKnockbackH);
                                ImguiDrawingHelper.DrawBoolInput("launches", ref launches);
                                if (launches)
                                {
                                    ImguiDrawingHelper.DrawDecimalInput("launchKnockbackVertical", ref launchKnockbackV);
                                    ImguiDrawingHelper.DrawDecimalInput("launchKnockbackHorizontal", ref launchKnockbackH);
                                }
                                else
                                {
                                    launchKnockbackV = 0;
                                    launchKnockbackH = 0;
                                }
                                ImguiDrawingHelper.DrawDecimalInput("gravityScaling", ref gravityScaling);
                                ImguiDrawingHelper.DrawDecimalInput("pushback", ref pushback);
                                ImguiDrawingHelper.DrawIntInput("particleOffsetX", ref particleOffsetX);
                                ImguiDrawingHelper.DrawIntInput("particleOffsetY", ref particleOffsetY);

                                int selectedParticleEffect = string.IsNullOrWhiteSpace(particleEffect) ? 0 : allSprites.IndexOf(allSprites.Where(x => x.Name == particleEffect).First());
                                ImguiDrawingHelper.DrawComboInput("particleEffect", allSprites.Select(x => x.Name).ToArray(), ref selectedParticleEffect);

                                ImguiDrawingHelper.DrawIntInput("particleDuration", ref particleDuration);
                                ImguiDrawingHelper.DrawIntInput("holdOffsetX", ref holdOffsetX);
                                ImguiDrawingHelper.DrawIntInput("holdOffsetY", ref holdOffsetY);

                                var hitSoundId = attackDataItem.HitSound ?? string.Empty;
                                var selectedHitSoundIndex = hitSoundId != string.Empty ? allSounds.IndexOf(allSounds.First(x => x.Name == attackDataItem.HitSound)) : -1;
                                ImguiDrawingHelper.DrawComboInput("hitSoundEffect", allSounds.Select(x => x.Name).ToArray(), ref selectedHitSoundIndex);
                                hitSound = selectedHitSoundIndex != -1 ? allSounds[selectedHitSoundIndex].Name : string.Empty;

                                attackDataItem.Start = start;
                                attackDataItem.Lifetime = lifetime;
                                attackDataItem.AttackWidth = attackWidth;
                                attackDataItem.AttackHeight = attackHeight;
                                attackDataItem.WidthOffset = widthOffset;
                                attackDataItem.HeightOffset = heightOffset;
                                attackDataItem.Group = group;
                                attackDataItem.Damage = damage;
                                attackDataItem.MeterGain = meterGain;
                                attackDataItem.ComboScaling = comboScaling;
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
                                attackDataItem.GravityScaling = gravityScaling;
                                attackDataItem.Pushback = pushback;
                                attackDataItem.ParticleXOffset = particleOffsetX;
                                attackDataItem.ParticleYOffset = particleOffsetY;
                                attackDataItem.ParticleEffect = allSprites[selectedParticleEffect].Name;
                                attackDataItem.ParticleDuration = particleDuration;
                                attackDataItem.HoldXOffset = holdOffsetX;
                                attackDataItem.HoldYOffset = holdOffsetY;
                                attackDataItem.CausesWallbounce = causesWallbounce;
                                attackDataItem.HitSound = hitSound;

                                ImGui.TreePop();
                            }
                        }
                    }
                }

                ImguiDrawingHelper.DrawVerticalSpacing(scale, 5.0f);

                // Counter Hit Data dropdown
                if (ImGui.CollapsingHeader("Counter Hit Data"))
                {
                    if (projectile.NumberOfHitboxes == 0)
                    {
                        ImGui.Text("No properties");
                    }
                    else
                    {
                        for (int i = 0; i < projectile.NumberOfHitboxes; i++)
                        {
                            var currentCounterData = projectile.CounterData[i];

                            if (ImGui.TreeNode($"Counter Data [{i}]"))
                            {
                                int CounterHitLevel = currentCounterData.CounterHitLevel;
                                int Group = currentCounterData.Group;
                                int Damage = currentCounterData.Damage;
                                float MeterGain = currentCounterData.MeterGain;
                                float ComboScaling = currentCounterData.ComboScaling;
                                int AttackHitStop = currentCounterData.AttackHitStop;
                                int AttackHitStun = currentCounterData.AttackHitStun;
                                float KnockBack = currentCounterData.KnockBack;
                                float AirKnockbackVertical = currentCounterData.AirKnockbackVertical;
                                float AirKnockbackHorizontal = currentCounterData.AirKnockbackHorizontal;
                                bool Launches = currentCounterData.Launches;
                                float LaunchKnockbackVertical = currentCounterData.LaunchKnockbackVertical;
                                float LaunchKnockbackHorizontal = currentCounterData.LaunchKnockbackHorizontal;
                                float GravtyScaling = currentCounterData.GravityScaling;
                                float Pushback = currentCounterData.Pushback;
                                int ParticleXOffset = currentCounterData.ParticleXOffset;
                                int ParticleYOffset = currentCounterData.ParticleYOffset;
                                string ParticleEffect = currentCounterData.ParticleEffect;
                                int ParticleDuration = currentCounterData.ParticleDuration;
                                bool CausesWallbounce = currentCounterData.CausesWallbounce;
                                string HitSound = currentCounterData.HitSound;

                                ImguiDrawingHelper.DrawIntInput("counterHitLevel", ref CounterHitLevel);
                                ImguiDrawingHelper.DrawIntInput("group", ref Group);
                                ImguiDrawingHelper.DrawIntInput("damage", ref Damage);
                                ImguiDrawingHelper.DrawDecimalInput("meterGain", ref MeterGain);
                                ImguiDrawingHelper.DrawDecimalInput("comboScaling", ref ComboScaling);
                                ImguiDrawingHelper.DrawIntInput("attackHitStop", ref AttackHitStop);
                                ImguiDrawingHelper.DrawIntInput("attackHitStun", ref AttackHitStun);
                                ImguiDrawingHelper.DrawDecimalInput("knockback", ref KnockBack);
                                ImguiDrawingHelper.DrawDecimalInput("airKnockbackVertical", ref AirKnockbackVertical);
                                ImguiDrawingHelper.DrawDecimalInput("airKnockbackHorizontal", ref AirKnockbackHorizontal);
                                ImguiDrawingHelper.DrawBoolInput("launches", ref Launches);
                                if (Launches)
                                {
                                    ImguiDrawingHelper.DrawDecimalInput("launchKnockbackVertical", ref LaunchKnockbackVertical);
                                    ImguiDrawingHelper.DrawDecimalInput("launchKnockbackHorizontal", ref LaunchKnockbackHorizontal);
                                }
                                else
                                {
                                    LaunchKnockbackVertical = 0;
                                    LaunchKnockbackHorizontal = 0;
                                }
                                ImguiDrawingHelper.DrawDecimalInput("gravityScaling", ref GravtyScaling);
                                ImguiDrawingHelper.DrawDecimalInput("pushback", ref Pushback);
                                ImguiDrawingHelper.DrawIntInput("particleOffset X", ref ParticleXOffset);
                                ImguiDrawingHelper.DrawIntInput("particleOffset Y", ref ParticleYOffset);

                                int selectedParticleEffect = string.IsNullOrWhiteSpace(ParticleEffect) ? 0 : allSprites.IndexOf(allSprites.Where(x => x.Name == ParticleEffect).First());
                                ImguiDrawingHelper.DrawComboInput("particleEffect", allSprites.Select(x => x.Name).ToArray(), ref selectedParticleEffect);

                                ImguiDrawingHelper.DrawIntInput("particleDuration", ref ParticleDuration);
                                ImguiDrawingHelper.DrawBoolInput("causesWallbounce", ref CausesWallbounce);

                                var hitSoundId = currentCounterData.HitSound ?? string.Empty;
                                var selectedHitSoundIndex = hitSoundId != string.Empty ? allSounds.IndexOf(allSounds.First(x => x.Name == currentCounterData.HitSound)) : -1;
                                ImguiDrawingHelper.DrawComboInput("hitSoundEffect", allSounds.Select(x => x.Name).ToArray(), ref selectedHitSoundIndex);
                                HitSound = selectedHitSoundIndex != -1 ? allSounds[selectedHitSoundIndex].Name : string.Empty;

                                currentCounterData.CounterHitLevel = CounterHitLevel;
                                currentCounterData.Group = Group;
                                currentCounterData.Damage = Damage;
                                currentCounterData.MeterGain = MeterGain;
                                currentCounterData.ComboScaling = ComboScaling;
                                currentCounterData.AttackHitStop = AttackHitStop;
                                currentCounterData.AttackHitStun = AttackHitStun;
                                currentCounterData.KnockBack = KnockBack;
                                currentCounterData.AirKnockbackVertical = AirKnockbackVertical;
                                currentCounterData.AirKnockbackHorizontal = AirKnockbackHorizontal;
                                currentCounterData.Launches = Launches;
                                currentCounterData.LaunchKnockbackVertical = LaunchKnockbackVertical;
                                currentCounterData.LaunchKnockbackHorizontal = LaunchKnockbackHorizontal;
                                currentCounterData.GravityScaling = GravtyScaling;
                                currentCounterData.Pushback = Pushback;
                                currentCounterData.ParticleXOffset = ParticleXOffset;
                                currentCounterData.ParticleYOffset = ParticleYOffset;
                                currentCounterData.ParticleEffect = allSprites[selectedParticleEffect].Name;
                                currentCounterData.ParticleDuration = ParticleDuration;
                                currentCounterData.CausesWallbounce = CausesWallbounce;
                                currentCounterData.HitSound = HitSound;

                                ImGui.TreePop();
                            }
                            projectile.CounterData[i] = currentCounterData;
                        }
                    }
                }

                ImguiDrawingHelper.DrawVerticalSpacing(scale, 5.0f);

                // Rehit Data dropdown
                if (ImGui.CollapsingHeader("Rehit Data"))
                {
                    int rehitHitbox = projectile.RehitData.HitBox;
                    int numberOfRepeats = projectile.RehitData.NumberOfHits;

                    ImguiDrawingHelper.DrawIntInput("hitboxToRepeat", ref rehitHitbox, 0);
                    ImguiDrawingHelper.DrawIntInput("numberOfRepeats", ref numberOfRepeats, 0);

                    if (rehitHitbox < 1)
                    {
                        rehitHitbox = 1;
                    }
                    if (rehitHitbox > projectile.NumberOfHitboxes)
                    {
                        rehitHitbox = projectile.NumberOfHitboxes;
                    }
                    if (numberOfRepeats < 0)
                    {
                        numberOfRepeats = 0;
                    }

                    if (numberOfRepeats < projectile.RehitData.NumberOfHits)
                    {
                        while (numberOfRepeats < projectile.RehitData.NumberOfHits)
                        {
                            projectile.RehitData.HitOnFrames.RemoveAt(projectile.RehitData.NumberOfHits - 1);
                        }
                    }
                    else
                    {
                        while (numberOfRepeats > projectile.RehitData.NumberOfHits)
                        {
                            projectile.RehitData.HitOnFrames.Add(0);
                        }
                    }

                    if (numberOfRepeats == 0)
                    {
                        ImGui.Text("No Repeat Frame Information");
                    }
                    else
                    {
                        for (int i = 0; i < numberOfRepeats; i++)
                        {
                            var currentFrame = projectile.RehitData.HitOnFrames[i];

                            if (ImGui.TreeNode($"Hit On Frame [{i}]"))
                            {
                                ImguiDrawingHelper.DrawIntInput("repeatFrameIndex", ref currentFrame);

                                ImGui.TreePop();
                            }

                            projectile.RehitData.HitOnFrames[i] = currentFrame;
                        }
                    }

                    projectile.RehitData.HitBox = rehitHitbox;
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
                    //exports json to the projectiledata folder in the game
                    SaveProjectile();
                }

                if (ImGui.MenuItem("Export..."))
                {
                    //shows windows modal to allow saving json to anywhere
                    var saveFile = Win32DialogHelper.ShowSaveFileDialog(
                        "JSON File (*.json)\0*.json\0All Files (*.*)\0*.*\0",
                        "Save Projectile JSON Output",
                        projectData.ProjectPathOnly,
                        ".json");

                    if (saveFile != string.Empty)
                    {
                        //export json to the destination
                        _projectileOperations.SaveProjectile(projectile, projectData.ProjectPathOnly, saveFile);
                    }
                }

                ImGui.Separator();

                if (ImGui.MenuItem("Clear all data (reset)"))
                {
                    Init(new { width, height, projectData, action, projectile = new ProjectileDataModel() });
                }

                ImGui.Separator();

                if (ImGui.MenuItem("Close Projectile"))
                {
                    _logger.LogInformation("Project closed");
                    if (unsaved)
                    {
                        exitConfirmAction = (keycode) =>
                        {
                            if (keycode == (int)KeyboardKey.KEY_S)
                            {
                                SaveProjectile();
                                screenManager.NavigateTo(typeof(ProjectHomeScreen), new { height, width, projectData });
                            }
                            else if (keycode == (int)KeyboardKey.KEY_X)
                            {
                                screenManager.NavigateTo(typeof(ProjectHomeScreen), new { height, width, projectData });
                            }
                            else if (keycode == (int)KeyboardKey.KEY_C)
                            {
                                exiting = false;
                            }
                        };

                        exiting = true;
                    }
                    else
                    {
                        screenManager.NavigateTo(typeof(ProjectHomeScreen), new { height, width, projectData });
                    }
                }

                ImGui.Separator();

                if (ImGui.MenuItem("Close Project"))
                {
                    _logger.LogInformation("Close Project button clicked");
                    if (unsaved)
                    {
                        exitConfirmAction = (keycode) =>
                        {
                            if (keycode == (int)KeyboardKey.KEY_S)
                            {
                                SaveProjectile();
                                screenManager.NavigateTo(typeof(MainScreen), new { height, width });
                            }
                            else if (keycode == (int)KeyboardKey.KEY_X)
                            {
                                screenManager.NavigateTo(typeof(MainScreen), new { height, width });
                            }
                            else if (keycode == (int)KeyboardKey.KEY_C)
                            {
                                exiting = false;
                            }
                        };

                        exiting = true;
                    }
                    else
                    {
                        screenManager.NavigateTo(typeof(MainScreen), new { height, width });
                    }
                }

                ImGui.Separator();

                if (ImGui.MenuItem("Exit"))
                {
                    _logger.LogInformation("Exit Menu Item Clicked");

                    if (unsaved)
                    {
                        exitConfirmAction = (keycode) =>
                        {
                            if (keycode == (int)KeyboardKey.KEY_S)
                            {
                                SaveProjectile();
                                screenManager.ExitWindow = true;
                            }
                            else if (keycode == (int)KeyboardKey.KEY_X)
                            {
                                screenManager.ExitWindow = true;
                            }
                            else if (keycode == (int)KeyboardKey.KEY_C)
                            {
                                exiting = false;
                            }
                        };

                        exiting = true;
                    }
                    else
                    {
                        screenManager.ExitWindow = true;
                    }
                }

                ImGui.End();
            }

            ImGui.EndMainMenuBar();
        }

        public void CheckForExit(IScreenManager screenManager)
        {
            bool shouldClose = Raylib.WindowShouldClose();

            if (shouldClose && unsaved)
            {
                exitConfirmAction = (keycode) =>
                {
                    if (keycode == (int)KeyboardKey.KEY_S)
                    {
                        SaveProjectile();
                        screenManager.ExitWindow = true;
                    }
                    else if (keycode == (int)KeyboardKey.KEY_X)
                    {
                        screenManager.ExitWindow = true;
                    }
                    else if (keycode == (int)KeyboardKey.KEY_C)
                    {
                        exiting = false;
                    }
                };

                exiting = true;
            }
            else if (shouldClose)
            {
                screenManager.ExitWindow = true;
            }
        }

        private void SaveProjectile()
        {
            _projectileOperations.SaveProjectile(projectile, projectData.ProjectPathOnly);
            originalProjectile = projectile.Clone();
        }
    }
}
