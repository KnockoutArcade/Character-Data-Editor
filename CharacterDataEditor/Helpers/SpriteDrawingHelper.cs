using CharacterDataEditor.Constants;
using CharacterDataEditor.Enums;
using CharacterDataEditor.Extensions;
using CharacterDataEditor.Models;
using Microsoft.Extensions.Logging;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace CharacterDataEditor.Helpers
{
    public class SpriteDrawingHelper
    {
        private int currentAnimationFrame = 0;
        private int frameCounter = 0;
        private int nextFrameAdvance = 0;
        private string previousSprite = string.Empty;
        private string currentSprite = string.Empty;

        private List<LoadedTextureModel> spriteTextures;
        private Texture2D bullseye;

        private Vector2 ClientWindow
        {
            get
            {
                if (_client == Vector2.Zero)
                {
                    _client = HardwareHelper.GetClientWindowSize();
                }

                return _client;
            }
        }

        private Vector2 _client = Vector2.Zero;

        public SpriteDrawingHelper()
        {
            bullseye = Raylib.LoadTexture(Path.Combine(AppContext.BaseDirectory, ResourceConstants.BullseyePath));
        }

        private AnimatedSpriteReturnDataModel DrawSprite(SpriteDrawDataModel data)
        {
            Texture2D textureToDraw;

            if (currentAnimationFrame > spriteTextures.Count - 1)
            {
                currentAnimationFrame = 0;
            }

            if (data.Flags.HasFlag(SpriteDrawFlags.NotAnimated))
            {
                textureToDraw = spriteTextures.FirstOrDefault().Texture;
            }
            else
            {
                textureToDraw = spriteTextures[currentAnimationFrame].Texture;
            }

            Rectangle textureSourceRectangle = new Rectangle(0.0f, 0.0f, textureToDraw.width, textureToDraw.height);

            //destination rectangle determines the size to scale it to and the position on screen
            Rectangle destinationRectangle = new Rectangle();
            destinationRectangle.width = (textureToDraw.width * 3) * data.Scale;
            destinationRectangle.height = (textureToDraw.height * 3) * data.Scale;

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

            if (data.MaxDrawSize != Vector2.Zero && (destinationRectangle.width > data.MaxDrawSize.X || destinationRectangle.height > data.MaxDrawSize.Y))
            {
                data.MaxDrawSize *= data.Scale;
                var newWH = Rescale(destinationRectangle.width, destinationRectangle.height, data.MaxDrawSize);

                destinationRectangle.width = newWH.X;
                destinationRectangle.height = newWH.Y;
            }

            if (data.Flags.HasFlag(SpriteDrawFlags.CenterHorizontal))
            {
                var centerHorizontal = ClientWindow.X / 2 - (destinationRectangle.width / 2);
                destinationRectangle.x = centerHorizontal;
            }
            else
            {
                destinationRectangle.x = data.DrawPosition.X * data.Scale;
            }

            if (data.Flags.HasFlag(SpriteDrawFlags.CenterVertical))
            {
                var centerVertical = ClientWindow.Y / 2 - (destinationRectangle.height / 2);
                destinationRectangle.y = centerVertical;
            }
            else
            {
                destinationRectangle.y = data.DrawPosition.Y * data.Scale;
            }

            if (data.Flags.HasFlag(SpriteDrawFlags.ShowSpriteOutline))
            {
                Raylib.DrawRectangle((int)destinationRectangle.x, (int)destinationRectangle.y, (int)destinationRectangle.width, (int)destinationRectangle.height, Color.BLACK);
            }

            if (data.Flags.HasFlag(SpriteDrawFlags.PaletteSwapActive) && data.BaseColor?.ColorPalette?.Count > 0 && data.SwapColor?.ColorPalette?.Count > 0)
            {
                ShaderHelper.ShaderStartRender();

                var baseColorArray = data.BaseColor.ToShaderVec4Array();
                var swapColorArray = data.SwapColor.ToShaderVec4Array();

                for (int i = 0; i < baseColorArray.Length; i++)
                {
                    ShaderHelper.SetValue($"basecolor{i}", baseColorArray[i], ShaderUniformDataType.SHADER_UNIFORM_VEC4);
                    ShaderHelper.SetValue($"swapcolor{i}", swapColorArray[i], ShaderUniformDataType.SHADER_UNIFORM_VEC4);
                }
            }

            // Origin determines where everything is based, passing 0x0y keeps it default
            // Color.White is used to not tint the texture at all
            Raylib.DrawTexturePro(textureToDraw, textureSourceRectangle, destinationRectangle, Vector2.Zero, 0.0f, Color.WHITE);

            if (data.Flags.HasFlag(SpriteDrawFlags.PaletteSwapActive))
            {
                ShaderHelper.ShaderEndRender();
            }

            if (data.Flags.HasFlag(SpriteDrawFlags.DrawOrigin))
            {
                var srcRect = new Rectangle(0.0f, 0.0f, bullseye.width, bullseye.height);
                // x and y will be the destination rect for the main sprite's X and Y, then adjust for the
                // origin, then adjust by 1/2 the destination drawing size...

                var destRect = new Rectangle(destinationRectangle.x, destinationRectangle.y, bullseye.width * data.Scale, bullseye.height * data.Scale);

                // determine actual scale of sprite to original
                var spriteScale = destinationRectangle.width / textureSourceRectangle.width;

                // adjust the draw x and y to reflect the "origin" set by GMS2
                destRect.x += ((data.Origin.X * spriteScale) - (destRect.width / 2.0f));
                destRect.y += ((data.Origin.Y * spriteScale) - (destRect.height / 2.0f));

                Raylib.DrawTexturePro(bullseye, srcRect, destRect, Vector2.Zero, 0.0f, Color.WHITE);
            }
            

            //return new Vector2(destinationRectangle.x, destinationRectangle.y);
            return new AnimatedSpriteReturnDataModel
            {
                CurrentFrame = currentAnimationFrame,
                DrawOrigin = new Vector2(destinationRectangle.x, destinationRectangle.y),
                ScaledDrawSize = new Vector2(destinationRectangle.width, destinationRectangle.height)
            };
        }

        public AnimatedSpriteReturnDataModel DrawSpecificFrameSpriteToScreen(SpriteDrawDataModel data)
        {
            data.Origin = data.SpriteData != null ? new Vector2(data.SpriteData.Sequence.xorigin, data.SpriteData.Sequence.yorigin) : Vector2.Zero;


            if (data.FrameAdvance == FrameAdvance.Forward)
            {
                currentAnimationFrame++;

                if (currentAnimationFrame >= data.SpriteData.Frames.Count())
                {
                    currentAnimationFrame = 0;
                }
            }
            else if (data.FrameAdvance == FrameAdvance.Backward)
            {
                currentAnimationFrame--;

                if (currentAnimationFrame < 0)
                {
                    currentAnimationFrame = data.SpriteData.Frames.Count() - 1;
                }
            }

            return DrawSprite(data);
        }

        public AnimatedSpriteReturnDataModel DrawSpriteToScreen(SpriteDrawDataModel data)
        {
            data.Origin = data.SpriteData != null ? new Vector2(data.SpriteData.Sequence.xorigin, data.SpriteData.Sequence.yorigin) : Vector2.Zero;

            if (data.SpriteData == null)
            {
                if (spriteTextures == null)
                {
                    spriteTextures = new List<LoadedTextureModel>();
                }

                var textureFullPath = Path.Combine(AppContext.BaseDirectory, data.DefaultTexture);
                if (!spriteTextures.Any() || textureFullPath != spriteTextures.First().TexturePath)
                {
                    spriteTextures = new List<LoadedTextureModel> { new LoadedTextureModel(textureFullPath) };
                    currentAnimationFrame = 0;
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
                            nextFrameAdvance = (frameDataToDraw == null) ? 0 : frameDataToDraw.Length;
                        }
                        else
                        {
                            frameCounter = 0;
                        }

                        currentAnimationFrame++;

                        if (currentAnimationFrame >= data.SpriteData.Frames.Count())
                        {
                            currentAnimationFrame = 0;

                            if (data.EnableFrameDataDraw)
                            {
                                frameCounter = 0;
                            }
                        }
                    }
                }
            }

            return DrawSprite(data);
        }
    }
}
