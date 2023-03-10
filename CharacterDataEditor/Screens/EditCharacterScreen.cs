using CharacterDataEditor.Constants;
using CharacterDataEditor.Enums;
using CharacterDataEditor.Extensions;
using CharacterDataEditor.Helpers;
using CharacterDataEditor.Models;
using CharacterDataEditor.Models.CharacterData;
using CharacterDataEditor.Services;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using static System.Collections.Specialized.BitVector32;

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

        private MoveDataModel moveInEditor;
        private PaletteModel paletteInEditor;
        private List<string> moveTypesList = new List<string>();
        private List<string> attackTypesList = new List<string>();
        private List<string> spriteTypesList = new List<string>();
        private List<SpriteDataModel> allSprites;
        private List<ScriptDataModel> allScripts;

        private string spriteToDraw;
        private string prevSpriteToDraw;
        private SpriteDataModel spriteData;
        private PaletteModel paletteData;
        private SpriteDrawingHelper spriteDrawer;
        private Vector2 spriteDrawPosition;

        private bool animationPaused;
        private FrameAdvance frameAdvance;

        private EditorMode editorMode;
        private BoxDrawMode boxDrawMode;

        private Rectangle boxRect;

        private int currentFrame;
        private AnimatedSpriteReturnDataModel spriteDrawData;

        private int frameCounter;
        private bool unsaved;
        private bool exiting;

        private const int buttonSpacing = 8;

        private string editorWindowTitle;

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

            allSprites = _characterOperations.GetAllGameData<SpriteDataModel>(projectData.ProjectPathOnly);
            allScripts = _characterOperations.GetAllGameData<ScriptDataModel>(projectData.ProjectPathOnly);

            if (action == "edit" && !string.IsNullOrWhiteSpace(character.CharacterSprites?.Idle))
            {
                var sprite = allSprites.FirstOrDefault(x => x.Name == character.CharacterSprites.Idle);
                ChangeAnimatedSprite(sprite);
            }

            playButtonTexture = Raylib.LoadTexture(Path.Combine(AppContext.BaseDirectory, ResourceConstants.PlayButtonPath));
            pauseButtonTexture = Raylib.LoadTexture(Path.Combine(AppContext.BaseDirectory, ResourceConstants.PauseButtonPath));
            advanceOneFrameForwardTexture = Raylib.LoadTexture(Path.Combine(AppContext.BaseDirectory, ResourceConstants.AdvanceOneFrameButtonPath));
            advanceOneFrameBackTexture = Raylib.LoadTexture(Path.Combine(AppContext.BaseDirectory, ResourceConstants.AdvanceOneFrameBackButtonPath));

            spriteDrawer = new SpriteDrawingHelper();
            frameCounter = 0;

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

            var spriteTypes = Enum.GetValues(typeof(SpriteType));
            spriteTypesList = new List<string>();

            foreach (SpriteType item in spriteTypes)
            {
                var itemAsString = item.ToString().AddSpacesToCamelCase();

                spriteTypesList.Add(itemAsString);
            }

            //init spritedrawposition
            spriteDrawPosition = new Vector2(0, 40);

            currentFrame = 0;
            frameAdvance = FrameAdvance.None;
            boxDrawMode = BoxDrawMode.None;

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

        private void ChangeAnimatedSprite(SpriteDataModel sprite)
        {
            //disable box drawing when the change first happens
            boxDrawMode = BoxDrawMode.None;

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
                var frameData = spriteDrawer.DrawSpecificFrameSpriteToScreen(spriteData, character.BaseColor, paletteData, spriteDrawPosition, scale, _logger, maxSpriteSize, frameAdvance, animationFlags);

                currentFrame = frameData.CurrentFrame;
                spriteDrawData = frameData;

                frameAdvance = FrameAdvance.None;
            }
            else
            {
                var frameData = spriteDrawer.DrawSpriteToScreen(spriteData, character.BaseColor, paletteData, spriteDrawPosition, scale, ResourceConstants.LogoPath, _logger, maxSpriteSize, animationFlags);

                currentFrame = frameData.CurrentFrame;
                spriteDrawData = frameData;
            }
        }

        private void RenderHitHurtBox(float scale)
        {
            Color boxDrawColor;

            switch (boxDrawMode)
            {
                case BoxDrawMode.Hurtbox:
                    boxDrawColor = Color.RED;
                    break;
                case BoxDrawMode.Hitbox:
                    boxDrawColor = Color.BLUE;
                    break;
                case BoxDrawMode.None:
                default:
                    return;
            }

            var spriteFinalScale = spriteDrawData.ScaledDrawSize.X / spriteData.Width;

            //adjust height and width to triple then multiply by scale
            var xOriginAdjustment = spriteData.Sequence.xorigin * spriteFinalScale;
            var yOriginAdjustment = spriteData.Sequence.yorigin * spriteFinalScale;

            var xOffsetAdjusted = boxRect.x * spriteFinalScale;
            var yOffsetAdjusted = boxRect.y * spriteFinalScale;

            var xDrawPos = spriteDrawData.DrawOrigin.X + xOriginAdjustment;
            var yDrawPos = spriteDrawData.DrawOrigin.Y + yOriginAdjustment;
            var finalWidth = boxRect.width * spriteFinalScale;
            var finalHeight = boxRect.height * spriteFinalScale;

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
            
            //draw the box
            Raylib.DrawRectangleLinesEx(destRect, 3.0f, boxDrawColor);
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
                        moveInEditor = null;
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
                                moveInEditor = null;
                                editorMode = EditorMode.Palette;
                                ChangeRenderedPalette(palette);
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
            bool anyBoxRenderActive = false;

            editorWindowTitle = "Move Editor";

            if (moveInEditor == null)
            {
                ImGui.Text("No move selected...");
            }
            else
            {
                int selectedMoveType = (int)moveInEditor.MoveType;
                ImguiDrawingHelper.DrawComboInput("moveType", moveTypesList.ToArray(), ref selectedMoveType);
                moveInEditor.MoveType = (MoveType)selectedMoveType;

                var spriteId = moveInEditor.SpriteName ?? string.Empty;
                var selectedSpriteIndex = spriteId != string.Empty ? allSprites.IndexOf(allSprites.First(x => x.Name == moveInEditor.SpriteName)) : -1;
                ImguiDrawingHelper.DrawComboInput("sprite", allSprites.Select(x => x.Name).ToArray(), ref selectedSpriteIndex);

                var scriptId = moveInEditor.SupplimentaryScript ?? string.Empty;
                var selectedScriptIndex = scriptId != string.Empty ? allScripts.IndexOf(allScripts.First(x => x.Name == moveInEditor.SupplimentaryScript)) : -1;
                ImguiDrawingHelper.DrawComboInput("supplimentaryScript", allScripts.Select(x => x.Name).ToArray(), ref selectedScriptIndex);
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
                ImguiDrawingHelper.DrawIntInput("moveDuration", ref moveDuration, 0);
                moveInEditor.Duration = moveDuration;

                bool isThrow = moveInEditor.IsThrow;
                ImguiDrawingHelper.DrawBoolInput("isMoveAThrow?", ref isThrow);
                moveInEditor.IsThrow = isThrow;

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
                            moveInEditor.CounterData.RemoveAt(moveInEditor.NumberOfHitboxes - 1);
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
                                float pushback = attackDataItem.Pushback;
                                int particleOffsetX = attackDataItem.ParticleXOffset;
                                int particleOffsetY = attackDataItem.ParticleYOffset;
                                string particleEffect = attackDataItem.ParticleEffect;
                                int particleDuration = attackDataItem.ParticleDuration;
                                int holdOffsetX = attackDataItem.HoldXOffset;
                                int holdOffsetY = attackDataItem.HoldYOffset;
                                float comboScaling = attackDataItem.ComboScaling;
                                float meterGain = attackDataItem.MeterGain;
                                bool causesWallbounce = attackDataItem.CausesWallbounce;

                                ImguiDrawingHelper.DrawIntInput("start", ref start);
                                ImguiDrawingHelper.DrawIntInput("lifetime", ref lifetime);
                                ImguiDrawingHelper.DrawBoolInput("causesWallbounce", ref causesWallbounce);
                                var wSelect = ImguiDrawingHelper.DrawIntInput("attackWidth", ref attackWidth);
                                var hSelect = ImguiDrawingHelper.DrawIntInput("attackHeight", ref attackHeight);
                                var xSelect = ImguiDrawingHelper.DrawIntInput("widthOffset", ref widthOffset);
                                var ySelect = ImguiDrawingHelper.DrawIntInput("heightOffset", ref heightOffset);
                                ImguiDrawingHelper.DrawIntInput("group", ref group);
                                ImguiDrawingHelper.DrawIntInput("damage", ref damage);
                                ImguiDrawingHelper.DrawDecimalInput("meterGain", ref meterGain);
                                ImguiDrawingHelper.DrawDecimalInput("comboScaling", ref comboScaling);
                                ImguiDrawingHelper.DrawIntInput("attackHitStop", ref attackHitstop);
                                ImguiDrawingHelper.DrawIntInput("attackHitStun", ref attackHitstun);

                                if (wSelect || hSelect || xSelect || ySelect)
                                {
                                    anyBoxRenderActive = true;
                                    boxRect = new Rectangle(widthOffset, heightOffset, attackWidth, attackHeight);
                                    boxDrawMode = BoxDrawMode.Hitbox;
                                }

                                int selectedAttackType = (int)attackType;
                                ImguiDrawingHelper.DrawComboInput("attackType", attackTypesList.ToArray(), ref selectedAttackType);

                                ImguiDrawingHelper.DrawDecimalInput("blockStun", ref blockStun);
                                ImguiDrawingHelper.DrawDecimalInput("knockback", ref knockBack);
                                ImguiDrawingHelper.DrawDecimalInput("airKnockbackVertical", ref airKnockbackV);
                                ImguiDrawingHelper.DrawDecimalInput("airKnockbackHorizontal", ref airKnockbackH);
                                ImguiDrawingHelper.DrawBoolInput("launches", ref launches);
                                ImguiDrawingHelper.DrawDecimalInput("launchKnockbackVertical", ref launchKnockbackV);
                                ImguiDrawingHelper.DrawDecimalInput("launchKnockbackHorizontal", ref launchKnockbackH);
                                ImguiDrawingHelper.DrawDecimalInput("pushback", ref pushback);
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
                                attackDataItem.ComboScaling = comboScaling;
                                attackDataItem.MeterGain = meterGain;
                                attackDataItem.CausesWallbounce = causesWallbounce;

                                ImGui.TreePop();
                            }
                        }
                    }
                }

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
                                int AttackHitStop = currentCounterData.AttackHitStop;
                                int AttackHitStun = currentCounterData.AttackHitStun;
                                float KnockBack = currentCounterData.KnockBack;
                                float AirKnockbackVertical = currentCounterData.AirKnockbackVertical;
                                float AirKnockbackHorizontal = currentCounterData.AirKnockbackHorizontal;
                                float Pushback = currentCounterData.Pushback;
                                int ParticleXOffset = currentCounterData.ParticleXOffset;
                                int ParticleYOffset = currentCounterData.ParticleYOffset;
                                string ParticleEffect = currentCounterData.ParticleEffect;
                                int ParticleDuration = currentCounterData.ParticleDuration;
                                bool Launches = currentCounterData.Launches;
                                float LaunchKnockbackVertical = currentCounterData.LaunchKnockbackVertical;
                                float LaunchKnockbackHorizontal = currentCounterData.LaunchKnockbackHorizontal;
                                float ComboScaling = currentCounterData.ComboScaling;
                                bool CausesWallbounce = currentCounterData.CausesWallbounce;

                                ImguiDrawingHelper.DrawIntInput("counterHitLevel", ref CounterHitLevel);
                                ImguiDrawingHelper.DrawBoolInput("causesWallbounce", ref CausesWallbounce);
                                ImguiDrawingHelper.DrawIntInput("group", ref Group);
                                ImguiDrawingHelper.DrawIntInput("damage", ref Damage);
                                ImguiDrawingHelper.DrawDecimalInput("meterGain", ref MeterGain);
                                ImguiDrawingHelper.DrawDecimalInput("comboScaling", ref ComboScaling);
                                ImguiDrawingHelper.DrawIntInput("attackHitStop", ref AttackHitStop);
                                ImguiDrawingHelper.DrawIntInput("attackHitStun", ref AttackHitStun);
                                ImguiDrawingHelper.DrawDecimalInput("knockBack", ref KnockBack);
                                ImguiDrawingHelper.DrawDecimalInput("pushback", ref Pushback);
                                ImguiDrawingHelper.DrawDecimalInput("airKnockbackVertical", ref AirKnockbackVertical);
                                ImguiDrawingHelper.DrawDecimalInput("airKnockbackHorizontal", ref AirKnockbackHorizontal);
                                ImguiDrawingHelper.DrawBoolInput("launches", ref Launches);
                                if (Launches)
                                {
                                    ImguiDrawingHelper.DrawDecimalInput("launchKnockbackVertical", ref LaunchKnockbackVertical);
                                    ImguiDrawingHelper.DrawDecimalInput("launchKnockbackHorizontal", ref LaunchKnockbackHorizontal);
                                }
                                ImguiDrawingHelper.DrawIntInput("particleOffset X", ref ParticleXOffset);
                                ImguiDrawingHelper.DrawIntInput("particleOffset Y", ref ParticleYOffset);

                                int selectedParticleEffect = string.IsNullOrWhiteSpace(ParticleEffect) ? 0 : allSprites.IndexOf(allSprites.Where(x => x.Name == ParticleEffect).First());
                                ImguiDrawingHelper.DrawComboInput("particleEffect", allSprites.Select(x => x.Name).ToArray(), ref selectedParticleEffect);

                                ImguiDrawingHelper.DrawIntInput("particleDuration", ref ParticleDuration);

                                currentCounterData.CounterHitLevel = CounterHitLevel;
                                currentCounterData.Group = Group;
                                currentCounterData.Damage = Damage;
                                currentCounterData.MeterGain = MeterGain;
                                currentCounterData.AttackHitStop = AttackHitStop;
                                currentCounterData.AttackHitStun = AttackHitStun;
                                currentCounterData.KnockBack = KnockBack;
                                currentCounterData.AirKnockbackVertical = AirKnockbackVertical;
                                currentCounterData.AirKnockbackHorizontal = AirKnockbackHorizontal;
                                currentCounterData.Pushback = Pushback;
                                currentCounterData.ParticleXOffset = ParticleXOffset;
                                currentCounterData.ParticleYOffset = ParticleYOffset;
                                currentCounterData.ParticleEffect = allSprites[selectedParticleEffect].Name;
                                currentCounterData.ParticleDuration = ParticleDuration;
                                currentCounterData.Launches = Launches;
                                currentCounterData.ComboScaling = ComboScaling;
                                currentCounterData.LaunchKnockbackVertical = LaunchKnockbackVertical;
                                currentCounterData.LaunchKnockbackHorizontal= LaunchKnockbackHorizontal;
                                currentCounterData.CausesWallbounce = CausesWallbounce;

                                ImGui.TreePop();
                            }

                            moveInEditor.CounterData[i] = currentCounterData;
                        }
                    }
                }

                if (ImGui.CollapsingHeader("Rehit Data"))
                {
                    int rehitHitbox = moveInEditor.RehitData.HitBox;
                    int numberOfRepeats = moveInEditor.RehitData.NumberOfHits;

                    ImguiDrawingHelper.DrawIntInput("hitboxToRepeat", ref rehitHitbox, 0);
                    ImguiDrawingHelper.DrawIntInput("numberOfRepeats", ref numberOfRepeats, 0);

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

                if (isThrow && ImGui.CollapsingHeader("Opponent Position Data"))
                {
                    float distanceFromWall = moveInEditor.OpponentPositionData.DistanceFromWall;
                    int numberOfFrames = moveInEditor.OpponentPositionData.NumberOfFrames;

                    ImguiDrawingHelper.DrawDecimalInput("distanceFromWall", ref distanceFromWall);
                    ImguiDrawingHelper.DrawIntInput("numberOfFrames", ref numberOfFrames);

                    moveInEditor.OpponentPositionData.DistanceFromWall = distanceFromWall;

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
                                var wSelect = ImguiDrawingHelper.DrawIntInput("attackWidth", ref attackWidth);
                                var hSelect = ImguiDrawingHelper.DrawIntInput("attackHeight", ref attackHeight);
                                var xSelect = ImguiDrawingHelper.DrawIntInput("widthOffset", ref widthOffset);
                                var ySelect = ImguiDrawingHelper.DrawIntInput("heightOffset", ref heightOffset);

                                if (wSelect || hSelect || xSelect || ySelect)
                                {
                                    anyBoxRenderActive = true;
                                    boxRect = new Rectangle(widthOffset, heightOffset, attackWidth, attackHeight);
                                    boxDrawMode = BoxDrawMode.Hurtbox;
                                }

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
            }

            if (!anyBoxRenderActive)
            {
                boxDrawMode = BoxDrawMode.None;
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

                var spriteName = spriteData != null ? spriteData.Name : "No Selected Sprite";

                ImGui.Text($"Current Sprite: {spriteName}");

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

                var spriteFramesCount = spriteData != null ? spriteData.Frames.Count : 0;

                ImGui.Text($"Total Frames: {spriteFramesCount}");

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
                    ImGui.EndTable();

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
                            var playingIndicator = moveSpriteIndex != -1 && spriteData == allSprites[moveSpriteIndex] ? "*" : "";

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

                                    //if this sprite is the one playing, remove it
                                    if (moveSpriteIndex != -1 && spriteData == allSprites[moveSpriteIndex])
                                    {
                                        spriteData = null;
                                    }
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
    }
}
