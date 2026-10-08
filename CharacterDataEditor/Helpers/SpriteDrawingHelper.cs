using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using CharacterDataEditor.Constants;
using CharacterDataEditor.Enums;
using CharacterDataEditor.Extensions;
using CharacterDataEditor.Models;
using CharacterDataEditor.Services;
using Microsoft.Extensions.Logging;
using Raylib_cs;

namespace CharacterDataEditor.Helpers;

public class SpriteDrawingHelper
{
    private int currentAnimationFrame = 0;
    private int currentTotalFrame = 1;
    private int frameCounter = 0;
    private int nextFrameAdvance = 0;
    private string previousSprite = string.Empty;
    private string currentSprite = string.Empty;

    private List<LoadedTextureModel> spriteTextures;
    private Texture2D bullseye;

    private Vector2 ClientWindow => _displayInfo.ClientSize;

    private readonly IDisplayInfo _displayInfo;

    public SpriteDrawingHelper(IDisplayInfo displayInfo)
    {
        _displayInfo = displayInfo;
        bullseye = Raylib.LoadTexture(Path.Combine(AppContext.BaseDirectory, ResourceConstants.BullseyePath));
    }

    private AnimatedSpriteReturnDataModel DrawSprite(SpriteDrawDataModel data)
    {
        Texture2D textureToDraw;

        if (currentAnimationFrame > spriteTextures.Count - 1)
        {
            currentAnimationFrame = spriteTextures.Count - 1;
        }

        if (data.Flags.HasFlag(SpriteDrawFlags.NotAnimated))
        {
            textureToDraw = spriteTextures.FirstOrDefault().Texture;
        }
        else
        {
            if (currentAnimationFrame < 0)
            {
                currentAnimationFrame = 0;
            }
            textureToDraw = spriteTextures[currentAnimationFrame].Texture;
        }

        var textureSourceRectangle = new Rectangle(0.0f, 0.0f, textureToDraw.Width, textureToDraw.Height);

        //destination rectangle determines the size to scale it to and the position on screen
        var destinationRectangle = new Rectangle
        {
            Width = (textureToDraw.Width * 3) * data.Scale,
            Height = (textureToDraw.Height * 3) * data.Scale
        };

        //check if sprite destination is above max size, if so... determine the scale between x and y, and adjust the larger to the bounds
        // and the smaller to be scaled appropriately

        Vector2 Rescale(float w, float h, Vector2 max)
        {
            var widthOver = w - max.X;
            var heightOver = h - data.MaxDrawSize.Y;

            if (widthOver >= heightOver)
            {
                var differenceScale = h / w;

                w = max.X;
                h = max.X * differenceScale;
            }
            else
            {
                var differenceScale = w / h;

                h = max.Y;
                w = max.Y * differenceScale;
            }

            widthOver = w - max.X;
            heightOver = h - data.MaxDrawSize.Y;

            if (widthOver > 0 || heightOver > 0)
            {
                return Rescale(w, h, max);
            }

            return new Vector2(w, h);
        }

        if (data.MaxDrawSize != Vector2.Zero && (destinationRectangle.Width > data.MaxDrawSize.X || destinationRectangle.Height > data.MaxDrawSize.Y))
        {
            data.MaxDrawSize *= data.Scale;
            var newWH = Rescale(destinationRectangle.Width, destinationRectangle.Height, data.MaxDrawSize);

            destinationRectangle.Width = newWH.X;
            destinationRectangle.Height = newWH.Y;
        }

        if (data.Flags.HasFlag(SpriteDrawFlags.CenterHorizontal))
        {
            var centerHorizontal = ClientWindow.X / 2 - (destinationRectangle.Width / 2);
            destinationRectangle.X = centerHorizontal;
        }
        else
        {
            destinationRectangle.X = data.DrawPosition.X * data.Scale;
        }

        if (data.Flags.HasFlag(SpriteDrawFlags.CenterVertical))
        {
            var centerVertical = ClientWindow.Y / 2 - (destinationRectangle.Height / 2);
            destinationRectangle.Y = centerVertical;
        }
        else
        {
            destinationRectangle.Y = data.DrawPosition.Y * data.Scale;
        }

        if (data.Flags.HasFlag(SpriteDrawFlags.ShowSpriteOutline))
        {
            Raylib.DrawRectangle((int)destinationRectangle.X, (int)destinationRectangle.Y, (int)destinationRectangle.Width, (int)destinationRectangle.Height, Color.Black);
        }

        if (data.Flags.HasFlag(SpriteDrawFlags.PaletteSwapActive) && data.BaseColor?.ColorPalette?.Count > 0 && data.SwapColor?.ColorPalette?.Count > 0)
        {
            ShaderHelper.ShaderStartRender();

            var baseColorArray = data.BaseColor.ToShaderVec4Array();
            var swapColorArray = data.SwapColor.ToShaderVec4Array();

            for (var i = 0; i < baseColorArray.Length; i++)
            {
                ShaderHelper.SetValue($"basecolor{i}", baseColorArray[i], ShaderUniformDataType.Vec4);
                ShaderHelper.SetValue($"swapcolor{i}", swapColorArray[i], ShaderUniformDataType.Vec4);
            }
        }

        // Origin determines where everything is based, passing 0x0y keeps it default
        // Color.White is used to not tint the texture at all
        Raylib.DrawTexturePro(textureToDraw, textureSourceRectangle, destinationRectangle, Vector2.Zero, 0.0f, Color.White);

        if (data.Flags.HasFlag(SpriteDrawFlags.PaletteSwapActive))
        {
            ShaderHelper.ShaderEndRender();
        }

        if (data.Flags.HasFlag(SpriteDrawFlags.DrawOrigin))
        {
            var srcRect = new Rectangle(0.0f, 0.0f, bullseye.Width, bullseye.Height);
            // x and y will be the destination rect for the main sprite's X and Y, then adjust for the
            // origin, then adjust by 1/2 the destination drawing size...

            var destRect = new Rectangle(destinationRectangle.X, destinationRectangle.Y, bullseye.Width * data.Scale, bullseye.Height * data.Scale);

            // determine actual scale of sprite to original
            var spriteScale = destinationRectangle.Width / textureSourceRectangle.Width;

            // adjust the draw x and y to reflect the "origin" set by GMS2
            destRect.X += ((data.Origin.X * spriteScale) - (destRect.Width / 2.0f));
            destRect.Y += ((data.Origin.Y * spriteScale) - (destRect.Height / 2.0f));

            Raylib.DrawTexturePro(bullseye, srcRect, destRect, Vector2.Zero, 0.0f, Color.White);
        }


        //return new Vector2(destinationRectangle.x, destinationRectangle.y);
        return new AnimatedSpriteReturnDataModel
        {
            CurrentFrame = currentTotalFrame,
            DrawOrigin = new Vector2(destinationRectangle.X, destinationRectangle.Y),
            ScaledDrawSize = new Vector2(destinationRectangle.Width, destinationRectangle.Height)
        };
    }

