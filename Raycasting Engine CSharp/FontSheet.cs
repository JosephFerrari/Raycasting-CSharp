using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Raycasting_Engine_CSharp
{
    public class FontSheet
    {
        public byte[][] characters;
        public int charWidth;
        public int charHeight;
        public char[] textValues;
        public FontSheet(Bitmap fontSource, int charWidth, int charHeight, int charactersHorizontally, int charactersVertically, char[] textValues)
        {
            characters = new byte[textValues.Length][];
            Byte[] fontLocked = DrawingUtilities.LockBitmap(fontSource);
            this.charWidth = charWidth;
            this.charHeight = charHeight;
            this.textValues = textValues;
            for (int x = 0; x < charactersHorizontally; x++)
            {
                for (int y = 0; y < charactersVertically; y++)
                {
                    if (x + (y * charactersHorizontally) < textValues.Length) characters[x + (y * charactersHorizontally)] = DrawingUtilities.CopySection(fontLocked, fontSource.Width, fontSource.Height, x * charWidth, y * charHeight, charWidth, charHeight);
                }
            }
        }
        public byte[] Character(int character)
        {
            return characters[character];
        }
        public int GetStride(Bitmap targetBitmap)
        {
            Rectangle bounds = new Rectangle(0, 0, targetBitmap.Width, targetBitmap.Height);
            System.Drawing.Imaging.BitmapData bmpData = targetBitmap.LockBits(bounds, System.Drawing.Imaging.ImageLockMode.ReadWrite, targetBitmap.PixelFormat);
            return bmpData.Stride;
        }
    }
}
