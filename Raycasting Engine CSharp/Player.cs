using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Media;

namespace Raycasting_Engine_CSharp
{
    public class Player
    {
        public Entity entity;
        public double dsp; // Directional change
        public double hShift = 0; // Sideways momentum
        public double vShift = 0; // Forwards and backwards momentum
        public double planeX; // X plane
        public double planeY; // Y plane
        public int hpMax = 100;
        public int hpValue = 100;
        public double hpDisplay = 100;
        public double hpTrail = 100;
        public int hpWidth = 100;
        public int hpSegments = 0;
        public int weapon = 0;
        public List<Weapon> weapons = new List<Weapon>();
        public int ammoValue = 64;
        public double ammoDisplay = 64;
        public int keysHeld = 10;
        public Tile targetedTile = null;
        public int targetedSide = 0;
        public int targetedX = 0;
        public int targetedY = 0;
        public double targetedDistance = 0;
        public List<Entity> targetedEntities = new List<Entity>();
        public List<double> targetedDistances = new List<double>();
        public List<double> targetedMultipliers = new List<double>();
        public double leftHandX = 0;
        public double leftHandY = 0;
        public double leftHandHsp = 0;
        public double leftHandVsp = 0;
        public double leftHandBaseY = 0;
        public double rightHandX = 0;
        public double rightHandY = 0;
        public double rightHandHsp = 0;
        public double rightHandVsp = 0;
        public double rightHandBaseY = 0;
        public bool hand = true;
        public int punch = 0;
        public Byte[][] interfaceSprites = new Byte[0][];
        public int cooldown = 0;
        public bool active = true;
        public int damageFrames = 0;
        public Player(ref Entity entity)
        {
            this.entity = entity;
            interfaceSprites = new Byte[][] { DrawingUtilities.LockBitmap(Properties.Resources.HPBarLeft), DrawingUtilities.LockBitmap(Properties.Resources.HPBarRight), DrawingUtilities.LockBitmap(Properties.Resources.HPBarBack), DrawingUtilities.LockBitmap(Properties.Resources.HPBarRed), DrawingUtilities.LockBitmap(Properties.Resources.HPBarRedEnd), DrawingUtilities.LockBitmap(Properties.Resources.HPBarPurple), DrawingUtilities.LockBitmap(Properties.Resources.HPBarPurpleEnd), DrawingUtilities.LockBitmap(Properties.Resources.AmmoBarLeft), DrawingUtilities.LockBitmap(Properties.Resources.AmmoBarRight), DrawingUtilities.LockBitmap(Properties.Resources.AmmoBarBack), DrawingUtilities.LockBitmap(Properties.Resources.AmmoBarFull), DrawingUtilities.LockBitmap(Properties.Resources.AmmoBarEmpty) };
            weapons.Add(new Weapon("Fists", this, 0, 0, 0, false, 0, 10, 0.3, 2, EntityType.None, 40, 40, 8, new SoundPlayer(Properties.Resources.Punch), null, true));
            weapons.Add(new Weapon("Pistol", this, 1, 16, 4, false, 4, 20, 0.3, 0, EntityType.None, 20, 20, 4, new SoundPlayer(Properties.Resources.Pistol), null, false));
            weapons.Add(new Weapon("Assault Rifle", this, 1, 20, 5, true, 2, 30, 0.2, 0, EntityType.None, 20, 20, 6, new SoundPlayer(Properties.Resources.Pistol), null, false));
            weapons.Add(new Weapon("Shotgun", this, 2, 2, 1, false, 10, 60, 0.5, 3, EntityType.None, 20, 20, 6, new SoundPlayer(Properties.Resources.Pistol), null, false));
            weapons.Add(new Weapon("Minigun", this, 1, 24, 2, true, 4, 40, 0.2, 0, EntityType.None, 20, 20, 2, new SoundPlayer(Properties.Resources.Pistol), null, false));
            //weapons.Add(new Weapon("Rocket Launcher", this, 0, 0, 0, false, 0, 10, 1, 0, EntityType.Rocket, 40, 40, 8, new SoundPlayer(Properties.Resources.Punch), null, false));
            keysHeld = 0;
        }
        public void HandleMovement(double mouseX, double delta)
        {
            if (mouseX / 200 != 0) dsp = (mouseX / 200) * delta;
            else dsp *= 0.25;
            entity.dir += dsp;
            planeX = Math.Cos(entity.dir + (Math.PI / 2)) * 0.45;
            planeY = Math.Sin(entity.dir + (Math.PI / 2)) * 0.45;
            int moveWS = Convert.ToInt32(Keyboard.IsKeyDown(Key.W)) - Convert.ToInt32(Keyboard.IsKeyDown(Key.S));
            int moveDA = Convert.ToInt32(Keyboard.IsKeyDown(Key.D)) - Convert.ToInt32(Keyboard.IsKeyDown(Key.A));
            int sprint = ((1 - Convert.ToInt32(Keyboard.IsKeyDown(Key.S))) * (1 - Convert.ToInt32(Keyboard.IsKeyDown(Key.A))) * (1 - Convert.ToInt32(Keyboard.IsKeyDown(Key.D)))) * Convert.ToInt32(Keyboard.IsKeyDown(Key.LeftShift));
            if (entity.currentBoard.environment == Environment.Ice) sprint = 0;
            entity.hsp += (((moveWS * planeY / 8) + (moveDA * planeX / 14)) / Math.Min(Math.Max(Math.Abs(moveWS) + Math.Abs(moveDA), 1), 1.5) * (entity.moveSpeed + (0.2 * sprint))) * delta;
            entity.vsp -= (((moveWS * planeX / 8) - (moveDA * planeY / 14)) / Math.Min(Math.Max(Math.Abs(moveWS) + Math.Abs(moveDA), 1), 1.5) * (entity.moveSpeed + (0.2 * sprint))) * delta;
            hShift = -moveDA * 8;
            vShift = (moveWS * (1 + sprint)) * 6;
        }
        public void HandlePlayer()
        {
            double leftSpeed = 0.2;
            double rightSpeed = 0.2;
            if (weapons[weapon].name != "Fists") { leftHandBaseY = 64; rightHandBaseY = 16; rightSpeed = 0.1; leftSpeed = 0.1; }
            else if (hand) { leftHandBaseY = 8; rightHandBaseY = 0; leftSpeed = 0.1; }
            else { leftHandBaseY = 0; rightHandBaseY = 8; rightSpeed = 0.1; }

            leftHandBaseY += vShift;
            rightHandBaseY += vShift;

            leftHandHsp = MathUtilities.Lerp(leftHandHsp, (hShift - (dsp * 50) - leftHandX) * 0.5, leftSpeed);
            leftHandX = leftHandX + leftHandHsp;
            leftHandVsp = MathUtilities.Lerp(leftHandVsp, (leftHandBaseY - leftHandY) * 0.5, leftSpeed);
            leftHandY = leftHandY + leftHandVsp;

            rightHandHsp = MathUtilities.Lerp(rightHandHsp, (hShift - (dsp * 50) - rightHandX) * 0.5, rightSpeed);
            rightHandX = rightHandX + rightHandHsp;
            rightHandVsp = MathUtilities.Lerp(rightHandVsp, (rightHandBaseY - rightHandY) * 0.5, rightSpeed);
            rightHandY = rightHandY + rightHandVsp;

            if (punch > 0) punch--;

            if ((hpDisplay <= hpValue + 0.1) && (hpDisplay < hpTrail)) hpTrail -= 0.5;
            if (hpDisplay > hpTrail) hpTrail = Convert.ToInt32(hpDisplay);
            hpValue = Math.Max(hpValue, 0);
            hpDisplay = MathUtilities.Lerp(hpDisplay, hpValue, 0.5);
            //hpTrail = Lerp(hpTrail, hpDisplay, 0.05);

            if (weapons[weapon].reloading) weapons[weapon].Reload();
            ammoDisplay = MathUtilities.Lerp(ammoDisplay, ammoValue, 0.5);
            if (cooldown > 0) cooldown--;

            if (hpValue <= 0) active = false;
            if (damageFrames > 0) damageFrames--;
        }
        public void Punch()
        {
            if (hand) { leftHandHsp += 8; leftHandVsp -= 8; rightHandHsp += 4; rightHandVsp += 4; }
            else { rightHandHsp -= 8; rightHandVsp -= 8; leftHandHsp -= 4; leftHandVsp += 4; }
            hand = !hand;
            punch = 20;
        }
    }
}