    public AnimatedSpriteReturnDataModel DrawSpecificFrameSpriteToScreen(SpriteDrawDataModel data, bool showingMove = false, int totalFrames = 0, List<int> windows = null, bool resetAnimation = false)
    {
        data.Origin = data.SpriteData != null ? new Vector2(data.SpriteData.Sequence.xorigin, data.SpriteData.Sequence.yorigin) : Vector2.Zero;

        if (!showingMove)
        {
            if (data.FrameAdvance == FrameAdvance.Forward)
            {
                currentAnimationFrame++;
                currentTotalFrame++;

                if (currentAnimationFrame >= data.SpriteData.Frames.Count())
                {
                    currentAnimationFrame = 0;
                    currentTotalFrame = 1;
                }
            }
            else if (data.FrameAdvance == FrameAdvance.Backward)
            {
                currentAnimationFrame--;
                currentTotalFrame--;

                if (currentAnimationFrame < 0)
                {
                    currentAnimationFrame = data.SpriteData.Frames.Count() - 1;
                    currentTotalFrame = data.SpriteData.Frames.Count();
                }
            }
        }
        else
        {
            if (resetAnimation)
            {
                currentAnimationFrame = 0;
                currentTotalFrame = 1;
            }

            if (data.FrameAdvance == FrameAdvance.Forward)
            {
                if (windows.Count > 0)
                {
                    currentTotalFrame++;
                    if (currentTotalFrame > totalFrames)
                    {
                        currentTotalFrame = 1;
                    }
                    currentAnimationFrame = windows[currentTotalFrame];
                }
            }
            else if (data.FrameAdvance == FrameAdvance.Backward)
            {
                if (windows.Count > 0)
                {
                    currentTotalFrame--;
                    if (currentTotalFrame <= 0)
                    {
                        currentTotalFrame = totalFrames;
                    }
                    currentAnimationFrame = windows[currentTotalFrame];
                }
            }
        }

        return DrawSprite(data);
    }

