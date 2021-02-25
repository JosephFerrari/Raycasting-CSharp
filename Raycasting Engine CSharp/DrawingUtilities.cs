using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Raycasting_Engine_CSharp
{
    public static class DrawingUtilities
    {
        public static void DrawSprite(ref Byte[] drawTarget, int stride, int width, int height, Byte[] texture, int xPos, int yPos, int texWidth, int texHeight, AlignH alignH, AlignV alignV)
        {
            switch (alignH)
            {
                case AlignH.Center: xPos -= texWidth / 2; break;
                case AlignH.Right: xPos -= texWidth; break;
            }
            switch (alignV)
            {
                case AlignV.Center: yPos -= texHeight / 2; break;
                case AlignV.Bottom: yPos -= texHeight; break;
            }
            for (int x = Math.Max(xPos, 0); x < Math.Min(xPos + texWidth, width); x++)
            {
                for (int y = Math.Max(yPos, 0); y < Math.Min(yPos + texHeight, height); y++)
                {
                    Color texel = DrawingUtilities.TexturePixel(texture, x - xPos, y - yPos, texWidth, texHeight);
                    if (texel != Color.FromArgb(236, 84, 220)) DrawingUtilities.DrawPixel(ref drawTarget, x, y, stride, texel, 255, false);
                }
            }
        }
        public static Byte[] DrawSpriteScaled(Byte[] drawTarget, int stride, int witdh, int height, Byte[] texture, int xPos, int yPos, double xScale, double yScale, int texWidth, int texHeight, AlignH alignH, AlignV alignV)
        {
            int scaledTexWidth = Convert.ToInt32(texWidth * xScale);
            int scaledTexHeight = Convert.ToInt32(texHeight * yScale);
            switch (alignH)
            {
                case AlignH.Center: xPos -= scaledTexWidth / 2; break;
                case AlignH.Right: xPos -= scaledTexWidth; break;
            }
            switch (alignV)
            {
                case AlignV.Center: yPos -= scaledTexHeight / 2; break;
                case AlignV.Bottom: yPos -= scaledTexHeight; break;
            }
            Parallel.For(Math.Max(xPos, 0), Math.Min(xPos + scaledTexWidth, witdh), x =>
            {
                Parallel.For(Math.Max(yPos, 0), Math.Min(yPos + scaledTexHeight, height), y =>
                {
                    Color texel = DrawingUtilities.TexturePixel(texture, Convert.ToInt32((x - xPos) / xScale), Convert.ToInt32((y - yPos) / yScale), texWidth, texHeight);
                    if (texel != Color.FromArgb(236, 84, 220)) DrawingUtilities.DrawPixel(ref drawTarget, x, y, stride, texel, 255, false);
                });
            });
            return drawTarget;
        }
        public static void DrawPixel(ref Byte[] texture, int texX, int texY, int stride, Color texel, int alpha, bool draw)
        {
            alpha = Convert.ToInt32(MathUtilities.Clamp(alpha, 0, 255));
            if ((!draw) || (texture[Convert.ToInt32((texX * 4) + (texY * stride) + 3)] == 0) || (texture[Convert.ToInt32((texX * 4) + (texY * stride) + 3)] == 254))
            {
                texture[Convert.ToInt32(MathUtilities.Clamp((texX * 4) + (texY * stride) + 0, 0, texture.Length - 1))] = Convert.ToByte(texel.B);
                texture[Convert.ToInt32(MathUtilities.Clamp((texX * 4) + (texY * stride) + 1, 0, texture.Length - 1))] = Convert.ToByte(texel.G);
                texture[Convert.ToInt32(MathUtilities.Clamp((texX * 4) + (texY * stride) + 2, 0, texture.Length - 1))] = Convert.ToByte(texel.R);
                texture[Convert.ToInt32(MathUtilities.Clamp((texX * 4) + (texY * stride) + 3, 0, texture.Length - 1))] = Convert.ToByte(alpha);
            }
        }
        public static Byte[] DrawTextOutline(Byte[] drawTarget, int stride, int width, int height, string text, int xPos, int yPos, FontSheet font, Color textColor, int spacing, int thickness, int resolution)
        {
            Parallel.For(0, 360 / resolution, angle => { DrawingUtilities.DrawText(ref drawTarget, stride, width, height, text, xPos + Convert.ToInt32(thickness * Math.Cos(angle * resolution)), yPos + Convert.ToInt32(thickness * Math.Sin(angle * resolution)), font, textColor, spacing); });
            //for (int angle = 0; angle < 360; angle += resolution)
            return drawTarget;
        }
        public static void DrawText(ref Byte[] drawTarget, int stride, int width, int height, string text, int xPos, int yPos, FontSheet font, Color textColor, int spacing)
        {
            for (int pos = 0; pos < text.Length; pos++)
            {
                char currentCharValue = Convert.ToChar(text.Substring(pos, 1));
                if (font.textValues.Contains(currentCharValue))
                {
                    byte[] currentChar = font.Character(Array.IndexOf(font.textValues, currentCharValue));
                    DrawCharacter(ref drawTarget, stride, width, height, currentChar, xPos + ((font.charWidth + spacing) * pos), yPos, font.charWidth, font.charHeight, textColor);
                }
            }
        }
        public static string FormatNumer(int number)
        {
            string text = number.ToString();
            int length = text.Length;
            for (int i = 0; i < Math.Floor((length - 1) / 3.0); i++) { text = text.Insert(text.Length - ((i + 1) * 3) - i, ","); }
            return text;
        }
        public static void DrawCharacter(ref Byte[] drawTarget, int stride, int width, int height, Byte[] texture, int xPos, int yPos, int texWidth, int texHeight, Color textColor)
        {
            if (texture == null) return;
            for (int x = Math.Max(xPos, 0); x < Math.Min(xPos + texWidth, width); x++)
            {
                for (int y = Math.Max(yPos, 0); y < Math.Min(yPos + texHeight, height); y++)
                {
                    Color texel = DrawingUtilities.TexturePixel(texture, x - xPos, y - yPos, texWidth, texHeight);
                    if (texel != Color.FromArgb(236, 84, 220)) DrawingUtilities.DrawPixel(ref drawTarget, x, y, stride, textColor, 255, false);
                }
            }
        }
        public static Byte[] DiscolorSprite(Byte[] texture, int texWidth, int texHeight, double multiplier, double saturation)
        {
            Parallel.For(0, texWidth, xPos =>
            {
                Parallel.For(0, texHeight, yPos =>
                {
                    DrawingUtilities.DrawPixel(ref texture, xPos, yPos, texWidth * 4, ColorUtilities.Desaturate(ColorUtilities.Multiply(DrawingUtilities.TexturePixel(texture, xPos, yPos, texWidth, texHeight), multiplier), saturation), 255, false);
                });
            });
            return texture;
        }
        public static Byte[] LockBitmap(Bitmap targetBitmap)
        {
            Rectangle bounds = new Rectangle(0, 0, targetBitmap.Width, targetBitmap.Height);
            System.Drawing.Imaging.BitmapData bmpData = targetBitmap.LockBits(bounds, System.Drawing.Imaging.ImageLockMode.ReadWrite, targetBitmap.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * targetBitmap.Height;
            Byte[] rgbValues = new Byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
            return rgbValues;
        }
        public static Color TexturePixel(Byte[] texture, int texX, int texY, int texWidth, int texHeight)
        {
            if (texture == null) return Color.Orange;
            int cPos = (Convert.ToInt32(MathUtilities.Clamp(texX, 0, texWidth - 1)) * 4) + ((Convert.ToInt32(MathUtilities.Clamp(texY, 0, texHeight - 1))) * texWidth * 4);
            return Color.FromArgb(texture[cPos + 3], texture[cPos + 2], texture[cPos + 1], texture[cPos + 0]);
        }
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);
            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }
        public static Byte[] CopySection(Byte[] sprite, int spriteWidth, int spriteHeight, int xPos, int yPos, int width, int height)
        {
            Bitmap startingBitmap = new Bitmap(width, height);
            Byte[] section = DrawingUtilities.LockBitmap(startingBitmap);
            for (int x = xPos; x < xPos + width; x++)
            {
                for (int y = yPos; y < yPos + height; y++)
                {
                    DrawingUtilities.DrawPixel(ref section, x - xPos, y - yPos, width * 4, DrawingUtilities.TexturePixel(sprite, x, y, spriteWidth, spriteHeight), 255, false);
                }
            }
            return section;
        }
    }
}
