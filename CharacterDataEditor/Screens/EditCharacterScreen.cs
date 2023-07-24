using CharacterDataEditor.Constants;
using CharacterDataEditor.Enums;
using CharacterDataEditor.Extensions;
using CharacterDataEditor.Helpers;
using CharacterDataEditor.Models;
using CharacterDataEditor.Models.CharacterData;
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

namespace CharacterDataEditor.Screens
{
    public class EditCharacterScreen : IScreen
    {
        private readonly ILogger<IScreen> _logger;
        private readonly ICharacterOperations _characterOperations;

        private float width;
        private float height;
        private CharacterDataModel character;
        private CharacterDataModel originalCharacter;
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

        private List<string> spiritDataList = new List<string>();
        private List<CharacterDataModel> spiritCharacters = new List<CharacterDataModel>();
        private List<string> spiritNames = new List<string>();
        private MoveDataModel moveInEditor;
        private PaletteModel paletteInEditor;
        private List<string> moveTypesList = new List<string>();
        private List<string> specialMoveTypesList = new List<string>();
        private List<string> positionTypesList = new List<string>();
        private List<string> allMoveTypesList = new List<string>(); // Used for selecing the move type, combines both moveType enums
        private int allMoveTypesListIndex;
        private List<string> attackTypesList = new List<string>();
        private List<string> spriteTypesList = new List<string>();
        private List<string> directionsList = new List<string>();
        private List<string> commandButtonsList = new List<string>();
        private List<SpriteDataModel> allSprites;
        private List<ScriptDataModel> allScripts;
        private List<SoundDataModel> allSounds;
        private List<ObjectDataModel> allProjectiles;

        private string spriteToDraw;
        private string prevSpriteToDraw;
        private SpriteDataModel spriteData;
        private SpriteDataModel moveSprite;
        private SpriteDataModel walkForwardSprite;
        private SpriteDataModel walkBackwardSprite;
        private SpriteDataModel runForwardSprite;
        private SpriteDataModel runBackwardSprite;
        private PaletteModel paletteData;
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
        private List<CachedSound> soundPlayers;
        private CachedSound footstepSoundPlayer;
        private List<CachedSound> hitSoundPlayers;

        private const int buttonSpacing = 8;

        private string editorWindowTitle;

        private bool isMoveAnimationRender;

        private delegate void AfterConfirmAction(int keyCode);
        private AfterConfirmAction exitConfirmAction;

        public EditCharacterScreen(ILogger<IScreen> logger, ICharacterOperations characterOperations)
        {
            _logger = logger;
            _characterOperations = characterOperations;
        }

        public void Init(dynamic screenData)
        {
            Raylib.SetWindowTitle(TitleConstants.EditCharacterTitle);

            spriteToDraw = string.Empty;
            prevSpriteToDraw = string.Empty;
            spriteData = null;
            moveSprite = null;
            walkForwardSprite = null;
            walkBackwardSprite = null;
            runForwardSprite = null;
            runBackwardSprite = null;
            editorMode = EditorMode.None;

            moveInEditor = null;
            paletteInEditor = null;
            unsaved = false;

            width = screenData?.width ?? 1280;
            height = screenData?.height ?? 720;
            projectData = screenData?.projectData ?? new RecentProjectModel();
            action = screenData?.action ?? "new";
            character = action == "edit" ? screenData.character : new CharacterDataModel();
            //copies the data from character into original character
            originalCharacter = character.Clone();

            // Add every character to this list
            spiritCharacters = _characterOperations.GetCharactersFromProject<CharacterDataModel>(projectData.ProjectPathOnly);
            List<CharacterDataModel> spiritsToRemove = new List<CharacterDataModel>();
            // Find which character need to be removed from this list
            foreach (var spiritCharacter in spiritCharacters)
            {
                if (spiritCharacter.UniqueData.SpiritData != SpiritDataType.IsSpirit)
                {
                    spiritsToRemove.Add(spiritCharacter);
                }
            }
            // Remove the unwanted characters
            foreach (var spiritToRemove in spiritsToRemove)
            {
                spiritCharacters.Remove(spiritToRemove);
            }
            // Add the names of the remaining characters to another list
            spiritNames.Clear();
            spiritNames.Add("None");
            foreach (var spiritCharacter in spiritCharacters)
            {
                spiritNames.Add(spiritCharacter.Name);
            }

            allSprites = _characterOperations.GetAllGameData<SpriteDataModel>(projectData.ProjectPathOnly);
            allScripts = _characterOperations.GetAllGameData<ScriptDataModel>(projectData.ProjectPathOnly);
            allSounds = _characterOperations.GetAllGameData<SoundDataModel>(projectData.ProjectPathOnly);
            allProjectiles = _characterOperations.GetAllGameData<ObjectDataModel>(projectData.ProjectPathOnly).Where(x => x.ContainerInfo?.ContainingFolder == "Projectiles").ToList();

            playSound = false;
            soundPlayers = new List<CachedSound>();
            hitSoundPlayers = new List<CachedSound>();

            if (action == "edit" && !string.IsNullOrWhiteSpace(character.CharacterSprites?.Idle))
            {
                var sprite = allSprites.FirstOrDefault(x => x.Name == character.CharacterSprites.Idle);
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

            var spiritDataTypes = Enum.GetValues(typeof(SpiritDataType));
            spiritDataList = new List<string>();
            var moveTypes = Enum.GetValues(typeof(MoveType));
            moveTypesList = new List<string>();
            var specialMoveTypes = Enum.GetValues(typeof(EnhanceMoveType));
            specialMoveTypesList = new List<string>();
            allMoveTypesList = new List<string>();
            var positionTypes = Enum.GetValues(typeof(PositionType));
            positionTypesList = new List<string>();

            foreach (SpiritDataType item in spiritDataTypes)
            {
                var itemAsString = item.ToString().AddSpacesToCamelCase();

                spiritDataList.Add(itemAsString);
            }

            foreach (MoveType item in moveTypes)
            {
                var itemAsString = item.ToString().AddSpacesToCamelCase();

                moveTypesList.Add(itemAsString);
            }
            foreach (EnhanceMoveType item in specialMoveTypes)
            {
                var itemAsString = item.ToString().AddSpacesToCamelCase();

                specialMoveTypesList.Add(itemAsString);
            }
            allMoveTypesList = moveTypesList.Concat(specialMoveTypesList).ToList();
            allMoveTypesList.RemoveAt(moveTypesList.Count);

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

            var spriteTypes = Enum.GetValues(typeof(SpriteType));
            spriteTypesList = new List<string>();

            foreach (SpriteType item in spriteTypes)
            {
                var itemAsString = item.ToString().AddSpacesToCamelCase();

                spriteTypesList.Add(itemAsString);
            }

            var directionTypes = Enum.GetValues(typeof(DirectionType));
            directionsList = new List<string>();

            foreach (DirectionType item in directionTypes)
            {
                var itemAsString = item.ToString().AddSpacesToCamelCase();

                directionsList.Add(itemAsString);
            }

            var commandButtonTypes = Enum.GetValues(typeof(CommandButton));
            commandButtonsList = new List<string>();

            foreach (CommandButton item in commandButtonTypes)
            {
                var itemAsString = item.ToString().AddSpacesToCamelCase();

                commandButtonsList.Add(itemAsString);
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
            RenderCharacterDataWindow(screenManager.ScreenScale);
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
                if (!character.Equals(originalCharacter))
                {
                    Raylib.SetWindowTitle($"{TitleConstants.EditCharacterTitle}{TitleConstants.UnsavedIndicator}");
                    unsaved = true;
                }
                else
                {
                    Raylib.SetWindowTitle(TitleConstants.EditCharacterTitle);
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

        private void ChangeRenderedPalette(PaletteModel palette)
        {
            paletteData = palette;
        }

        private void RenderAnimatedSprite(float scale)
        {
            var maxSpriteSize = new Vector2(200, 200);

            var animationFlags = SpriteDrawFlags.CenterHorizontal | SpriteDrawFlags.ShowSpriteOutline | SpriteDrawFlags.DrawOrigin;

            var useShader = character.BaseColor != null && paletteData != null;

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
                    BaseColor = character.BaseColor,
                    SwapColor = paletteData,
                    DrawPosition = spriteDrawPosition,
                    Scale = scale,
                    Logger = _logger,
                    MaxDrawSize = maxSpriteSize,
                    FrameAdvance = frameAdvance,
                    Flags = animationFlags
                }, showingMove, totalFrames, windows, resetAnimation);

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
                    BaseColor = character.BaseColor,
                    SwapColor = paletteData,
                    DrawPosition = spriteDrawPosition,
                    Scale = scale,
                    Logger = _logger,
                    MaxDrawSize = maxSpriteSize,
                    DefaultTexture = ResourceConstants.LogoPath,
                    Flags = animationFlags,
                    EnableFrameDataDraw = isMoveAnimationRender,
                    FrameDrawData = (isMoveAnimationRender) ? moveInEditor.FrameData : null
                }, showingMove, totalFrames, windows, resetAnimation);

                currentFrame = frameData.CurrentFrame;
                spriteDrawData = frameData;
                resetAnimation = false;
            }

            // Handle playing sound effect
            if (!animationPaused && playSound)
            {
                if (moveInEditor != null)
                {
                    for (int i = 0; i < moveInEditor.MoveSoundData.Count; i++)
                    {
                        if (spriteData == moveSprite && currentFrame == moveInEditor.MoveSoundData[i].SFXPlayFrame && moveInEditor.MoveSoundData[i].SoundEffect != string.Empty)
                        {
                            AudioPlaybackEngine.Instance.PlaySound(soundPlayers[i]);
                        }
                    }

                    for (int i = 0; i < hitSoundPlayers.Count; i++)
                    {
                        if (spriteData == moveSprite && currentFrame == moveInEditor.AttackData[i].Start)
                        {
                            AudioPlaybackEngine.Instance.PlaySound(hitSoundPlayers[i]);
                        }
                    }
                }
                
                if (spriteData == walkForwardSprite || spriteData == walkBackwardSprite || spriteData == runForwardSprite || spriteData == runBackwardSprite)
                {
                    if ((spriteData == walkForwardSprite && character.NonmoveSoundData.WalkForwardFootsteps.Contains(currentFrame) && character.NonmoveSoundData.WalkingSoundEffect != string.Empty) ||
                            (spriteData == walkBackwardSprite && character.NonmoveSoundData.WalkBackwardFootsteps.Contains(currentFrame) && character.NonmoveSoundData.WalkingSoundEffect != string.Empty) ||
                            (spriteData == runForwardSprite && character.NonmoveSoundData.RunForwardFootsteps.Contains(currentFrame) && character.NonmoveSoundData.RunningSoundEffect != string.Empty) ||
                            (spriteData == runBackwardSprite && character.NonmoveSoundData.RunBackwardFootsteps.Contains(currentFrame) && character.NonmoveSoundData.RunningSoundEffect != string.Empty))
                    {
                        AudioPlaybackEngine.Instance.PlaySound(footstepSoundPlayer);
                    }
                }
            }
        }

