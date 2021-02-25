using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Raycasting_Engine_CSharp
{
    public class Animation
    {
        public List<Byte[]> frames = new List<byte[]>();
        public int frameWidth;
        public int frameHeight;
        public int speed;
        public bool loop;
        public Animation(Bitmap spriteSheet, int frameWidth, int frameHeight, int framesPerRow, int framesPerColumn, int framesTotal, int speed, bool loop)
        {
            this.frameWidth = frameWidth;
            this.frameHeight = frameHeight;
            this.speed = speed;
            this.loop = loop;
            Byte[] sprite = DrawingUtilities.LockBitmap(spriteSheet);
            for (int y = 0; y < framesPerColumn; y++)
            {
                for (int x = 0; x < framesPerRow; x++)
                {
                    if (frames.Count < framesTotal) frames.Add(DrawingUtilities.CopySection(sprite, spriteSheet.Width, spriteSheet.Height, x * frameWidth, y * frameHeight, frameWidth, frameHeight));
                }
            }
        }
    }
}