    public AnimatedSpriteReturnDataModel DrawSpriteToScreen(SpriteDrawDataModel data, bool showingMove = false, int totalFrames = 0, List<int> windows = null, bool resetAnimation = false)
    {
        data.Origin = data.SpriteData != null ? new Vector2(data.SpriteData.Sequence.xorigin, data.SpriteData.Sequence.yorigin) : Vector2.Zero;

        if (!showingMove)
        {
            if (data.SpriteData == null)
            {
                if (spriteTextures == null)
                {
                    spriteTextures = new List<LoadedTextureModel>();
                }

                var textureFullPath = Path.Combine(AppContext.BaseDirectory, data.DefaultTexture);
                if (!spriteTextures.Any() || textureFullPath != spriteTextures.First().TexturePath)
                {
                    spriteTextures = new List<LoadedTextureModel> { new(textureFullPath) };
                    currentAnimationFrame = 0;
                    currentTotalFrame = 1;
                    frameCounter = 0;
                }
            }
            else
            {
                currentSprite = data.SpriteData.Name;

                if (previousSprite != currentSprite)
                {
                    previousSprite = currentSprite;
                    currentAnimationFrame = 0;
                    currentTotalFrame = 1;
                    frameCounter = 0;

                    if (data.EnableFrameDataDraw)
                    {
                        nextFrameAdvance = data.FrameDrawData.GetFrameToDraw(frameCounter).Length;
                    }
                    else
                    {
                        nextFrameAdvance = (int)data.SpriteData.Sequence.playbackSpeed == 0 ? 10 : 60 / (int)data.SpriteData.Sequence.playbackSpeed;
                    }
                    frameCounter = 0;
                    spriteTextures = new List<LoadedTextureModel>();

                    //load all textures now...
                    var spriteDataPathFragments = data.SpriteData.FilePath.Split(new string[] { "/", "\\" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var sequenceItem in data.SpriteData.Sequence.tracks[0].keyframes.Frames)
                    {
                        var spriteImagePath = data.SpriteData.FilePath.Replace(spriteDataPathFragments.Last(), string.Empty);
                        var frameData = data.SpriteData.Frames.Where(x => x.name == sequenceItem.Channels._0.Id.name).FirstOrDefault();

                        if (frameData == null)
                        {
                            data.Logger.LogError($"Frame data not found for current frame {currentAnimationFrame}");
                        }

                        spriteImagePath = Path.Combine(spriteImagePath, frameData.name + ".png");

                        if (!File.Exists(spriteImagePath))
                        {
                            data.Logger.LogError("unable to open file for frame data");
                        }

                        spriteTextures.Add(new LoadedTextureModel(spriteImagePath));
                    }
                }

                if (!data.Flags.HasFlag(SpriteDrawFlags.Pause))
                {
                    frameCounter++;

                    if (frameCounter >= nextFrameAdvance)
                    {
                        if (data.EnableFrameDataDraw)
                        {
                            var frameDataToDraw = data.FrameDrawData.GetFrameToDraw(frameCounter);

                            if (frameDataToDraw == null)
                            {
                                var firstFrameData = data.FrameDrawData.OrderBy(x => x.ImageIndex).FirstOrDefault();

                                nextFrameAdvance = (firstFrameData == null) ? 1 : firstFrameData.Length;
                            }
                            else
                            {
                                nextFrameAdvance = frameDataToDraw.Length;
                            }
                        }
                        else
                        {
                            frameCounter = 0;
                        }

                        currentAnimationFrame++;
                        currentTotalFrame++;

                        if (currentAnimationFrame >= data.SpriteData.Frames.Count())
                        {
                            currentAnimationFrame = 0;
                            currentTotalFrame = 1;

                            if (data.EnableFrameDataDraw)
                            {
                                frameCounter = 0;
                            }
                        }
                    }
                }
            }
        }
        else
        {
            if (resetAnimation)
            {
                currentAnimationFrame = 0;
                currentTotalFrame = 1;
                frameCounter = 0;
            }

            if (data.SpriteData == null)
            {
                if (spriteTextures == null)
                {
                    spriteTextures = new List<LoadedTextureModel>();
                }

                var textureFullPath = Path.Combine(AppContext.BaseDirectory, data.DefaultTexture);
                if (!spriteTextures.Any() || textureFullPath != spriteTextures.First().TexturePath)
                {
                    spriteTextures = new List<LoadedTextureModel> { new(textureFullPath) };
                    currentAnimationFrame = 0;
                    currentTotalFrame = 1;
                }
            }
            else
            {
                currentSprite = data.SpriteData.Name;

                if (previousSprite != currentSprite)
                {
                    previousSprite = currentSprite;
                    currentAnimationFrame = 0;

                    spriteTextures = new List<LoadedTextureModel>();

                    //load all textures now...
                    var spriteDataPathFragments = data.SpriteData.FilePath.Split(new string[] { "/", "\\" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var sequenceItem in data.SpriteData.Sequence.tracks[0].keyframes.Frames)
                    {
                        var spriteImagePath = data.SpriteData.FilePath.Replace(spriteDataPathFragments.Last(), string.Empty);
                        var frameData = data.SpriteData.Frames.Where(x => x.name == sequenceItem.Channels._0.Id.name).FirstOrDefault();

                        if (frameData == null)
                        {
                            data.Logger.LogError($"Frame data not found for current frame {currentAnimationFrame}");
                        }

                        spriteImagePath = Path.Combine(spriteImagePath, frameData.name + ".png");

                        if (!File.Exists(spriteImagePath))
                        {
                            data.Logger.LogError("unable to open file for frame data");
                        }

                        spriteTextures.Add(new LoadedTextureModel(spriteImagePath));
                    }
                }

                if (!data.Flags.HasFlag(SpriteDrawFlags.Pause))
                {
                    if (windows.Count > 0)
                    {
                        currentTotalFrame++;
                        if (currentTotalFrame > totalFrames)
                        {
                            currentTotalFrame = 1;
                        }
                        currentAnimationFrame = windows[currentTotalFrame];
                    }
                }
            }
        }

        return DrawSprite(data);
    }
}