        private void RenderHitHurtBox(float scale)
        {
            Color hitboxDrawColor = Color.RED;
            Color hurtboxDrawColor = Color.BLUE;

            if (spriteData != null)
            {
                var spriteFinalScale = spriteDrawData.ScaledDrawSize.X / spriteData.Width;

                //draw the box
                switch (boxDrawMode)
                {
                    case BoxDrawMode.Both:
                        if (showingMove)
                        {
                            hurtboxRects.Clear();
                            for (int i = 0; i < moveInEditor.NumberOfHurtboxes; i++)
                            {
                                hurtboxRects.Add(new List<Rectangle>());

                                var attackDataItem = moveInEditor.HurtboxData[i];

                                hurtboxRects[i].Clear();
                                int tempLifetime = 0;
                                for (int j = 0; j <= totalFrames; j++)
                                {
                                    if (j == attackDataItem.Start)
                                    {
                                        tempLifetime = attackDataItem.Lifetime;
                                    }

                                    if (tempLifetime <= 0)
                                    {
                                        hurtboxRects[i].Add(new Rectangle(0, 0, 0, 0));
                                    }
                                    else
                                    {
                                        hurtboxRects[i].Add(new Rectangle(attackDataItem.WidthOffset, attackDataItem.HeightOffset, attackDataItem.AttackWidth, attackDataItem.AttackHeight));
                                    }

                                    tempLifetime--;
                                }
                                hurtboxRects[i].RemoveAt(0);
                                hurtboxRects[i].Add(new Rectangle(0, 0, 0, 0));

                                //adjust height and width to triple then multiply by scale
                                var xOriginAdjustment = spriteData.Sequence.xorigin * spriteFinalScale;
                                var yOriginAdjustment = spriteData.Sequence.yorigin * spriteFinalScale;

                                var xOffsetAdjusted = hurtboxRects[i][currentFrame - 1].x * spriteFinalScale;
                                var yOffsetAdjusted = hurtboxRects[i][currentFrame - 1].y * spriteFinalScale;

                                var xDrawPos = spriteDrawData.DrawOrigin.X + xOriginAdjustment;
                                var yDrawPos = spriteDrawData.DrawOrigin.Y + yOriginAdjustment;
                                var finalWidth = hurtboxRects[i][currentFrame - 1].width * spriteFinalScale;
                                var finalHeight = hurtboxRects[i][currentFrame - 1].height * spriteFinalScale;

                                yDrawPos -= yOffsetAdjusted;
                                xDrawPos += xOffsetAdjusted;

                                yDrawPos -= finalHeight; //because hitboxes are drawn upside-down for some reason in GMS?

                                var destRect = new Rectangle(
                                    xDrawPos,
                                    yDrawPos,
                                    finalWidth,
                                    finalHeight);

                                // draw the hurtbox
                                Raylib.DrawRectangleLinesEx(destRect, 3.0f, hurtboxDrawColor);
                            }

                            hitboxRects.Clear();
                            for (int i = 0; i < moveInEditor.NumberOfHitboxes; i++)
                            {
                                hitboxRects.Add(new List<Rectangle>());

                                var attackDataItem = moveInEditor.AttackData[i];

                                hitboxRects[i].Clear();
                                int tempLifetime = 0;
                                for (int j = 0; j <= totalFrames; j++)
                                {
                                    if (j == attackDataItem.Start)
                                    {
                                        tempLifetime = attackDataItem.Lifetime;
                                    }

                                    for (int k = 0; k < moveInEditor.RehitData.HitOnFrames.Count; k++)
                                    {
                                        if (j == moveInEditor.RehitData.HitOnFrames[k] && moveInEditor.RehitData.HitBox - 1 == i)
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

                var baseSelected = paletteInEditor == character.BaseColor;
                if (ImGui.Selectable("Base Palette##base-100", baseSelected))
                {
                    if (character.BaseColor == null)
                    {
                        character.BaseColor = new PaletteModel();
                        character.BaseColor.Name = "Base Palette";
                    }

                    paletteInEditor = character.BaseColor;
                    editorMode = EditorMode.BasePalette;
                    ChangeRenderedPalette(null);
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
                        editorMode = EditorMode.Palette;
                        ChangeRenderedPalette(newPalette);
                    }

                    ImguiDrawingHelper.DrawVerticalSpacing(scale, 5.0f);

                    for (int i = 0; i < character.Palettes.Count; i++)
                    {
                        var palette = character.Palettes[i];

                        var paletteSelected = paletteInEditor == palette;

                        if (ImguiDrawingHelper.DrawSelectableWithRemove(() =>
                            {
                                paletteInEditor = palette;
                                editorMode = EditorMode.Palette;
                                ChangeRenderedPalette(palette);
                            }, () =>
                            {
                                character.Palettes.Add(palette.GetDuplicate());
                            }, palette.Name, paletteSelected, i))
                        {
                            if (paletteInEditor == palette)
                            {
                                paletteInEditor = null;
                                editorMode = EditorMode.None;
                                ChangeRenderedPalette(null);
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
                // Select the move
                string[] allMoves = allMoveTypesList.ToArray();
                if (moveInEditor.EnhanceMoveType != EnhanceMoveType.None)
                {
                    var moveTypeName = moveInEditor.EnhanceMoveType.ToString();
                    allMoveTypesListIndex = specialMoveTypesList.IndexOf(moveTypeName.AddSpacesToCamelCase()) + moveTypesList.Count - 1;
                }
                else if (moveInEditor.MoveType != MoveType.None)
                {
                    var moveTypeName = moveInEditor.MoveType.ToString();
                    allMoveTypesListIndex = moveTypesList.IndexOf(moveTypeName.AddSpacesToCamelCase());
                }
                ImguiDrawingHelper.DrawComboInput("moveType", allMoves, ref allMoveTypesListIndex);
                var selectedMoveTypeName = allMoveTypesList[allMoveTypesListIndex];
                if (allMoveTypesListIndex < moveTypesList.Count)
                {
                    moveInEditor.MoveType = (MoveType)Enum.Parse(typeof(MoveType), selectedMoveTypeName.ToCamelCase());
                    moveInEditor.EnhanceMoveType = EnhanceMoveType.None;
                }
                else
                {
                    moveInEditor.EnhanceMoveType = (EnhanceMoveType)Enum.Parse(typeof(EnhanceMoveType), selectedMoveTypeName.ToCamelCase());
                    moveInEditor.MoveType = MoveType.None;
                }

                // Select what moves to cancel into
                if (moveInEditor.EnhanceMoveType == EnhanceMoveType.None && character.UniqueData.SpiritData != SpiritDataType.IsSpirit)
                {
                    var moveCancel = moveInEditor.MoveCanCancelInto;
                    ImguiDrawingHelper.DrawFlagsInputListbox("moveCancelsInto", ref moveCancel, scale);
                    moveInEditor.MoveCanCancelInto = moveCancel;
                }
                else
                {
                    moveInEditor.MoveCanCancelInto = 0;
                }

                // If the character has more than one moveset
                if (character.UniqueData.AdditionalMovesets > 0)
                {
                    ImGui.Columns(2);
                    ImGui.Text("Usable in Movesets:");
                    ImGui.Columns(1);

                    var inMovesets = moveInEditor.InMovesets;
                    var switchMoveset = moveInEditor.SwitchMoveset;
                    var switchToMoveset = moveInEditor.SwitchToMoveset;

                    // Shortens length of Movesets list
                    while (inMovesets.Count > character.UniqueData.AdditionalMovesets + 1)
                    {
                        inMovesets.RemoveAt(inMovesets.Count - 1);
                    }

                    // Removes movesets that are out of the range of possible movesets
                    int numberOfInvalidMovesets = 0;
                    List<int> invalidMovesets = new List<int>();
                    foreach (int moveset in inMovesets)
                    {
                        if (moveset > character.UniqueData.AdditionalMovesets + 1)
                        {
                            numberOfInvalidMovesets++;
                            invalidMovesets.Add(moveset);
                        }
                    }
                    while (numberOfInvalidMovesets > 0)
                    {
                        inMovesets.Remove(invalidMovesets[0]);
                        invalidMovesets.RemoveAt(0);
                        numberOfInvalidMovesets--;
                    }

                    // Failsafe to prevent the move from not being used in any movesets
                    if (inMovesets.Count <= 0)
                    {
                        inMovesets.Add(1);
                    }

                    // Create a list of checkboxes, one checkbox for each moveset
                    for (int i = 0; i <= character.UniqueData.AdditionalMovesets; i++)
                    {
                        var isInMoveset = false;
                        if (inMovesets.Contains(i + 1))
                        {
                            isInMoveset = true;
                        }

                        ImguiDrawingHelper.DrawBoolInput((i + 1).ToString(), ref isInMoveset);
                        if (isInMoveset && !inMovesets.Contains(i + 1))
                        {
                            inMovesets.Add(i + 1);
                        }
                        else if (!isInMoveset && inMovesets.Contains(i + 1))
                        {
                            inMovesets.Remove(i + 1);
                            if (inMovesets.Count <= 0)
                            {
                                inMovesets.Add(i + 1);
                            }
                        }
                    }
                    inMovesets.Sort();

                    ImguiDrawingHelper.DrawBoolInput("switchMoveset", ref switchMoveset);

                    if (switchMoveset)
                    {
                        ImguiDrawingHelper.DrawIntInput("switchToMoveset", ref switchToMoveset);
                        if (switchToMoveset < 1)
                        {
                            switchToMoveset = 1;
                        }
                        if (switchToMoveset > character.UniqueData.AdditionalMovesets + 1)
                        {
                            switchToMoveset = character.UniqueData.AdditionalMovesets + 1;
                        }
                    }
                    else
                    {
                        switchToMoveset = 0;
                    }

                    moveInEditor.InMovesets = inMovesets;
                    moveInEditor.SwitchMoveset = switchMoveset;
                    moveInEditor.SwitchToMoveset = switchToMoveset;
                }
                else
                {
                    moveInEditor.InMovesets.Clear();
                    moveInEditor.SwitchToMoveset = 0;
                }

                var spriteId = moveInEditor.SpriteName ?? string.Empty;
                var selectedSpriteIndex = spriteId != string.Empty ? allSprites.IndexOf(allSprites.First(x => x.Name == moveInEditor.SpriteName)) : -1;
                ImguiDrawingHelper.DrawComboInput("sprite", allSprites.Select(x => x.Name).ToArray(), ref selectedSpriteIndex);

                var scriptId = moveInEditor.SupplimentaryScript ?? string.Empty;
                var selectedScriptIndex = scriptId != string.Empty ? allScripts.IndexOf(allScripts.First(x => x.Name == moveInEditor.SupplimentaryScript)) : -1;
                ImguiDrawingHelper.DrawComboInput("supplementaryScript", allScripts.Select(x => x.Name).ToArray(), ref selectedScriptIndex);
                moveInEditor.SupplimentaryScript = selectedScriptIndex != -1 ? allScripts[selectedScriptIndex].Name : string.Empty;

                if (selectedSpriteIndex > -1)
                {
                    if (moveInEditor.SpriteName != allSprites[selectedSpriteIndex].Name)
                    {
                        moveInEditor.SpriteName = allSprites[selectedSpriteIndex].Name;
                        var sprite = allSprites[selectedSpriteIndex];
                        ChangeAnimatedSprite(sprite);
                    }
                }

                int moveDuration = moveInEditor.Duration;
                var adjustDuration = ImguiDrawingHelper.DrawIntInput("moveDuration", ref moveDuration, 0);
                moveInEditor.Duration = moveDuration;
                totalFrames = moveDuration;
                if (adjustDuration)
                {
                    ChangeWindowArray();
                    animationPaused = true;
                    boxDrawMode = BoxDrawMode.None;
                    showHitHurtboxes = false;
                }

                bool isThrow = moveInEditor.IsThrow;
                ImguiDrawingHelper.DrawBoolInput("isMoveAThrow?", ref isThrow);
                moveInEditor.IsThrow = isThrow;

                // Windows dropdown
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

                    // Fill windows list with animation frame indexes for each frame
                    ChangeWindowArray();
                }

                if (character.MoveData == null)
                {
                    character.MoveData = new List<MoveDataModel>();
                }

                // Move Sound Properties dropdown
                if (ImGui.CollapsingHeader("Sound Properties"))
                {
                    int soundCount = moveInEditor.NumberOfSounds;

                    ImguiDrawingHelper.DrawIntInput("numberOfSounds", ref soundCount, int.MinValue, null, "These are sounds that play as the move happens, not when the move hits the opponent.");

                    if (soundCount < 0)
                    {
                        soundCount = 0;
                    }

                    if (soundCount < moveInEditor.NumberOfSounds)
                    {
                        while (soundCount < moveInEditor.NumberOfSounds)
                        {
                            if (soundPlayers.Count > 0)
                            {
                                soundPlayers.RemoveAt(moveInEditor.NumberOfSounds - 1);
                            }
                            moveInEditor.MoveSoundData.RemoveAt(moveInEditor.NumberOfSounds - 1);
                        }
                    }
                    else
                    {
                        while (soundCount > moveInEditor.NumberOfSounds)
                        {
                            moveInEditor.MoveSoundData.Add(new MoveSoundDataModel());
                        }
                    }

                    if (soundCount == 0)
                    {
                        ImGui.Text("No sounds");
                    }
                    else
                    {
                        for (int i = 0; i < moveInEditor.MoveSoundData.Count; i++)
                        {
                            var moveSoundDataItem = moveInEditor.MoveSoundData[i];

                            if (ImGui.TreeNode($"Sound Effect [{i}]"))
                            {
                                var soundEffect = moveSoundDataItem.SoundEffect;
                                var sfxPlayFrame = moveSoundDataItem.SFXPlayFrame;

                                var soundId = soundEffect ?? string.Empty;
                                var selectedSoundIndex = soundId != string.Empty ? allSounds.IndexOf(allSounds.First(x => x.Name == soundEffect)) : -1;
                                ImguiDrawingHelper.DrawComboInput("soundEffect", allSounds.Select(x => x.Name).ToArray(), ref selectedSoundIndex);
                                soundEffect = selectedSoundIndex != -1 ? allSounds[selectedSoundIndex].Name : string.Empty;

                                ImguiDrawingHelper.DrawIntInput("soundPlayFrame", ref sfxPlayFrame);
                                if (sfxPlayFrame < 1)
                                {
                                    sfxPlayFrame = 1;
                                }
                                if (sfxPlayFrame > moveInEditor.Duration)
                                {
                                    sfxPlayFrame = moveInEditor.Duration;
                                }

                                moveSoundDataItem.SoundEffect = soundEffect;
                                moveSoundDataItem.SFXPlayFrame = sfxPlayFrame;

                                ImGui.TreePop();
                            }

                            if (moveSoundDataItem.SoundEffect != string.Empty &&
                                    spriteData != walkForwardSprite &&
                                    spriteData != walkBackwardSprite &&
                                    spriteData != runForwardSprite &&
                                    spriteData != runBackwardSprite)
                            {
                                string filePath = projectData.ProjectPathOnly + @"sounds\" + moveSoundDataItem.SoundEffect + @"\" + moveSoundDataItem.SoundEffect + ".wav";
                                if (!File.Exists(filePath))
                                {
                                    _logger.LogError($"File path {filePath} does not exist or is not accessable.");
                                    moveSoundDataItem.SoundEffect = "";
                                }
                                else
                                {
                                    if (soundPlayers.Count > i)
                                    {
                                        string currentPath = soundPlayers[i].filePath;
                                        if (currentPath != filePath)
                                        {
                                            soundPlayers[i] = new CachedSound(filePath);
                                        }
                                    }
                                    else
                                    {
                                        soundPlayers.Add(new CachedSound(filePath));
                                    }
                                }
                            }
                        }
                    }
                }

                // Attack Properties dropdown
                if (ImGui.CollapsingHeader("Attack Properties"))
                {
                    int hitboxCount = moveInEditor.NumberOfHitboxes;

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

                    if (hitboxCount < moveInEditor.NumberOfHitboxes)
                    {
                        while (hitboxCount < moveInEditor.NumberOfHitboxes)
                        {
                            moveInEditor.CounterData.RemoveAt(moveInEditor.NumberOfHitboxes - 1);
                            if (hitSoundPlayers.Count > 0)
                            {
                                hitSoundPlayers.RemoveAt(moveInEditor.NumberOfHitboxes - 1);
                            }
                            moveInEditor.AttackData.RemoveAt(moveInEditor.NumberOfHitboxes - 1);
                        }
                    }
                    else
                    {
                        while (hitboxCount > moveInEditor.NumberOfHitboxes)
                        {
                            moveInEditor.AttackData.Add(new AttackDataModel());
                            moveInEditor.CounterData.Add(new CounterHitDataModel());
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

                            if (attackDataItem.HitSound != string.Empty &&
                                    spriteData != walkForwardSprite &&
                                    spriteData != walkBackwardSprite &&
                                    spriteData != runForwardSprite &&
                                    spriteData != runBackwardSprite)
                            {
                                string filePath = projectData.ProjectPathOnly + @"sounds\" + attackDataItem.HitSound + @"\" + attackDataItem.HitSound + ".wav";
                                if (!File.Exists(filePath))
                                {
                                    _logger.LogError($"File path {filePath} does not exist or is not accessable.");
                                    attackDataItem.HitSound = "";
                                }
                                else
                                {
                                    if (hitSoundPlayers.Count > i)
                                    {
                                        string currentPath = hitSoundPlayers[i].filePath;
                                        if (currentPath != filePath)
                                        {
                                            hitSoundPlayers[i] = new CachedSound(filePath);
                                        }
                                    }
                                    else
                                    {
                                        hitSoundPlayers.Add(new CachedSound(filePath));
                                    }
                                }
                            }
                        }
                    }
                }

                // Counter Hit Data dropdown
                if (ImGui.CollapsingHeader("Counter Hit Data"))
                {
                    if (moveInEditor.NumberOfHitboxes == 0)
                    {
                        ImGui.Text("No properties");
                    }
                    else
                    {
                        for (int i = 0; i < moveInEditor.NumberOfHitboxes; i++)
                        {
                            var currentCounterData = moveInEditor.CounterData[i];

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
                            moveInEditor.CounterData[i] = currentCounterData;
                        }
                    }
                }

                // Command Normal dropdown
                if (ImGui.CollapsingHeader("Command Normal Data"))
                {
                    if ((moveInEditor.MoveType == MoveType.CommandNormal1 || moveInEditor.MoveType == MoveType.CommandNormal2 || moveInEditor.MoveType == MoveType.CommandNormal3) && 
                        character.UniqueData.SpiritData != SpiritDataType.IsSpirit)
                    {
                        DirectionType numpadDirection = moveInEditor.CommandNormalData.NumpadDirection;
                        CommandButton button = moveInEditor.CommandNormalData.Button;
                        bool groundOrAir = moveInEditor.CommandNormalData.GroundOrAir;
                        bool cancelWhenLanding = moveInEditor.CommandNormalData.CancelWhenLanding;

                        var directionName = numpadDirection.ToString();
                        int directionIndex = directionsList.IndexOf(directionName.AddSpacesToCamelCase());
                        ImguiDrawingHelper.DrawComboInput("direction", directionsList.ToArray(), ref directionIndex);
                        var selectedDirection = directionsList[directionIndex];
                        numpadDirection = (DirectionType)Enum.Parse(typeof(DirectionType), selectedDirection.ToCamelCase());

                        var buttonName = button.ToString();
                        int buttonIndex = commandButtonsList.IndexOf(buttonName.AddSpacesToCamelCase());
                        ImguiDrawingHelper.DrawComboInput("button", commandButtonsList.ToArray(), ref buttonIndex);
                        var selectedButton = commandButtonsList[buttonIndex];
                        button = (CommandButton)Enum.Parse(typeof(CommandButton), selectedButton.ToCamelCase());

                        ImguiDrawingHelper.DrawBoolInput("groundOrAir", ref groundOrAir, "Unchecked for ground, checked for air");

                        if (groundOrAir)
                        {
                            ImguiDrawingHelper.DrawBoolInput("cancelWhenLanding", ref cancelWhenLanding);
                        }
                        else
                        {
                            cancelWhenLanding = false;
                        }

                        moveInEditor.CommandNormalData.NumpadDirection = numpadDirection;
                        moveInEditor.CommandNormalData.Button = button;
                        moveInEditor.CommandNormalData.GroundOrAir = groundOrAir;
                        moveInEditor.CommandNormalData.CancelWhenLanding = cancelWhenLanding;
                    }
                    else if (character.UniqueData.SpiritData == SpiritDataType.IsSpirit)
                    {
                        ImGui.Text("Character is a spirit!");
                        moveInEditor.CommandNormalData.NumpadDirection = DirectionType.None;
                        moveInEditor.CommandNormalData.Button = CommandButton.Light;
                        moveInEditor.CommandNormalData.GroundOrAir = false;
                        moveInEditor.CommandNormalData.CancelWhenLanding = false;
                    }
                    else
                    {
                        ImGui.Text("Not a command normal!");
                        moveInEditor.CommandNormalData.NumpadDirection = DirectionType.None;
                        moveInEditor.CommandNormalData.Button = CommandButton.Light;
                        moveInEditor.CommandNormalData.GroundOrAir = false;
                        moveInEditor.CommandNormalData.CancelWhenLanding = false;
                    }
                }

                // Special Data dropdown
                if (ImGui.CollapsingHeader("Special Data"))
                {
                    if ((moveInEditor.MoveType == MoveType.NeutralSpecial || moveInEditor.MoveType == MoveType.SideSpecial ||
                        moveInEditor.MoveType == MoveType.UpSpecial || moveInEditor.MoveType == MoveType.DownSpecial || moveInEditor.EnhanceMoveType != EnhanceMoveType.None) && 
                        character.UniqueData.SpiritData != SpiritDataType.IsSpirit)
                    {
                        int enhancementCount = moveInEditor.NumberOfEnhancements;
                        ImguiDrawingHelper.DrawIntInput("numberOfEnhancements", ref enhancementCount, int.MinValue, null, "This can also be used for rekka follow-ups.");
                        if (enhancementCount < 0)
                        {
                            enhancementCount = 0;
                        }

                        if (enhancementCount < moveInEditor.NumberOfEnhancements)
                        {
                            while (enhancementCount < moveInEditor.NumberOfEnhancements)
                            {
                                moveInEditor.SpecialData.RemoveAt(moveInEditor.NumberOfEnhancements - 1);
                            }
                        }
                        else
                        {
                            while (enhancementCount > moveInEditor.NumberOfEnhancements)
                            {
                                moveInEditor.SpecialData.Add(new SpecialDataModel());
                            }
                        }

                        if (enhancementCount == 0)
                        {
                            ImGui.Text("No enhancements");
                        }
                        else
                        {
                            for (int i = 0; i < moveInEditor.NumberOfEnhancements; i++)
                            {
                                var specialDataItem = moveInEditor.SpecialData[i];

                                if (ImGui.TreeNode($"Enhancement [{i}]"))
                                {
                                    string numpadInput = specialDataItem.NumpadInput;
                                    bool buttonPressRequired = specialDataItem.ButtonPressRequired;
                                    int startingFrame = specialDataItem.StartingFrame;
                                    int endingFrame = specialDataItem.EndingFrame;
                                    EnhanceMoveType enhancementMove = specialDataItem.EnhancementMove;
                                    bool transitionImmediately = specialDataItem.TransitionImmediately;
                                    int transitionFrame = specialDataItem.TransitionFrame;
                                    PositionType requiredPosition = specialDataItem.RequiredPosition;
                                    bool deactivateSpirit = specialDataItem.DeactivateSpirit;

                                    ImguiDrawingHelper.DrawStringInput("numpadInput", ref numpadInput, 255, "For rekka follow-ups, you can also use single directions (like 8 or 2). Keep this value at 0 if no direction is required.");
                                    Regex regex = new Regex(@"[^\d]");
                                    numpadInput = regex.Replace(numpadInput, "");

                                    ImguiDrawingHelper.DrawBoolInput("buttonPressRequired", ref buttonPressRequired);
                                    ImguiDrawingHelper.DrawIntInput("startingFrame", ref startingFrame);
                                    ImguiDrawingHelper.DrawIntInput("endingFrame", ref endingFrame);

                                    if (character.UniqueData.SpiritData == SpiritDataType.HasSpirit)
                                    {
                                        ImguiDrawingHelper.DrawBoolInput("deactivateSpirit", ref deactivateSpirit);
                                        if (!deactivateSpirit)
                                        {
                                            var enhancementName = enhancementMove.ToString();
                                            int selectedEnhancementIndex = specialMoveTypesList.IndexOf(enhancementName.AddSpacesToCamelCase());
                                            ImguiDrawingHelper.DrawComboInput("enhancementMove", specialMoveTypesList.ToArray(), ref selectedEnhancementIndex);
                                            var selectedEnhancementName = specialMoveTypesList[selectedEnhancementIndex];
                                            enhancementMove = (EnhanceMoveType)Enum.Parse(typeof(EnhanceMoveType), selectedEnhancementName.ToCamelCase());

                                            ImguiDrawingHelper.DrawBoolInput("transitionImmediately", ref transitionImmediately);

                                            if (!transitionImmediately)
                                            {
                                                ImguiDrawingHelper.DrawIntInput("transitionFrame", ref transitionFrame);
                                            }
                                            else
                                            {
                                                transitionFrame = 0;
                                            }
                                        }
                                        else
                                        {
                                            enhancementMove = EnhanceMoveType.None;
                                            transitionImmediately = false;
                                            transitionFrame = 0;
                                        }
                                    }
                                    else
                                    {
                                        var enhancementName = enhancementMove.ToString();
                                        int selectedEnhancementIndex = specialMoveTypesList.IndexOf(enhancementName.AddSpacesToCamelCase());
                                        ImguiDrawingHelper.DrawComboInput("enhancementMove", specialMoveTypesList.ToArray(), ref selectedEnhancementIndex);
                                        var selectedEnhancementName = specialMoveTypesList[selectedEnhancementIndex];
                                        enhancementMove = (EnhanceMoveType)Enum.Parse(typeof(EnhanceMoveType), selectedEnhancementName.ToCamelCase());

                                        ImguiDrawingHelper.DrawBoolInput("transitionImmediately", ref transitionImmediately);

                                        if (!transitionImmediately)
                                        {
                                            ImguiDrawingHelper.DrawIntInput("transitionFrame", ref transitionFrame);
                                        }
                                        else
                                        {
                                            transitionFrame = 0;
                                        }

                                        deactivateSpirit = false;
                                    }

                                    var positionName = requiredPosition.ToString();
                                    int selectedPositionIndex = positionTypesList.IndexOf(positionName.AddSpacesToCamelCase());
                                    ImguiDrawingHelper.DrawComboInput("requiredPosition", positionTypesList.ToArray(), ref selectedPositionIndex);
                                    var selectedPositionName = positionTypesList[selectedPositionIndex];
                                    requiredPosition = (PositionType)Enum.Parse(typeof(PositionType), selectedPositionName.ToCamelCase());

                                    specialDataItem.NumpadInput = numpadInput;
                                    specialDataItem.ButtonPressRequired = buttonPressRequired;
                                    specialDataItem.StartingFrame = startingFrame;
                                    specialDataItem.EndingFrame = endingFrame;
                                    specialDataItem.EnhancementMove = enhancementMove;
                                    specialDataItem.TransitionImmediately = transitionImmediately;
                                    specialDataItem.TransitionFrame = transitionFrame;
                                    specialDataItem.RequiredPosition = requiredPosition;
                                    specialDataItem.DeactivateSpirit = deactivateSpirit;

                                    ImGui.TreePop();
                                }
                            }
                        }
                    }
                    else if (character.UniqueData.SpiritData == SpiritDataType.IsSpirit)
                    {
                        ImGui.Text("Character is a spirit! Handle this data in host character.");
                        moveInEditor.SpecialData.Clear();
                    }
                    else
                    {
                        ImGui.Text("Not a special move!");
                        moveInEditor.SpecialData.Clear();
                    }
                }

                // Rehit Data dropdown
                if (ImGui.CollapsingHeader("Rehit Data"))
                {
                    int rehitHitbox = moveInEditor.RehitData.HitBox;
                    int numberOfRepeats = moveInEditor.RehitData.NumberOfHits;

                    ImguiDrawingHelper.DrawIntInput("hitboxToRepeat", ref rehitHitbox, 0);
                    ImguiDrawingHelper.DrawIntInput("numberOfRepeats", ref numberOfRepeats, 0);

                    if (rehitHitbox < 1)
                    {
                        rehitHitbox = 1;
                    }
                    if (rehitHitbox > moveInEditor.NumberOfHitboxes)
                    {
                        rehitHitbox = moveInEditor.NumberOfHitboxes;
                    }
                    if (numberOfRepeats < 0)
                    {
                        numberOfRepeats = 0;
                    }

                    if (numberOfRepeats < moveInEditor.RehitData.NumberOfHits)
                    {
                        while (numberOfRepeats < moveInEditor.RehitData.NumberOfHits)
                        {
                            moveInEditor.RehitData.HitOnFrames.RemoveAt(moveInEditor.RehitData.NumberOfHits - 1);
                        }
                    }
                    else
                    {
                        while (numberOfRepeats > moveInEditor.RehitData.NumberOfHits)
                        {
                            moveInEditor.RehitData.HitOnFrames.Add(0);
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
                            var currentFrame = moveInEditor.RehitData.HitOnFrames[i];

                            if (ImGui.TreeNode($"Hit On Frame [{i}]"))
                            {
                                ImguiDrawingHelper.DrawIntInput("repeatFrameIndex", ref currentFrame);

                                ImGui.TreePop();
                            }

                            moveInEditor.RehitData.HitOnFrames[i] = currentFrame;
                        }
                    }

                    moveInEditor.RehitData.HitBox = rehitHitbox;
                }

                // Opponent Position Data dropdown
                if (isThrow && ImGui.CollapsingHeader("Opponent Position Data"))
                {
                    float distanceFromWall = moveInEditor.OpponentPositionData.DistanceFromWall;
                    int numberOfFrames = moveInEditor.OpponentPositionData.NumberOfFrames;
                    int throwOffset = moveInEditor.OpponentPositionData.ThrowOffset;

                    ImguiDrawingHelper.DrawDecimalInput("distanceFromWall", ref distanceFromWall);
                    ImguiDrawingHelper.DrawIntInput("throwOffset", ref throwOffset);
                    ImguiDrawingHelper.DrawIntInput("numberOfFrames", ref numberOfFrames);

                    moveInEditor.OpponentPositionData.DistanceFromWall = distanceFromWall;
                    moveInEditor.OpponentPositionData.ThrowOffset = throwOffset;

                    if (numberOfFrames < 0)
                    {
                        numberOfFrames = 0;
                    }

                    if (numberOfFrames < moveInEditor.OpponentPositionData.NumberOfFrames)
                    {
                        while (numberOfFrames < moveInEditor.OpponentPositionData.NumberOfFrames)
                        {
                            moveInEditor.OpponentPositionData.Frames.RemoveAt(moveInEditor.OpponentPositionData.NumberOfFrames - 1);
                        }
                    }
                    else
                    {
                        while (numberOfFrames > moveInEditor.OpponentPositionData.NumberOfFrames)
                        {
                            moveInEditor.OpponentPositionData.Frames.Add(new OpponentPositionFrameModel());
                        }
                    }

                    if (numberOfFrames == 0)
                    {
                        ImGui.Text("No Frame Data");
                    }
                    else
                    {
                        for (int i = 0; i < numberOfFrames; i++)
                        {
                            var currentFrame = moveInEditor.OpponentPositionData.Frames[i];

                            if (ImGui.TreeNode($"Frame [{i}]"))
                            {
                                int frameNumber = currentFrame.Frame;
                                int relativeXPos = currentFrame.RelativeX;
                                int relativeYPos = currentFrame.RelativeY;
                                int frameIndex = currentFrame.Index;
                                int rotation = currentFrame.Rotation;
                                int xScale = currentFrame.XScale;
                                int spriteToUse = (int)currentFrame.Sprite;

                                ImguiDrawingHelper.DrawIntInput("frame", ref frameNumber);
                                ImguiDrawingHelper.DrawIntInput("relativeXPosition", ref relativeXPos);
                                ImguiDrawingHelper.DrawIntInput("relativeYPosition", ref relativeYPos);
                                ImguiDrawingHelper.DrawComboInput("spriteToUse", spriteTypesList.ToArray(), ref spriteToUse);
                                ImguiDrawingHelper.DrawIntInput("index", ref frameIndex);
                                ImguiDrawingHelper.DrawIntInput("rotation", ref rotation);
                                ImguiDrawingHelper.DrawIntInput("xScale", ref xScale);

                                currentFrame.Frame = frameNumber;
                                currentFrame.RelativeY = relativeYPos;
                                currentFrame.RelativeX = relativeXPos;
                                currentFrame.Index = frameIndex;
                                currentFrame.Rotation = rotation;
                                currentFrame.XScale = xScale;
                                currentFrame.Sprite = (SpriteType)spriteToUse;

                                ImGui.TreePop();
                            }

                            moveInEditor.OpponentPositionData.Frames[i] = currentFrame;
                        }
                    }
                }

                // Projectile Data dropdown
                if (!isThrow && ImGui.CollapsingHeader("Projectile Data"))
                {
                    int projectileCount = moveInEditor.NumberOfProjectiles;

                    ImguiDrawingHelper.DrawIntInput("numberOfProjectiles", ref projectileCount);

                    if (projectileCount < 0)
                    {
                        projectileCount = 0;
                    }

                    if (projectileCount < moveInEditor.NumberOfProjectiles)
                    {
                        while (projectileCount < moveInEditor.NumberOfProjectiles)
                        {
                            moveInEditor.ProjectileData.RemoveAt(moveInEditor.NumberOfProjectiles - 1);
                        }
                    }
                    else
                    {
                        while (projectileCount > moveInEditor.NumberOfProjectiles)
                        {
                            moveInEditor.ProjectileData.Add(new CharacterProjectileDataModel());
                        }
                    }

                    if (projectileCount == 0)
                    {
                        ImGui.Text("No projectiles");
                    }
                    else
                    {
                        for (int i = 0; i < moveInEditor.NumberOfProjectiles; i++)
                        {
                            var currentProjectile = moveInEditor.ProjectileData[i];

                            if (ImGui.TreeNode($"Projectile [{i}]"))
                            {
                                int spawnX = currentProjectile.SpawnXOffset;
                                int spawnY = currentProjectile.SpawnYOffset;
                                int spawnFrame = currentProjectile.SpawnFrame;

                                var objId = currentProjectile.ProjectileObject ?? string.Empty;
                                var selectedObjectIndex = objId != string.Empty ? allProjectiles.IndexOf(allProjectiles.First(x => x.Name == objId)) : -1;
                                ImguiDrawingHelper.DrawComboInput("projectileObject", allProjectiles.Select(x => x.Name).ToArray(), ref selectedObjectIndex);
                                currentProjectile.ProjectileObject = selectedObjectIndex != -1 ? allProjectiles[selectedObjectIndex].Name : string.Empty;

                                ImguiDrawingHelper.DrawIntInput("spawnFrame", ref spawnFrame, 0);
                                ImguiDrawingHelper.DrawIntInput("spawnOffsetX", ref spawnX);
                                ImguiDrawingHelper.DrawIntInput("spawnOffsetY", ref spawnY);

                                currentProjectile.SpawnXOffset = spawnX;
                                currentProjectile.SpawnYOffset = spawnY;
                                currentProjectile.SpawnFrame = spawnFrame;

                                ImGui.TreePop();
                            }
                        }
                    }
                }

                // Hurtboxes dropdown
                if (ImGui.CollapsingHeader("Hurtboxes"))
                {
                    int hurtboxCount = moveInEditor.NumberOfHurtboxes;

                    ImguiDrawingHelper.DrawIntInput("numberOfHurtboxes", ref hurtboxCount);
                    hurtboxRects.Clear();
                    for (int i = 0; i < hurtboxCount; i++)
                    {
                        hurtboxRects.Add(new List<Rectangle>());
                    }

                    if (hurtboxCount < 0)
                    {
                        hurtboxCount = 0;
                    }

                    if (hurtboxCount < moveInEditor.NumberOfHurtboxes)
                    {
                        while (hurtboxCount < moveInEditor.NumberOfHurtboxes)
                        {
                            moveInEditor.HurtboxData.RemoveAt(moveInEditor.NumberOfHurtboxes - 1);
                            hurtboxRects.RemoveAt(moveInEditor.NumberOfHurtboxes - 1);
                        }
                    }
                    else
                    {
                        while (hurtboxCount > moveInEditor.NumberOfHurtboxes)
                        {
                            moveInEditor.HurtboxData.Add(new HurtboxDataModel());
                            hurtboxRects.Add(new List<Rectangle>());
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

                // Air Movement Data dropdown
                if (ImGui.CollapsingHeader("Air Movement Data"))
                {
                    int movementDataCount = moveInEditor.AirMovementData.NumberOfWindows;
                    float gravityScale = moveInEditor.AirMovementData.GravityScale;
                    float fallScale = moveInEditor.AirMovementData.FallScale;

                    ImguiDrawingHelper.DrawDecimalInput("gravityScale", ref gravityScale);
                    ImguiDrawingHelper.DrawDecimalInput("fallScale", ref fallScale);

                    moveInEditor.AirMovementData.FallScale = fallScale;
                    moveInEditor.AirMovementData.GravityScale = gravityScale;

                    ImguiDrawingHelper.DrawIntInput("numberOfMovementDataFrames", ref movementDataCount);

                    if (movementDataCount < 0)
                    {
                        movementDataCount = 0;
                    }

                    if (movementDataCount < moveInEditor.AirMovementData.NumberOfWindows)
                    {
                        while (movementDataCount < moveInEditor.AirMovementData.NumberOfWindows)
                        {
                            moveInEditor.AirMovementData.Windows.RemoveAt(moveInEditor.AirMovementData.NumberOfWindows - 1);
                        }
                    }
                    else
                    {
                        while (movementDataCount > moveInEditor.AirMovementData.NumberOfWindows)
                        {
                            moveInEditor.AirMovementData.Windows.Add(new MovementDataModel());
                        }
                    }

                    if (movementDataCount == 0)
                    {
                        ImGui.Text("No air movement data frames");
                    }
                    else
                    {
                        for (int i = 0; i < moveInEditor.AirMovementData.NumberOfWindows; i++)
                        {
                            if (ImGui.TreeNode($"Air Move Window [{i}]"))
                            {
                                var currentMovementData = moveInEditor.AirMovementData.Windows[i];

                                int startFrame = currentMovementData.StartingFrame;
                                float horizontalSpeed = currentMovementData.HorizontalSpeed;
                                float verticalSpeed = currentMovementData.VerticalSpeed;
                                bool overwriteVSpeed = currentMovementData.OverwriteVerticalSpeed;
                                bool overwriteHSpeed = currentMovementData.OverwriteHorizontalSpeed;

                                ImguiDrawingHelper.DrawIntInput("startingFrame", ref startFrame);
                                ImguiDrawingHelper.DrawDecimalInput("horizontalSpeed", ref horizontalSpeed);
                                ImguiDrawingHelper.DrawDecimalInput("verticalSpeed", ref verticalSpeed);
                                ImguiDrawingHelper.DrawBoolInput("overwriteHorizontalSpeed", ref overwriteHSpeed);
                                ImguiDrawingHelper.DrawBoolInput("overwriteVerticalSpeed", ref overwriteVSpeed);

                                currentMovementData.StartingFrame = startFrame;
                                currentMovementData.HorizontalSpeed = horizontalSpeed;
                                currentMovementData.VerticalSpeed = verticalSpeed;
                                currentMovementData.OverwriteVerticalSpeed = overwriteVSpeed;
                                currentMovementData.OverwriteHorizontalSpeed = overwriteHSpeed;

                                ImGui.TreePop();
                            }
                        }
                    }
                }

                // Ground Movement Data dropdown
                if (ImGui.CollapsingHeader("Ground Movement Data"))
                {
                    int movementDataCount = moveInEditor.GroundMovementData.NumberOfWindows;
                    float gravityScale = moveInEditor.GroundMovementData.GravityScale;
                    float fallScale = moveInEditor.GroundMovementData.FallScale;

                    ImguiDrawingHelper.DrawDecimalInput("gravityScale", ref gravityScale);
                    ImguiDrawingHelper.DrawDecimalInput("fallScale", ref fallScale);

                    moveInEditor.GroundMovementData.FallScale = fallScale;
                    moveInEditor.GroundMovementData.GravityScale = gravityScale;

                    ImguiDrawingHelper.DrawIntInput("numberOfMovementDataFrames", ref movementDataCount);

                    if (movementDataCount < 0)
                    {
                        movementDataCount = 0;
                    }

                    if (movementDataCount < moveInEditor.GroundMovementData.NumberOfWindows)
                    {
                        while (movementDataCount < moveInEditor.GroundMovementData.NumberOfWindows)
                        {
                            moveInEditor.GroundMovementData.Windows.RemoveAt(moveInEditor.GroundMovementData.NumberOfWindows - 1);
                        }
                    }
                    else
                    {
                        while (movementDataCount > moveInEditor.GroundMovementData.NumberOfWindows)
                        {
                            moveInEditor.GroundMovementData.Windows.Add(new MovementDataModel());
                        }
                    }

                    if (movementDataCount == 0)
                    {
                        ImGui.Text("No ground movement data frames");
                    }
                    else
                    {
                        for (int i = 0; i < moveInEditor.GroundMovementData.NumberOfWindows; i++)
                        {
                            if (ImGui.TreeNode($"Ground Move Window [{i}]"))
                            {
                                var currentMovementData = moveInEditor.GroundMovementData.Windows[i];

                                int startFrame = currentMovementData.StartingFrame;
                                float horizontalSpeed = currentMovementData.HorizontalSpeed;
                                float verticalSpeed = currentMovementData.VerticalSpeed;
                                bool overwriteVSpeed = currentMovementData.OverwriteVerticalSpeed;
                                bool overwriteHSpeed = currentMovementData.OverwriteHorizontalSpeed;

                                ImguiDrawingHelper.DrawIntInput("startingFrame", ref startFrame);
                                ImguiDrawingHelper.DrawDecimalInput("horizontalSpeed", ref horizontalSpeed);
                                ImguiDrawingHelper.DrawDecimalInput("verticalSpeed", ref verticalSpeed);
                                ImguiDrawingHelper.DrawBoolInput("overwriteHorizontalSpeed", ref overwriteHSpeed);
                                ImguiDrawingHelper.DrawBoolInput("overwriteVerticalSpeed", ref overwriteVSpeed);

                                currentMovementData.StartingFrame = startFrame;
                                currentMovementData.HorizontalSpeed = horizontalSpeed;
                                currentMovementData.VerticalSpeed = verticalSpeed;
                                currentMovementData.OverwriteVerticalSpeed = overwriteVSpeed;
                                currentMovementData.OverwriteHorizontalSpeed = overwriteHSpeed;

                                ImGui.TreePop();
                            }
                        }
                    }
                }

                // Spirit Data dropdown
                if (ImGui.CollapsingHeader("Spirit Data"))
                {
                    if (character.UniqueData.SpiritData == SpiritDataType.HasSpirit)
                    {
                        bool toggleState = moveInEditor.SpiritData.ToggleState;
                        bool performAttack = moveInEditor.SpiritData.PerformAttack;
                        bool performInSpiritOff = moveInEditor.SpiritData.PerformInSpiritOff;
                        bool returnToPlayer = moveInEditor.SpiritData.ReturnToPlayer;

                        ImguiDrawingHelper.DrawBoolInput("toggleState", ref toggleState, "Activate Spirit ON/OFF.");
                        ImguiDrawingHelper.DrawBoolInput("performCorrespondingAttack", ref performAttack);

                        if (performAttack)
                        {
                            ImguiDrawingHelper.DrawBoolInput("performInSpiritOff", ref performInSpiritOff, "If set to true, the spirit will temporarily be summoned in Spirit OFF to perform this move. The spirit will then immediately disappear when the move ends.");
                        }
                        else
                        {
                            performInSpiritOff = false;
                        }

                        if (performAttack)
                        {
                            int startXOffset = moveInEditor.SpiritData.StartXOffset;
                            int startYOffset = moveInEditor.SpiritData.StartYOffset;
                            bool summonSpirit = moveInEditor.SpiritData.SummonSpirit;

                            ImguiDrawingHelper.DrawIntInput("startPositionOffsetX", ref startXOffset, int.MinValue, null, "Sets the X Offset from the host if the spirit is currently standing by the host's side or when temporarily summoned in Spirit OFF.");
                            ImguiDrawingHelper.DrawIntInput("startPositionOffsetY", ref startYOffset, int.MinValue, null, "Sets the Y Offset from the host if the spirit is currently standing by the host's side or when temporarily summoned in Spirit OFF.");
                            ImguiDrawingHelper.DrawBoolInput("summonSpirit", ref summonSpirit, "Enters Spirit ON and keeps the spirit out after the move ends.");

                            moveInEditor.SpiritData.StartXOffset = startXOffset;
                            moveInEditor.SpiritData.StartYOffset = startYOffset;
                            moveInEditor.SpiritData.SummonSpirit = summonSpirit;
                        }
                        else
                        {
                            moveInEditor.SpiritData.StartXOffset = 0;
                            moveInEditor.SpiritData.StartYOffset = 0;
                            moveInEditor.SpiritData.SummonSpirit = false;
                            moveInEditor.SpiritData.MaintainPosition = false;
                        }

                        ImguiDrawingHelper.DrawBoolInput("returnToHost", ref returnToPlayer);

                        moveInEditor.SpiritData.ToggleState = toggleState;
                        moveInEditor.SpiritData.PerformAttack = performAttack;
                        moveInEditor.SpiritData.PerformInSpiritOff = performInSpiritOff;
                        moveInEditor.SpiritData.ReturnToPlayer = returnToPlayer;

                        moveInEditor.SpiritData.MaintainPosition = false;
                        moveInEditor.SpiritData.Vulnerable = false;
                        moveInEditor.SpiritData.OnlyInSpiritOff = false;
                    }
                    else if (character.UniqueData.SpiritData == SpiritDataType.IsSpirit)
                    {
                        bool maintainPosition = moveInEditor.SpiritData.MaintainPosition;
                        bool vulnerable = moveInEditor.SpiritData.Vulnerable;
                        bool onlyInSpiritOff = moveInEditor.SpiritData.OnlyInSpiritOff;

                        ImguiDrawingHelper.DrawBoolInput("maintainPosition", ref maintainPosition, "If true, then after the spirit finishes the attack, it stays at its current location and will follow the host's movements.");
                        ImguiDrawingHelper.DrawBoolInput("vulnerableAfterAttack", ref vulnerable, "If the spirit gets hit while performing this move, it instantly gets KO'd.");
                        ImguiDrawingHelper.DrawBoolInput("onlyInSpiritOff", ref onlyInSpiritOff, "This move will be performed when summoned with the corresponding move in Spirit OFF.");

                        moveInEditor.SpiritData.MaintainPosition = maintainPosition;
                        moveInEditor.SpiritData.Vulnerable = vulnerable;
                        moveInEditor.SpiritData.OnlyInSpiritOff = onlyInSpiritOff;

                        moveInEditor.SpiritData.ToggleState = false;
                        moveInEditor.SpiritData.PerformAttack = false;
                        moveInEditor.SpiritData.PerformInSpiritOff = false;
                        moveInEditor.SpiritData.StartXOffset = 0;
                        moveInEditor.SpiritData.StartYOffset = 0;
                        moveInEditor.SpiritData.SummonSpirit = false;
                        moveInEditor.SpiritData.ReturnToPlayer = false;
                    }
                    else
                    {
                        ImGui.Text("Character doesn't have a spirit and isn't a spirit!");
                        moveInEditor.SpiritData.ToggleState = false;
                        moveInEditor.SpiritData.PerformAttack = false;
                        moveInEditor.SpiritData.PerformInSpiritOff = false;
                        moveInEditor.SpiritData.StartXOffset = 0;
                        moveInEditor.SpiritData.StartYOffset = 0;
                        moveInEditor.SpiritData.SummonSpirit = false;
                        moveInEditor.SpiritData.ReturnToPlayer = false;
                        moveInEditor.SpiritData.MaintainPosition = false;
                        moveInEditor.SpiritData.Vulnerable = false;
                        moveInEditor.SpiritData.OnlyInSpiritOff = false;
                    }

                    ImGui.TreePop();
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

                ImGui.Text($"Sprite Animation Speed (fps): { currentAnimationSpeedLabel }");

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
                    if (ImGui.ImageButton("##ShowHitboxes", (IntPtr)showHitboxesTexture.id, imageButtonSize))
                    {
                        boxDrawMode = BoxDrawMode.None;
                        showHitHurtboxes = false;
                    }
                }
                else
                {
                    if (ImGui.ImageButton("##HideHitboxes", (IntPtr)hideHitboxesTexture.id, imageButtonSize))
                    {
                        boxDrawMode = BoxDrawMode.Both;
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

        private void RenderCharacterDataWindow(float scale)
        {
            var windowSize = new Vector2();
            windowSize.X = 320 * scale;
            windowSize.Y = 560 * scale;
            
            ImGui.SetNextWindowPos(new Vector2(width - windowSize.X - 10 * scale, 20 * scale));
            ImGui.SetNextWindowSize(windowSize);

            if (ImGui.Begin("Character Properties", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize))
            {
                int idleIndex = 0; // This variable is used for changing the sprite back to the idle animation when deleting a move to prevent crashing

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
                    ImGui.EndTable();

                    var maxHitPoints = character.MaxHitPoints;
                    ImguiDrawingHelper.DrawIntInput("maxHitPoints", ref maxHitPoints);
                    character.MaxHitPoints = maxHitPoints;

                    if (character.UniqueData.SpiritData == SpiritDataType.IsSpirit)
                    {
                        var regenSpeed = character.RegenSpeed;
                        var koRegenSpeed = character.KORegenSpeed;

                        ImguiDrawingHelper.DrawDecimalInput("regenSpeed", ref regenSpeed);
                        ImguiDrawingHelper.DrawDecimalInput("noHealthRegenSpeed", ref koRegenSpeed);

                        character.RegenSpeed = regenSpeed;
                        character.KORegenSpeed = koRegenSpeed;
                    }

                    var horizontalSpeed = character.HorizontalSpeed;
                    var verticalSpeed = character.VerticalSpeed;
                    var envDisplacement = character.EnvironmentalDisplacement;
                    var walkSpeed = character.WalkSpeed;
                    var runSpeed = character.RunSpeed;
                    var traction = character.Traction;
                    var jumpSpeed = character.JumpSpeed;
                    var fallSpeed = character.FallSpeed;
                    var backDashDuration = character.BackDashDuration;
                    var backDashInvincibility = character.BackDashInvincibility;
                    var backDashSpeed = character.BackDashSpeed;
                    var backDashStartup = character.BackDashStartup;
                    var fastFallSpeed = character.FastFallSpeed;
                    var jumpType = character.JumpType;
                    var jumpHorizontalSpeed = character.JumpHorizontalSpeed;
                    var superMeterBuildRate = character.SuperMeterBuildRate;
                    var spriteCollection = character.CharacterSprites;
                    
                    ImguiDrawingHelper.DrawDecimalInput("horizontalSpeed", ref horizontalSpeed);
                    ImguiDrawingHelper.DrawDecimalInput("verticalSpeed", ref verticalSpeed);
                    ImguiDrawingHelper.DrawIntInput("environmentalDisplacement", ref envDisplacement);
                    ImguiDrawingHelper.DrawDecimalInput("walkSpeed", ref walkSpeed);
                    ImguiDrawingHelper.DrawDecimalInput("runSpeed", ref runSpeed);
                    ImguiDrawingHelper.DrawDecimalInput("traction", ref traction);
                    ImguiDrawingHelper.DrawDecimalInput("jumpSpeed", ref jumpSpeed);
                    ImguiDrawingHelper.DrawDecimalInput("fallSpeed", ref fallSpeed);
                    ImguiDrawingHelper.DrawIntInput("backDashDuration", ref backDashDuration);
                    ImguiDrawingHelper.DrawIntInput("backDashInvincibility", ref backDashInvincibility);
                    ImguiDrawingHelper.DrawDecimalInput("backDashSpeed", ref backDashSpeed);
                    ImguiDrawingHelper.DrawDecimalInput("backDashStartup", ref backDashStartup);
                    ImguiDrawingHelper.DrawDecimalInput("fastFallSpeed", ref fastFallSpeed);
                    ImguiDrawingHelper.DrawFlagsInput("jumpType", ref jumpType);
                    ImguiDrawingHelper.DrawDecimalInput("jumpHorizontalSpeed", ref jumpHorizontalSpeed);
                    ImguiDrawingHelper.DrawDecimalInput("superMeterBuildRate", ref superMeterBuildRate);
                    
                    character.HorizontalSpeed = horizontalSpeed;
                    character.VerticalSpeed = verticalSpeed;
                    character.EnvironmentalDisplacement = envDisplacement;
                    character.WalkSpeed = walkSpeed;
                    character.RunSpeed = runSpeed;
                    character.Traction = traction;
                    character.JumpSpeed = jumpSpeed;
                    character.FallSpeed = fallSpeed;
                    character.BackDashDuration = backDashDuration;
                    character.BackDashInvincibility = backDashInvincibility;
                    character.BackDashSpeed = backDashSpeed;
                    character.BackDashStartup = backDashStartup;
                    character.FastFallSpeed = fastFallSpeed;
                    character.JumpType = jumpType;
                    character.JumpHorizontalSpeed = jumpHorizontalSpeed;
                    character.SuperMeterBuildRate = superMeterBuildRate;
                    character.CharacterSprites = spriteCollection;
                }
                ImguiDrawingHelper.DrawVerticalSpacing(scale, 5.0f);

                if (ImGui.CollapsingHeader("Sprite Data", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    var Idle = character.CharacterSprites.Idle;
                    var Crouch = character.CharacterSprites.Crouch;
                    var StandBlock = character.CharacterSprites.StandBlock;
                    var CrouchBlock = character.CharacterSprites.CrouchBlock;
                    var WalkForward = character.CharacterSprites.WalkForward;
                    var WalkBackward = character.CharacterSprites.WalkBackward;
                    var RunForward = character.CharacterSprites.RunForward;
                    var RunBackward = character.CharacterSprites.RunBackward;
                    var JumpSquat = character.CharacterSprites.JumpSquat;
                    var Jump = character.CharacterSprites.Jump;
                    var Hurt = character.CharacterSprites.Hurt;
                    var Grab = character.CharacterSprites.Grab;
                    var Hold = character.CharacterSprites.Hold;
                    var Launched = character.CharacterSprites.Launched;
                    var Knockdown = character.CharacterSprites.Knockdown;
                    var GetUp = character.CharacterSprites.GetUp;

                    int idleSelected = string.IsNullOrWhiteSpace(Idle) ? -1 : allSprites.IndexOf(allSprites.First(x => x.Name == Idle));
                    int crouchSelected = string.IsNullOrWhiteSpace(Crouch) ? -1 : allSprites.IndexOf(allSprites.First(x => x.Name == Crouch));
                    int standBlockSelected = string.IsNullOrWhiteSpace(StandBlock) ? -1 : allSprites.IndexOf(allSprites.First(x => x.Name == StandBlock));
                    int crouchBlockSelected = string.IsNullOrWhiteSpace(CrouchBlock) ? -1 : allSprites.IndexOf(allSprites.First(x => x.Name == CrouchBlock));
                    int walkForwardSelected = string.IsNullOrWhiteSpace(WalkForward) ? -1 : allSprites.IndexOf(allSprites.First(x => x.Name == WalkForward));
                    int walkBackwardSelected = string.IsNullOrWhiteSpace(WalkBackward) ? -1 : allSprites.IndexOf(allSprites.First(x => x.Name == WalkBackward));
                    int runForwardSelected = string.IsNullOrWhiteSpace(RunForward) ? -1 : allSprites.IndexOf(allSprites.First(x => x.Name == RunForward));
                    int runBackwardSelected = string.IsNullOrWhiteSpace(RunBackward) ? -1 : allSprites.IndexOf(allSprites.First(x => x.Name == RunBackward));
                    int jumpSquatSelected = string.IsNullOrWhiteSpace(JumpSquat) ? -1 : allSprites.IndexOf(allSprites.First(x => x.Name == JumpSquat));
                    int jumpSelected = string.IsNullOrWhiteSpace(Jump) ? -1 : allSprites.IndexOf(allSprites.First(x => x.Name == Jump));
                    int hurtSelected = string.IsNullOrWhiteSpace(Hurt) ? -1 : allSprites.IndexOf(allSprites.First(x => x.Name == Hurt));
                    int grabSelected = string.IsNullOrWhiteSpace(Grab) ? -1 : allSprites.IndexOf(allSprites.First(x => x.Name == Grab));
                    int holdSelected = string.IsNullOrWhiteSpace(Hold) ? -1 : allSprites.IndexOf(allSprites.First(x => x.Name == Hold));
                    int launchedSelected = string.IsNullOrWhiteSpace(Launched) ? -1 : allSprites.IndexOf(allSprites.First(x => x.Name == Launched));
                    int knockdownSelected = string.IsNullOrWhiteSpace(Knockdown) ? -1 : allSprites.IndexOf(allSprites.First(x => x.Name == Knockdown));
                    int getUpSelected = string.IsNullOrWhiteSpace(GetUp) ? -1 : allSprites.IndexOf(allSprites.First(x => x.Name == GetUp));

                    var selectionAction = (int selectedIndex) =>
                    {
                        if (selectedIndex > -1)
                        {
                            var sprite = allSprites[selectedIndex];
                            ChangeAnimatedSprite(sprite);
                        }

                        showingMove = false;
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

                    ImguiDrawingHelper.DrawSelectableComboInput($"idle{isPlaying(idleSelected)}", allSprites.Select(x => x.Name).ToArray(), ref idleSelected, selectionAction, changeAction);
                    ImguiDrawingHelper.DrawSelectableComboInput($"crouch{isPlaying(crouchSelected)}", allSprites.Select(x => x.Name).ToArray(), ref crouchSelected, selectionAction, changeAction);
                    ImguiDrawingHelper.DrawSelectableComboInput($"standBlock{isPlaying(standBlockSelected)}", allSprites.Select(x => x.Name).ToArray(), ref standBlockSelected, selectionAction, changeAction);
                    ImguiDrawingHelper.DrawSelectableComboInput($"crouchBlock{isPlaying(crouchBlockSelected)}", allSprites.Select(x => x.Name).ToArray(), ref crouchBlockSelected, selectionAction, changeAction);
                    ImguiDrawingHelper.DrawSelectableComboInput($"walkForward{isPlaying(walkForwardSelected)}", allSprites.Select(x => x.Name).ToArray(), ref walkForwardSelected, selectionAction, changeAction);
                    ImguiDrawingHelper.DrawSelectableComboInput($"walkBackward{isPlaying(walkBackwardSelected)}", allSprites.Select(x => x.Name).ToArray(), ref walkBackwardSelected, selectionAction, changeAction);
                    ImguiDrawingHelper.DrawSelectableComboInput($"runForward{isPlaying(runForwardSelected)}", allSprites.Select(x => x.Name).ToArray(), ref runForwardSelected, selectionAction, changeAction);
                    ImguiDrawingHelper.DrawSelectableComboInput($"runBackward{isPlaying(runBackwardSelected)}", allSprites.Select(x => x.Name).ToArray(), ref runBackwardSelected, selectionAction, changeAction);
                    ImguiDrawingHelper.DrawSelectableComboInput($"jumpSquat{isPlaying(jumpSquatSelected)}", allSprites.Select(x => x.Name).ToArray(), ref jumpSquatSelected, selectionAction, changeAction);
                    ImguiDrawingHelper.DrawSelectableComboInput($"jump{isPlaying(jumpSelected)}", allSprites.Select(x => x.Name).ToArray(), ref jumpSelected, selectionAction, changeAction);
                    ImguiDrawingHelper.DrawSelectableComboInput($"hurt{isPlaying(hurtSelected)}", allSprites.Select(x => x.Name).ToArray(), ref hurtSelected, selectionAction, changeAction);
                    ImguiDrawingHelper.DrawSelectableComboInput($"grab{isPlaying(grabSelected)}", allSprites.Select(x => x.Name).ToArray(), ref grabSelected, selectionAction, changeAction);
                    ImguiDrawingHelper.DrawSelectableComboInput($"hold{isPlaying(holdSelected)}", allSprites.Select(x => x.Name).ToArray(), ref holdSelected, selectionAction, changeAction);
                    ImguiDrawingHelper.DrawSelectableComboInput($"launched{isPlaying(launchedSelected)}", allSprites.Select(x => x.Name).ToArray(), ref launchedSelected, selectionAction, changeAction);
                    ImguiDrawingHelper.DrawSelectableComboInput($"knockDown{isPlaying(knockdownSelected)}", allSprites.Select(x => x.Name).ToArray(), ref knockdownSelected, selectionAction, changeAction);
                    ImguiDrawingHelper.DrawSelectableComboInput($"getUp{isPlaying(getUpSelected)}", allSprites.Select(x => x.Name).ToArray(), ref getUpSelected, selectionAction, changeAction);

                    character.CharacterSprites.Idle = idleSelected != -1 ? allSprites[idleSelected].Name : string.Empty;
                    character.CharacterSprites.Crouch = crouchSelected != -1 ? allSprites[crouchSelected].Name : string.Empty;
                    character.CharacterSprites.StandBlock = standBlockSelected != -1 ? allSprites[standBlockSelected].Name : string.Empty;
                    character.CharacterSprites.CrouchBlock = crouchBlockSelected != -1 ? allSprites[crouchBlockSelected].Name : string.Empty;
                    character.CharacterSprites.WalkForward = walkForwardSelected != -1 ? allSprites[walkForwardSelected].Name : string.Empty;
                    character.CharacterSprites.WalkBackward = walkBackwardSelected != -1 ? allSprites[walkBackwardSelected].Name : string.Empty;
                    character.CharacterSprites.RunForward = runForwardSelected != -1 ? allSprites[runForwardSelected].Name : string.Empty;
                    character.CharacterSprites.RunBackward = runBackwardSelected != -1 ? allSprites[runBackwardSelected].Name : string.Empty;
                    character.CharacterSprites.JumpSquat = jumpSquatSelected != -1 ? allSprites[jumpSquatSelected].Name : string.Empty;
                    character.CharacterSprites.Jump = jumpSelected != -1 ? allSprites[jumpSelected].Name : string.Empty;
                    character.CharacterSprites.Hurt = hurtSelected != -1 ? allSprites[hurtSelected].Name : string.Empty;
                    character.CharacterSprites.Grab = grabSelected != -1 ? allSprites[grabSelected].Name : string.Empty;
                    character.CharacterSprites.Hold = holdSelected != -1 ? allSprites[holdSelected].Name : string.Empty;
                    character.CharacterSprites.Launched = launchedSelected != -1 ? allSprites[launchedSelected].Name : string.Empty;
                    character.CharacterSprites.Knockdown = knockdownSelected != -1 ? allSprites[knockdownSelected].Name : string.Empty;
                    character.CharacterSprites.GetUp = getUpSelected != -1 ? allSprites[getUpSelected].Name : string.Empty;

                    idleIndex = idleSelected;

                    walkForwardSprite = allSprites[walkForwardSelected];
                    walkBackwardSprite = allSprites[walkBackwardSelected];
                    runForwardSprite = allSprites[runForwardSelected];
                    runBackwardSprite = allSprites[runBackwardSelected];
                }

                ImguiDrawingHelper.DrawVerticalSpacing(scale, 5.0f);

                if (ImGui.CollapsingHeader("Sound Data", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    var walkingSoundEffect = character.NonmoveSoundData.WalkingSoundEffect;
                    var runningSoundEffect = character.NonmoveSoundData.RunningSoundEffect;
                    var walkForwardFootsteps = character.NonmoveSoundData.WalkForwardFootsteps;
                    var walkBackwardFootsteps = character.NonmoveSoundData.WalkBackwardFootsteps;
                    var runForwardFootsteps = character.NonmoveSoundData.RunForwardFootsteps;
                    var runBackwardFootsteps = character.NonmoveSoundData.RunBackwardFootsteps;

                    var walkSoundId = character.NonmoveSoundData.WalkingSoundEffect ?? string.Empty;
                    var selectedWalkSoundIndex = walkSoundId != string.Empty ? allSounds.IndexOf(allSounds.First(x => x.Name == character.NonmoveSoundData.WalkingSoundEffect)) : -1;
                    ImguiDrawingHelper.DrawComboInput("walkFootstep", allSounds.Select(x => x.Name).ToArray(), ref selectedWalkSoundIndex);
                    walkingSoundEffect = selectedWalkSoundIndex != -1 ? allSounds[selectedWalkSoundIndex].Name : string.Empty;

                    var runSoundId = character.NonmoveSoundData.RunningSoundEffect ?? string.Empty;
                    var selectedRunSoundIndex = runSoundId != string.Empty ? allSounds.IndexOf(allSounds.First(x => x.Name == character.NonmoveSoundData.RunningSoundEffect)) : -1;
                    ImguiDrawingHelper.DrawComboInput("runFootstep", allSounds.Select(x => x.Name).ToArray(), ref selectedRunSoundIndex);
                    runningSoundEffect = selectedRunSoundIndex != -1 ? allSounds[selectedRunSoundIndex].Name : string.Empty;

                    if (spriteData == walkForwardSprite && walkForwardSprite.Frames.Count > 0)
                    {
                        if (walkingSoundEffect != string.Empty)
                        {
                            string filePath = projectData.ProjectPathOnly + @"sounds\" + walkingSoundEffect + @"\" + walkingSoundEffect + ".wav";
                            if (!File.Exists(filePath))
                            {
                                _logger.LogError($"File path {filePath} does not exist or is not accessable.");
                                walkingSoundEffect = "";
                            }
                            else
                            {
                                if (footstepSoundPlayer != null)
                                {
                                    string currentPath = footstepSoundPlayer.filePath;
                                    if (currentPath != filePath)
                                    {
                                        footstepSoundPlayer = new CachedSound(filePath);
                                    }
                                }
                                else
                                {
                                    footstepSoundPlayer = new CachedSound(filePath);
                                }
                            }
                        }

                        // Create list of checkboxes for walking forward footsteps
                        ImGui.Columns(2);
                        ImGui.Text("Footstep Frames:");
                        ImGui.SameLine();
                        ImguiDrawingHelper.DrawHelpMarker("If multiple walk/run cycles use the same sprite sheet, this will affect all of them.");
                        ImGui.Columns(1);

                        // Shortens length of the list
                        while (walkForwardFootsteps.Count > walkForwardSprite.Frames.Count)
                        {
                            walkForwardFootsteps.RemoveAt(walkForwardSprite.Frames.Count);
                        }

                        // Removes sprite frames that are out of the range of possible sprite frames
                        int numberOfInvalidFrames = 0;
                        List<int> invalidFrames = new List<int>();
                        foreach (int frame in walkForwardFootsteps)
                        {
                            if (frame > walkForwardSprite.Frames.Count)
                            {
                                numberOfInvalidFrames++;
                                invalidFrames.Add(frame);
                            }
                        }
                        while (numberOfInvalidFrames > 0)
                        {
                            walkForwardFootsteps.Remove(invalidFrames[0]);
                            invalidFrames.RemoveAt(0);
                            numberOfInvalidFrames--;
                        }

                        // Create a list of checkboxes, one checkbox for each moveset
                        for (int i = 0; i < walkForwardSprite.Frames.Count; i++)
                        {
                            var isSelected = false;
                            if (walkForwardFootsteps.Contains(i + 1))
                            {
                                isSelected = true;
                            }

                            ImguiDrawingHelper.DrawBoolInput("Frame" + (i + 1).ToString(), ref isSelected);
                            if (isSelected && !walkForwardFootsteps.Contains(i + 1))
                            {
                                walkForwardFootsteps.Add(i + 1);
                            }
                            else if (!isSelected && walkForwardFootsteps.Contains(i + 1))
                            {
                                walkForwardFootsteps.Remove(i + 1);
                            }
                        }
                        walkForwardFootsteps.Sort();
                    }
                    else if (spriteData == walkBackwardSprite && walkBackwardSprite.Frames.Count > 0)
                    {
                        if (walkingSoundEffect != string.Empty)
                        {
                            string filePath = projectData.ProjectPathOnly + @"sounds\" + walkingSoundEffect + @"\" + walkingSoundEffect + ".wav";
                            if (!File.Exists(filePath))
                            {
                                _logger.LogError($"File path {filePath} does not exist or is not accessable.");
                                walkingSoundEffect = "";
                            }
                            else
                            {
                                if (footstepSoundPlayer != null)
                                {
                                    string currentPath = footstepSoundPlayer.filePath;
                                    if (currentPath != filePath)
                                    {
                                        footstepSoundPlayer = new CachedSound(filePath);
                                    }
                                }
                                else
                                {
                                    footstepSoundPlayer = new CachedSound(filePath);
                                }
                            }
                        }

                        // Create list of checkboxes for walking backward footsteps
                        ImGui.Columns(2);
                        ImGui.Text("Footstep Frames:");
                        ImGui.SameLine();
                        ImguiDrawingHelper.DrawHelpMarker("If multiple walk/run cycles use the same sprite sheet, this will affect all of them.");
                        ImGui.Columns(1);

                        // Shortens length of the list
                        while (walkBackwardFootsteps.Count > walkBackwardSprite.Frames.Count)
                        {
                            walkBackwardFootsteps.RemoveAt(walkBackwardSprite.Frames.Count);
                        }

                        // Removes sprite frames that are out of the range of sprite frames
                        int numberOfInvalidFrames = 0;
                        List<int> invalidFrames = new List<int>();
                        foreach (int frame in walkBackwardFootsteps)
                        {
                            if (frame > walkBackwardSprite.Frames.Count)
                            {
                                numberOfInvalidFrames++;
                                invalidFrames.Add(frame);
                            }
                        }
                        while (numberOfInvalidFrames > 0)
                        {
                            walkBackwardFootsteps.Remove(invalidFrames[0]);
                            invalidFrames.RemoveAt(0);
                            numberOfInvalidFrames--;
                        }

                        // Create a list of checkboxes, one checkbox for each moveset
                        for (int i = 0; i < walkBackwardSprite.Frames.Count; i++)
                        {
                            var isSelected = false;
                            if (walkBackwardFootsteps.Contains(i + 1))
                            {
                                isSelected = true;
                            }

                            ImguiDrawingHelper.DrawBoolInput("Frame" + (i + 1).ToString(), ref isSelected);
                            if (isSelected && !walkBackwardFootsteps.Contains(i + 1))
                            {
                                walkBackwardFootsteps.Add(i + 1);
                            }
                            else if (!isSelected && walkBackwardFootsteps.Contains(i + 1))
                            {
                                walkBackwardFootsteps.Remove(i + 1);
                            }
                        }
                        walkBackwardFootsteps.Sort();
                    }
                    else if (spriteData == runForwardSprite && runForwardSprite.Frames.Count > 0)
                    {
                        if (runningSoundEffect != string.Empty)
                        {
                            string filePath = projectData.ProjectPathOnly + @"sounds\" + runningSoundEffect + @"\" + runningSoundEffect + ".wav";
                            if (!File.Exists(filePath))
                            {
                                _logger.LogError($"File path {filePath} does not exist or is not accessable.");
                                runningSoundEffect = "";
                            }
                            else
                            {
                                if (footstepSoundPlayer != null)
                                {
                                    string currentPath = footstepSoundPlayer.filePath;
                                    if (currentPath != filePath)
                                    {
                                        footstepSoundPlayer = new CachedSound(filePath);
                                    }
                                }
                                else
                                {
                                    footstepSoundPlayer = new CachedSound(filePath);
                                }
                            }
                        }

                        // Create list of checkboxes for running forward footsteps
                        ImGui.Columns(2);
                        ImGui.Text("Footstep Frames:");
                        ImGui.SameLine();
                        ImguiDrawingHelper.DrawHelpMarker("If multiple walk/run cycles use the same sprite sheet, this will affect all of them.");
                        ImGui.Columns(1);

                        // Shortens length of the list
                        while (runForwardFootsteps.Count > runForwardSprite.Frames.Count)
                        {
                            runForwardFootsteps.RemoveAt(runForwardSprite.Frames.Count);
                        }

                        // Removes sprite frames that are out of the range of possible sprite frames
                        int numberOfInvalidFrames = 0;
                        List<int> invalidFrames = new List<int>();
                        foreach (int frame in runForwardFootsteps)
                        {
                            if (frame > runForwardSprite.Frames.Count)
                            {
                                numberOfInvalidFrames++;
                                invalidFrames.Add(frame);
                            }
                        }
                        while (numberOfInvalidFrames > 0)
                        {
                            runForwardFootsteps.Remove(invalidFrames[0]);
                            invalidFrames.RemoveAt(0);
                            numberOfInvalidFrames--;
                        }

                        // Create a list of checkboxes, one checkbox for each moveset
                        for (int i = 0; i < runForwardSprite.Frames.Count; i++)
                        {
                            var isSelected = false;
                            if (runForwardFootsteps.Contains(i + 1))
                            {
                                isSelected = true;
                            }

                            ImguiDrawingHelper.DrawBoolInput("Frame" + (i + 1).ToString(), ref isSelected);
                            if (isSelected && !runForwardFootsteps.Contains(i + 1))
                            {
                                runForwardFootsteps.Add(i + 1);
                            }
                            else if (!isSelected && runForwardFootsteps.Contains(i + 1))
                            {
                                runForwardFootsteps.Remove(i + 1);
                            }
                        }
                        runForwardFootsteps.Sort();
                    }
                    else if (spriteData == runBackwardSprite && runBackwardSprite.Frames.Count > 0)
                    {
                        if (runningSoundEffect != string.Empty)
                        {
                            string filePath = projectData.ProjectPathOnly + @"sounds\" + runningSoundEffect + @"\" + runningSoundEffect + ".wav";
                            if (!File.Exists(filePath))
                            {
                                _logger.LogError($"File path {filePath} does not exist or is not accessable.");
                                runningSoundEffect = "";
                            }
                            else
                            {
                                if (footstepSoundPlayer != null)
                                {
                                    string currentPath = footstepSoundPlayer.filePath;
                                    if (currentPath != filePath)
                                    {
                                        footstepSoundPlayer = new CachedSound(filePath);
                                    }
                                }
                                else
                                {
                                    footstepSoundPlayer = new CachedSound(filePath);
                                }
                            }
                        }

                        // Create list of checkboxes for running backward footsteps
                        ImGui.Columns(2);
                        ImGui.Text("Footstep Frames:");
                        ImGui.SameLine();
                        ImguiDrawingHelper.DrawHelpMarker("If multiple walk/run cycles use the same sprite sheet, this will affect all of them.");
                        ImGui.Columns(1);

                        // Shortens length of the list
                        while (runBackwardFootsteps.Count > runBackwardSprite.Frames.Count)
                        {
                            runBackwardFootsteps.RemoveAt(runBackwardSprite.Frames.Count);
                        }

                        // Removes sprite frames that are out of the range of sprite frames
                        int numberOfInvalidFrames = 0;
                        List<int> invalidFrames = new List<int>();
                        foreach (int frame in runBackwardFootsteps)
                        {
                            if (frame > runBackwardSprite.Frames.Count)
                            {
                                numberOfInvalidFrames++;
                                invalidFrames.Add(frame);
                            }
                        }
                        while (numberOfInvalidFrames > 0)
                        {
                            runBackwardFootsteps.Remove(invalidFrames[0]);
                            invalidFrames.RemoveAt(0);
                            numberOfInvalidFrames--;
                        }

                        // Create a list of checkboxes, one checkbox for each moveset
                        for (int i = 0; i < runBackwardSprite.Frames.Count; i++)
                        {
                            var isSelected = false;
                            if (runBackwardFootsteps.Contains(i + 1))
                            {
                                isSelected = true;
                            }

                            ImguiDrawingHelper.DrawBoolInput("Frame" + (i + 1).ToString(), ref isSelected);
                            if (isSelected && !runBackwardFootsteps.Contains(i + 1))
                            {
                                runBackwardFootsteps.Add(i + 1);
                            }
                            else if (!isSelected && runBackwardFootsteps.Contains(i + 1))
                            {
                                runBackwardFootsteps.Remove(i + 1);
                            }
                        }
                        runBackwardFootsteps.Sort();
                    }
                    else
                    {
                        ImGui.Text("Choose a walking or running sprite.");
                    }

                    character.NonmoveSoundData.WalkingSoundEffect = walkingSoundEffect;
                    character.NonmoveSoundData.WalkForwardFootsteps = walkForwardFootsteps;
                    character.NonmoveSoundData.WalkBackwardFootsteps = walkBackwardFootsteps;
                    character.NonmoveSoundData.RunningSoundEffect = runningSoundEffect;
                    character.NonmoveSoundData.RunForwardFootsteps = runForwardFootsteps;
                    character.NonmoveSoundData.RunBackwardFootsteps = runBackwardFootsteps;
                }

                ImguiDrawingHelper.DrawVerticalSpacing(scale, 5.0f);

                if (ImGui.CollapsingHeader("Unique Data", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    if (character.UniqueData.SpiritData != SpiritDataType.IsSpirit)
                    {
                        int additionalMovesets = character.UniqueData.AdditionalMovesets;

                        ImguiDrawingHelper.DrawIntInput("additionalMovesets", ref additionalMovesets);
                        if (additionalMovesets < 0)
                        {
                            additionalMovesets = 0;
                        }

                        character.UniqueData.AdditionalMovesets = additionalMovesets;
                    }

                    SpiritDataType spiritData = character.UniqueData.SpiritData;

                    var dataType = spiritData.ToString();
                    int spiritDataIndex = spiritDataList.IndexOf(dataType.AddSpacesToCamelCase());
                    ImguiDrawingHelper.DrawComboInput("spiritData", spiritDataList.ToArray(), ref spiritDataIndex);
                    var selectedData = spiritDataList[spiritDataIndex];
                    spiritData = (SpiritDataType)Enum.Parse(typeof(SpiritDataType), selectedData.ToCamelCase());
                    
                    character.UniqueData.SpiritData = spiritData;

                    if (character.UniqueData.SpiritData == SpiritDataType.HasSpirit && spiritNames.Contains(character.UniqueData.Spirit))
                    {
                        var spirit = character.UniqueData.Spirit;
                        bool doubleJump = character.UniqueData.DoubleJump;

                        int spiritCharacterIndex = spiritNames.IndexOf(spirit);
                        ImguiDrawingHelper.DrawComboInput("spirit", spiritNames.ToArray(), ref spiritCharacterIndex);
                        var selectedSpiritCharacter = spiritNames[spiritCharacterIndex];
                        if (selectedSpiritCharacter != "None")
                        {
                            foreach (var spiritCharacter in spiritCharacters)
                            {
                                if (spiritCharacter.Name == selectedSpiritCharacter)
                                {
                                    spirit = spiritCharacter.Name;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            spirit = "None";
                        }

                        ImguiDrawingHelper.DrawBoolInput("doubleJump", ref doubleJump, "Toggles the ability to double jump in Spirit ON");

                        character.UniqueData.Spirit = spirit;
                        character.UniqueData.DoubleJump = doubleJump;
                    }
                    else
                    {
                        character.UniqueData.Spirit = "None";
                        character.UniqueData.DoubleJump = false;
                    }

                    if (character.UniqueData.AdditionalMovesets > 0 && character.UniqueData.SpiritData == SpiritDataType.HasSpirit)
                    {
                        bool linkMovesetsWithSpirits = character.UniqueData.LinkMovesetsWithSpirits;
                        ImguiDrawingHelper.DrawBoolInput("linkMovesets /Spirits", ref linkMovesetsWithSpirits);
                        character.UniqueData.LinkMovesetsWithSpirits = linkMovesetsWithSpirits;

                        if (linkMovesetsWithSpirits)
                        {
                            int spiritOffMoveset = character.UniqueData.SpiritOffMoveset;
                            int spiritOnMoveset = character.UniqueData.SpiritOnMoveset;

                            ImguiDrawingHelper.DrawIntInput("spiritOffMoveset", ref spiritOffMoveset);
                            if (spiritOffMoveset < 1)
                            {
                                spiritOffMoveset = 1;
                            }
                            if (spiritOffMoveset > character.UniqueData.AdditionalMovesets + 1)
                            {
                                spiritOffMoveset = character.UniqueData.AdditionalMovesets + 1;
                            }

                            ImguiDrawingHelper.DrawIntInput("spiritOnMoveset", ref spiritOnMoveset);
                            if (spiritOnMoveset < 1)
                            {
                                spiritOnMoveset = 1;
                            }
                            if (spiritOnMoveset > character.UniqueData.AdditionalMovesets + 1)
                            {
                                spiritOnMoveset = character.UniqueData.AdditionalMovesets + 1;
                            }

                            character.UniqueData.SpiritOffMoveset = spiritOffMoveset;
                            character.UniqueData.SpiritOnMoveset = spiritOnMoveset;
                        }
                        else
                        {
                            character.UniqueData.SpiritOffMoveset = 0;
                            character.UniqueData.SpiritOnMoveset = 0;
                        }
                    }
                    else
                    {
                        character.UniqueData.LinkMovesetsWithSpirits = false;
                        character.UniqueData.SpiritOffMoveset = 0;
                        character.UniqueData.SpiritOnMoveset = 0;
                    }
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
                        showingMove = true;
                    }
                    ImguiDrawingHelper.DrawVerticalSpacing(scale, 5.0f);
                    if (character.MoveData != null && character.MoveData.Count > 0)
                    {
                        for (int i = 0; i < character.MoveData.Count; i++)
                        {
                            var selected = character.MoveData[i] == moveInEditor;
                            var moveSpriteIndex = string.IsNullOrWhiteSpace(character.MoveData[i].SpriteName) ? -1 : allSprites.IndexOf(allSprites.First(x => x.Name == character.MoveData[i].SpriteName));
                            var playingIndicator = moveSpriteIndex != -1 && spriteData == allSprites[moveSpriteIndex] ? "*" : "";
                            string moveString = "";
                            if (character.MoveData[i].MoveType.ToString().AddSpacesToCamelCase() == "None")
                            {
                                moveString = character.MoveData[i].EnhanceMoveType.ToString().AddSpacesToCamelCase();
                            }
                            else
                            {
                                moveString = character.MoveData[i].MoveType.ToString().AddSpacesToCamelCase();
                            }

                            if (ImguiDrawingHelper.DrawSelectableWithRemove(() =>
                                {
                                    moveInEditor = character.MoveData[i];
                                    paletteInEditor = null;
                                    editorMode = EditorMode.Move;
                                    totalFrames = moveInEditor.Duration;
                                    // Fill windows list with animation frame indexes for each frame
                                    currentFrame = 0;
                                    ChangeWindowArray();
                                    resetAnimation = true;
                                    showingMove = true;

                                    boxDrawMode = BoxDrawMode.None;
                                    showHitHurtboxes = false;

                                    soundPlayers.Clear();
                                    hitSoundPlayers.Clear();

                                    if (moveSpriteIndex > -1)
                                    {
                                        var sprite = allSprites[moveSpriteIndex];
                                        moveSprite = sprite;
                                        ChangeAnimatedSprite(sprite, true);
                                    }
                                }, () =>
                                {
                                    character.MoveData.Add(character.MoveData[i].GetDuplicate());
                                }, $"{moveString}{playingIndicator}", selected, i))
                            {
                                //if selected, unselect
                                if (moveInEditor == character.MoveData[i])
                                {
                                    moveInEditor = null;
                                    editorMode = EditorMode.None;

                                    //if this sprite is the one playing, remove it
                                    if (moveSpriteIndex != -1 && spriteData == allSprites[moveSpriteIndex])
                                    {
                                        spriteData = null;
                                    }
                                }

                                //remove it here
                                character.MoveData.RemoveAt(i);
                                ChangeAnimatedSprite(allSprites[idleIndex], false); // Changes the sprite upon move deletion to prevent crashing
                                showingMove = false;
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
                    SaveCharacter();
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
                    if (unsaved)
                    {
                        exitConfirmAction = (keycode) =>
                        {
                            if (keycode == (int)KeyboardKey.KEY_S)
                            {
                                SaveCharacter();
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
                                SaveCharacter();
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
                                SaveCharacter();
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
                        SaveCharacter();
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

        private void SaveCharacter()
        {
            _characterOperations.SaveCharacter(character, projectData.ProjectPathOnly);
            originalCharacter = character.Clone();
        }

        private void ChangeWindowArray()
        {
            // Fill windows list with animation frame indexes for each frame
            int currentFrame = 0;
            windows.Clear();
            if (moveInEditor.FrameData.Count > 0)
            {
                for (int i = 0; i < totalFrames; i++)
                {
                    var windowItem = moveInEditor.FrameData[currentFrame];

                    if (i >= windowItem.Length - 1 && currentFrame < moveInEditor.FrameData.Count - 1)
                    {
                        currentFrame++;
                    }

                    if (currentFrame < moveInEditor.FrameData.Count - 1)
                    {
                        windows.Add(windowItem.ImageIndex - 1);
                    }
                    else
                    {
                        if (i < windowItem.Length)
                        {
                            windows.Add(windowItem.ImageIndex - 1);
                        }
                        else
                        {
                            windows.Add(windowItem.ImageIndex);
                        }
                    }
                }
                // Adds an extra window to prevent crashes
                if (windows.Count > 0)
                {
                    windows.Add(windows[windows.Count - 1]);
                }
            }
        }
    }
}
