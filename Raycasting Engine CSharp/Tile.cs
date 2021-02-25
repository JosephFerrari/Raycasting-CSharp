using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Raycasting_Engine_CSharp
{
    public class Tile
    {
        public int ID; // Tile type identification
        public bool solid = false; // Collision
        public Byte[] texture; // Current texture
        public Byte[] textureDefault; // Texture by default
        public double hOffset = 0; // Sideways texture offset
        public double vOffset = 0; // Upwards texture offset
        public double pushOffset = 0; // Push texture offset
        public double pushMax = 1; // Maximum push offset;
        public DoorType doorType = DoorType.Static; // Type of door
        public int status = 0; // Opening / closing / static
        public bool keyRequired = false; // Key required
        public bool transparent = false; // Texture includes transparency
        public Tile(int ID, Bitmap textureDefault, double pushOffset, DoorType doorType, bool keyRequired)
        {
            this.ID = ID;
            if (textureDefault != null) { this.textureDefault = DrawingUtilities.LockBitmap(textureDefault); solid = true; }
            else { this.textureDefault = new Byte[0]; }
            this.texture = this.textureDefault;
            this.pushOffset = pushOffset;
            this.doorType = doorType;
            this.keyRequired = keyRequired;
        }
        public void Update()
        {
            if (keyRequired) { return; }
            System.Media.SoundPlayer mediaPlayerPushWallStop = new System.Media.SoundPlayer(Properties.Resources.PushWallStop);
            if (status != 0) switch (doorType)
                {
                    case DoorType.HorizontalLeft: // Left sliding horizontal door
                        if (status != 0) hOffset += status * 0.015;
                        if (hOffset >= 1) { hOffset = 1; status = 0; solid = false; texture = new Byte[0]; break; }
                        if (hOffset <= -1) { hOffset = -1; status = 0; solid = false; texture = new Byte[0]; break; }
                        if (hOffset == 0) { hOffset = 0; status = 0; solid = true; texture = textureDefault; break; }
                        break;
                    case DoorType.HorizontalRight: // Right sliding horizontal door
                        if (status != 0) hOffset += status * 0.015;
                        if (hOffset >= 1) { hOffset = 1; status = 0; solid = false; texture = new Byte[0]; break; }
                        if (hOffset <= -1) { hOffset = -1; status = 0; solid = false; texture = new Byte[0]; break; }
                        if (hOffset == 0) { hOffset = 0; status = 0; solid = true; texture = textureDefault; break; }
                        break;
                    case DoorType.VerticalDown: // Downwards sliding vertical door
                        if (status != 0) vOffset += status * 0.015;
                        if (vOffset >= 1) { vOffset = 1; status = 0; solid = false; texture = new Byte[0]; }
                        if (vOffset <= -1) { vOffset = -1; status = 0; solid = false; texture = new Byte[0]; }
                        if (vOffset == 0) { vOffset = 0; status = 0; solid = true; texture = textureDefault; }
                        break;
                    case DoorType.VerticalUp: // Upwards sliding vertical door
                        if (status != 0) vOffset += status * 0.015;
                        if (vOffset >= 1) { vOffset = 1; status = 0; solid = false; texture = new Byte[0]; }
                        if (vOffset <= -1) { vOffset = -1; status = 0; solid = false; texture = new Byte[0]; }
                        if (vOffset == 0) { vOffset = 0; status = 0; solid = true; texture = textureDefault; }
                        break;
                    case DoorType.PushShort: // Short distance push wall
                        if (status != 0) pushOffset += status * 0.02;
                        if (pushOffset >= 1) { pushOffset = 1; status = 0; solid = false; texture = new Byte[0]; mediaPlayerPushWallStop.Play(); }
                        if (pushOffset == 0) { pushOffset = 0; status = 0; solid = true; texture = textureDefault; mediaPlayerPushWallStop.Play(); }
                        break;
                    case DoorType.PushLong: // Long distance push wall
                        if (status != 0) pushOffset += status * 0.02;
                        if (pushOffset >= 2) { pushOffset = 2; status = 0; solid = false; texture = new Byte[0]; mediaPlayerPushWallStop.Play(); }
                        if (pushOffset == 0) { pushOffset = 0; status = 0; solid = true; texture = textureDefault; mediaPlayerPushWallStop.Play(); }
                        break;
                }
        }
        public bool Interact(ref Player playerControl)
        {
            Entity player = playerControl.entity;
            System.Media.SoundPlayer mediaPlayerKey = new System.Media.SoundPlayer(Properties.Resources.KeySound);
            if ((keyRequired) && (playerControl.keysHeld > 0)) { keyRequired = false; playerControl.keysHeld--; mediaPlayerKey.Play(); }
            if ((!keyRequired) && (status == 0))
            {
                System.Media.SoundPlayer mediaPlayerDoorOpen = new System.Media.SoundPlayer(Properties.Resources.DoorOpen);
                System.Media.SoundPlayer mediaPlayerPushWallStart = new System.Media.SoundPlayer(Properties.Resources.PushWallStart);
                int direction;
                switch (doorType)
                {
                    case DoorType.HorizontalLeft: // Left sliding horizontal door
                        if (playerControl.targetedSide == 0)
                        {
                            if (player.xPos > playerControl.targetedX) direction = 1;
                            else direction = -1;
                        }
                        else
                        {
                            if (player.yPos < playerControl.targetedY) direction = 1;
                            else direction = -1;
                        }
                        if (hOffset <= -1) status = 1 * direction;
                        if (hOffset >= 0) status = -1 * direction;
                        mediaPlayerDoorOpen.Play();
                        return true;
                    case DoorType.HorizontalRight: // Right sliding horizontal door
                        if (playerControl.targetedSide == 0)
                        {
                            if (player.xPos > playerControl.targetedX) direction = -1;
                            else direction = 1;
                        }
                        else
                        {
                            if (player.yPos < playerControl.targetedY) direction = -1;
                            else direction = 1;
                        }
                        if (hOffset >= 1) status = 1 * direction;
                        if (hOffset <= 0) status = -1 * direction;
                        mediaPlayerDoorOpen.Play();
                        return true;
                    case DoorType.VerticalDown: // Downwards sliding vertical door
                        status = -1;
                        mediaPlayerDoorOpen.Play();
                        return true;
                    case DoorType.VerticalUp: // Upwards sliding vertical door
                        status = 1;
                        mediaPlayerDoorOpen.Play();
                        return true;
                    case DoorType.PushShort: // Short distance push wall
                        status = 1;
                        mediaPlayerPushWallStart.Play();
                        return true;
                    case DoorType.PushLong: // Long distance push wall
                        status = 1;
                        if ((playerControl.targetedSide == 1) && (player.yPos > playerControl.targetedY) && (player.currentBoard.BoardPoint(playerControl.targetedX, playerControl.targetedY - 1).solid != false)) doorType = DoorType.PushShort;
                        if ((playerControl.targetedSide == 1) && (player.yPos < playerControl.targetedY) && (player.currentBoard.BoardPoint(playerControl.targetedX, playerControl.targetedY + 1).solid != false)) doorType = DoorType.PushShort;
                        if ((playerControl.targetedSide == 0) && (player.xPos > playerControl.targetedX) && (player.currentBoard.BoardPoint(playerControl.targetedX - 1, playerControl.targetedY).solid != false)) doorType = DoorType.PushShort;
                        if ((playerControl.targetedSide == 0) && (player.xPos < playerControl.targetedX) && (player.currentBoard.BoardPoint(playerControl.targetedX + 1, playerControl.targetedY).solid != false)) doorType = DoorType.PushShort;
                        mediaPlayerPushWallStart.Play();
                        return true;
                }
            }
            return false;
        }
    }
}
