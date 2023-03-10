using CharacterDataEditor.Constants;
using CharacterDataEditor.Enums;
using CharacterDataEditor.Extensions;
using CharacterDataEditor.Models;
using CharacterDataEditor.Models.CharacterData;
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

        private AnimatedSpriteReturnDataModel DrawSprite(Vector2 drawPosition, Vector2 origin, float scale, Vector2 maxDrawSize, SpriteDrawFlags flags, PaletteModel baseColor = null, PaletteModel swapColor = null)
        {
            Texture2D textureToDraw;

            if (currentAnimationFrame > spriteTextures.Count - 1)
            {
                currentAnimationFrame = 0;
            }

            if (flags.HasFlag(SpriteDrawFlags.NotAnimated))
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
            destinationRectangle.width = (textureToDraw.width * 3) * scale;
            destinationRectangle.height = (textureToDraw.height * 3) * scale;

            //check if sprite destination is above max size, if so... determine the scale between x and y, and adjust the larger to the bounds
            // and the smaller to be scaled appropriately

            Vector2 Rescale(float w, float h, Vector2 max)
            {
                var widthOver = w - max.X;
                var heightOver = h - maxDrawSize.Y;

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
                heightOver = h - maxDrawSize.Y;

                if (widthOver > 0 || heightOver > 0)
                {
                    return Rescale(w, h, max);
                }

                return new Vector2(w, h);
            }

            if (maxDrawSize != Vector2.Zero && (destinationRectangle.width > maxDrawSize.X || destinationRectangle.height > maxDrawSize.Y))
            {
                maxDrawSize *= scale;
                var newWH = Rescale(destinationRectangle.width, destinationRectangle.height, maxDrawSize);

                destinationRectangle.width = newWH.X;
                destinationRectangle.height = newWH.Y;
            }

            if (flags.HasFlag(SpriteDrawFlags.CenterHorizontal))
            {
                var centerHorizontal = ClientWindow.X / 2 - (destinationRectangle.width / 2);
                destinationRectangle.x = centerHorizontal;
            }
            else
            {
                destinationRectangle.x = drawPosition.X * scale;
            }

            if (flags.HasFlag(SpriteDrawFlags.CenterVertical))
            {
                var centerVertical = ClientWindow.Y / 2 - (destinationRectangle.height / 2);
                destinationRectangle.y = centerVertical;
            }
            else
            {
                destinationRectangle.y = drawPosition.Y * scale;
            }

            if (flags.HasFlag(SpriteDrawFlags.ShowSpriteOutline))
            {
                Raylib.DrawRectangle((int)destinationRectangle.x, (int)destinationRectangle.y, (int)destinationRectangle.width, (int)destinationRectangle.height, Color.BLACK);
            }

            if (flags.HasFlag(SpriteDrawFlags.PaletteSwapActive) && baseColor?.ColorPalette?.Count > 0 && swapColor?.ColorPalette?.Count > 0)
            {
                ShaderHelper.ShaderStartRender();

                var baseColorArray = baseColor.ToShaderVec4Array();
                var swapColorArray = swapColor.ToShaderVec4Array();

                for (int i = 0; i < baseColorArray.Length; i++)
                {
                    ShaderHelper.SetValue($"basecolor{i}", baseColorArray[i], ShaderUniformDataType.SHADER_UNIFORM_VEC4);
                    ShaderHelper.SetValue($"swapcolor{i}", swapColorArray[i], ShaderUniformDataType.SHADER_UNIFORM_VEC4);
                }
            }

            // Origin determines where everything is based, passing 0x0y keeps it default
            // Color.White is used to not tint the texture at all
            Raylib.DrawTexturePro(textureToDraw, textureSourceRectangle, destinationRectangle, Vector2.Zero, 0.0f, Color.WHITE);

            if (flags.HasFlag(SpriteDrawFlags.PaletteSwapActive))
            {
                ShaderHelper.ShaderEndRender();
            }

            if (flags.HasFlag(SpriteDrawFlags.DrawOrigin))
            {
                var srcRect = new Rectangle(0.0f, 0.0f, bullseye.width, bullseye.height);
                // x and y will be the destination rect for the main sprite's X and Y, then adjust for the
                // origin, then adjust by 1/2 the destination drawing size...

                var destRect = new Rectangle(destinationRectangle.x, destinationRectangle.y, bullseye.width * scale, bullseye.height * scale);

                // determine actual scale of sprite to original
                var spriteScale = destinationRectangle.width / textureSourceRectangle.width;

                // adjust the draw x and y to reflect the "origin" set by GMS2
                destRect.x += ((origin.X * spriteScale) - (destRect.width / 2.0f));
                destRect.y += ((origin.Y * spriteScale) - (destRect.height / 2.0f));

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

        public AnimatedSpriteReturnDataModel DrawSpecificFrameSpriteToScreen(SpriteDataModel spriteData, PaletteModel basePalette, PaletteModel paletteData, Vector2 drawPosition, float scale, ILogger logger, Vector2 maxDrawSize, FrameAdvance frameAdvance, SpriteDrawFlags flags = SpriteDrawFlags.None)
        {
            var origin = spriteData != null ? new Vector2(spriteData.Sequence.xorigin, spriteData.Sequence.yorigin) : Vector2.Zero;


            if (frameAdvance == FrameAdvance.Forward)
            {
                currentAnimationFrame++;

                if (currentAnimationFrame >= spriteData.Frames.Count())
                {
                    currentAnimationFrame = 0;
                }
            }
            else if (frameAdvance == FrameAdvance.Backward)
            {
                currentAnimationFrame--;

                if (currentAnimationFrame < 0)
                {
                    currentAnimationFrame = spriteData.Frames.Count() - 1;
                }
            }

            return DrawSprite(drawPosition, origin, scale, maxDrawSize, flags, basePalette, paletteData);
        }

        public AnimatedSpriteReturnDataModel DrawSpriteToScreen(SpriteDataModel spriteData, PaletteModel basePalette, PaletteModel paletteData, Vector2 drawPosition, float scale, string defaultTexture, ILogger logger, Vector2 maxDrawSize, SpriteDrawFlags flags = SpriteDrawFlags.None)
        {
            var origin = spriteData != null ? new Vector2(spriteData.Sequence.xorigin, spriteData.Sequence.yorigin) : Vector2.Zero;

            if (spriteData == null)
            {
                if (spriteTextures == null)
                {
                    spriteTextures = new List<LoadedTextureModel>();
                }

                var textureFullPath = Path.Combine(AppContext.BaseDirectory, defaultTexture);
                if (!spriteTextures.Any() || textureFullPath != spriteTextures.First().TexturePath)
                {
                    spriteTextures = new List<LoadedTextureModel> { new LoadedTextureModel(textureFullPath) };
                    currentAnimationFrame = 0;
                    frameCounter = 0;
                }
            }
            else
            {
                currentSprite = spriteData.Name;

                if (previousSprite != currentSprite)
                {
                    previousSprite = currentSprite;
                    currentAnimationFrame = 0;
                    nextFrameAdvance = (int)spriteData.Sequence.playbackSpeed == 0 ? 10 : 60 / (int)spriteData.Sequence.playbackSpeed;
                    frameCounter = 0;
                    spriteTextures = new List<LoadedTextureModel>();

                    //load all textures now...

                    var spriteDataPathFragments = spriteData.FilePath.Split(new string[] { "/", "\\" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var sequenceItem in spriteData.Sequence.tracks[0].keyframes.Frames)
                    {
                        var spriteImagePath = spriteData.FilePath.Replace(spriteDataPathFragments.Last(), string.Empty);
                        var frameData = spriteData.Frames.Where(x => x.name == sequenceItem.Channels._0.Id.name).FirstOrDefault();

                        if (frameData == null)
                        {
                            logger.LogError($"Frame data not found for current frame {currentAnimationFrame}");
                        }

                        spriteImagePath = Path.Combine(spriteImagePath, frameData.name + ".png");

                        if (!File.Exists(spriteImagePath))
                        {
                            logger.LogError("unable to open file for frame data");
                        }

                        spriteTextures.Add(new LoadedTextureModel(spriteImagePath));
                    }
                }

                if (!flags.HasFlag(SpriteDrawFlags.Pause))
                {
                    frameCounter++;

                    if (frameCounter >= nextFrameAdvance)
                    {
                        frameCounter = 0;
                        currentAnimationFrame++;

                        if (currentAnimationFrame >= spriteData.Frames.Count())
                        {
                            currentAnimationFrame = 0;
                        }
                    }
                }
            }

            return DrawSprite(drawPosition, origin, scale, maxDrawSize, flags, basePalette, paletteData);
        }
    }
}
