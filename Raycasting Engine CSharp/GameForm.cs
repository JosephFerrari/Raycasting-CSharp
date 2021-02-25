using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Drawing.Imaging;
using System.Media;
using System.Diagnostics;
using System.Windows.Media;
using Color = System.Drawing.Color;
using System.Runtime.InteropServices;

namespace Raycasting_Engine_CSharp
{
    public enum Environment { Sewer, Ice, Haunted }; // Environment type enumeration used for textures and movement style
    public enum EntityType { None, Player, Freaker, FlyingEnemy, Barrel, Key, Health, Ammo, Basketball, Poof, Slash, Rocket }; // Entity type enumeration used for spawning preset objects
    public enum WeaponType { Fists, Pistol, AssaultRifle, Shotgun, Minigun, RocketLauncher }; // Weapon type enumeration used for creating preset weapons
    public enum DoorType { Static, HorizontalRight, HorizontalLeft, VerticalUp, VerticalDown, PushShort, PushLong }; // Door type enumeration used for moving tiles
    public enum AlignH { Left, Center, Right }; // Horizontal alignment enumeration used for drawing parameters
    public enum AlignV { Top, Center, Bottom }; // Vertical alignment enumeration used for drawing parameters

    public partial class GameForm : Form
    {
        // Audio Essentials
        [DllImport("winmm.dll")] public static extern int waveOutGetVolume(IntPtr hwo, out uint dwVolume);
        [DllImport("winmm.dll")] public static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);
        public int soundVolume = 10; // Volume of sound effects from 0 to 10
        // Gameplay Essentials
        public static Board currentBoard = new Board(16, 12, Environment.Sewer); // Set up level map including size and environment
        public Entity player = new Entity(EntityType.Player, currentBoard, 2.5, 2.5); // Set up player entity including type and location
        public Player playerControl; // Set up player control object
        public List<Entity> entitiesList = new List<Entity>(); // Set up list of active entities such as enemies and items
        public bool paused = false; // Is the game in a paused state
        // Display Settings
        public int screenWidth = 960 / 4; // Base width of game render
        public int screenHeight = 480 / 4; // Base height of game render
        public double resolution = 1; // Multiplier of game render representing quality
        public int upscale = 4; // Upscale factor of game after render causing pixelation
        public int windowWidth; // Upscaled width of window after render
        public int windowHeight; // Upscaled height of window after render
        public int centreX; // Horizontal centre of base game render for reference
        public int centreY; // Vertical centre of base game render for reference
        public int texWidth = 64; // Texture width for drawing parameters
        public int texHeight = 64; // Texture height for drawing parameters
        public Byte[] viewCache = null; // A cache of the previous frame
        public bool warningLeft = false; // Whether to display an indication of enemies to the player’s left
        public bool warningRight = false; // Whether to display an indication of enemies to the player’s right
        // Performance Measuring
        public long framerate; // Game loops per second
        public long framerateAverage = 0; // Framerate adjusted to be more stable and less sporadic for easier reading
        public List<long> framerateCaches = new List<long>(); // Cache of previous framerates for average calculation
        public long prevTick; // Cache of computer tick counter for calculating framerate
        public Stopwatch timer = new Stopwatch(); // Stopwatch for time keeping
        // Player Input
        public int crosshairX = 0; // Horizontal offset of crosshair for weapon aiming
        public int crosshairY = 0; // Vertical offset of crosshair for weapon aiming
        public int crosshairWidth = 20; // Crosshair width for reference when drawing
        public int crosshairHeight = 20; // Crosshair height for reference when drawing
        public int mouseBottleneck = 60; // Limit to mouse movement per frame
        public bool[] buttonCaches = new bool[9]; // Caches of button states to calculate how long a button has been held
        public FontSheet fontTeapot = new FontSheet(Properties.Resources.TeapotFontSheet, 6, 10, 13, 6, new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ' ' , '.', ',', ':', '?', '!', '#', '&', '(', ')', '[', ']', '<', '>', '_', '-' });
        public FontSheet fontHaptic = new FontSheet(Properties.Resources.HapticFontSheet, 4, 5, 10, 1, new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });
        // Game Over Screen Control
        public double goScale = 0; // Scale of game over graphic
        public double goChange = 0; // Scale increment for game over graphic
        public double goYpos = 1; // Vertical position of game over graphic
        public double goOffset = 0; // Positional offset of game over background graphic
        // Pause Screen Control
        public double pScale = 0; // Scale of pause screen graphic
        public double pChange = 0; // Scale increment for pause screen graphic
        public bool pDesaturated = false; // Desaturation of viewpoint during pause screen transition
        // Scorekeeping
        public int score = 0; // Player score value
        public int combo = 0; // Amount of kills achieved within a certain time frame
        public int comboTimer = 0; // The time remaining before combo counter is reset
        public int comboTimerMax = 0; // Cache of the maximum combo reset time for use drawing the countdown bar
        public int comboCache = 0; // The amount of points gained since starting the current combo, to be multiplied by the combo value
        public GameForm()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            uint CurrVol = 0;
            waveOutGetVolume(IntPtr.Zero, out CurrVol);
            uint NewVolumeAllChannels = (short.MaxValue / 5) * Convert.ToUInt32(soundVolume);
            waveOutSetVolume(IntPtr.Zero, NewVolumeAllChannels);
            SetVolume(soundVolume);
            playerControl = new Player(ref player); // Create player control object and link player entity
            windowWidth = Convert.ToInt32(screenWidth * upscale); // Determine upscaled width of window after render
            windowHeight = Convert.ToInt32(screenHeight * upscale); // Determine upscaled height of window after render
            screenWidth = Convert.ToInt32(screenWidth * resolution); // Multiply screen width by resolution to determine quality
            screenHeight = Convert.ToInt32(screenHeight * resolution); // Multiply screen height by resolution to determine quality
            centreX = windowWidth / 2; // Determine horizontal centre of base game render for reference
            centreY = windowHeight / 2; // Determine vertical centre of base game render for reference
            ViewBox.Size = new System.Drawing.Size(screenWidth, screenHeight); // Set size of game render picture box
            GUIBox.Size = new System.Drawing.Size(screenWidth, screenHeight); // Set size of interface picture box
            //currentBoard.player = player; // Link player entity to the level
            // Creating enemy and item entities with their type and location and linking them to the level
            /*entitiesList.Add(new Entity(EntityType.Freaker, currentBoard, 13.5, 10.5));
            entitiesList.Add(new Entity(EntityType.FlyingEnemy, currentBoard, 3.5, 3.5));
            entitiesList.Add(new Entity(EntityType.Barrel, currentBoard, 2, 8.5));
            entitiesList.Add(new Entity(EntityType.Barrel, currentBoard, 1.5, 8.5));
            entitiesList.Add(new Entity(EntityType.Barrel, currentBoard, 1.5, 8));
            entitiesList.Add(new Entity(EntityType.Barrel, currentBoard, 13, 1.5));
            entitiesList.Add(new Entity(EntityType.Barrel, currentBoard, 13.5, 2));
            entitiesList.Add(new Entity(EntityType.Barrel, currentBoard, 13.5, 1.5));
            entitiesList.Add(new Entity(EntityType.Key, currentBoard, 14.5, 6.5));
            entitiesList.Add(new Entity(EntityType.Key, currentBoard, 7.5, 8.5));*/
            entitiesList.Add(new Entity(EntityType.Basketball, currentBoard, 7.5, 1.5));
            for (int i = 0; i < currentBoard.entitiesList.Count; i++) { entitiesList.Add(currentBoard.entitiesList[i]); }
            Random rnd = new Random();
            //for (int i = 0; i < 20; i++) { entitiesList.Add(new Entity(EntityType.Freaker, ref currentBoard, rnd.Next(2, currentBoard.boardWidth - 2), rnd.Next(2, currentBoard.boardHeight - 2))); }
            System.Windows.Forms.Cursor.Hide(); // Hide cursor to stop it obscuring gameplay
            System.Windows.Forms.Cursor.Position = new Point(this.Left + centreX, this.Top + centreY); // Position the cursor in the centre of the window
            timer.Start();
        }
        private void GameTicker_Tick(object sender, EventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Y)) playerControl.hpValue = 0;
            framerate = Convert.ToInt64(1000 / Math.Max(timer.ElapsedMilliseconds - prevTick, 1));
            framerateCaches.Add(framerate);
            if (framerateCaches.Count > 20) framerateCaches.RemoveAt(0);
            framerateAverage = 0;
            for (int i = 0; i < framerateCaches.Count; i++) { framerateAverage += framerateCaches[i]; }
            framerateAverage /= framerateCaches.Count;
            prevTick = timer.ElapsedMilliseconds;
            SetDebug(ref currentBoard, ref playerControl);
            double delta = 60.0 / MathUtilities.Clamp(framerate, 1, 60); // Unused delta timing value to control game speed
            if (paused)
            {
                pChange = MathUtilities.Lerp(pChange, (0.3 - pScale) * 0.8, 0.25);
                pScale += pChange;
            }
            else
            {
                pChange = MathUtilities.Lerp(pChange, (0 - pScale) * 0.8, 0.25);
                pScale += pChange;
                if (pScale <= 0) pChange = 0;
                pDesaturated = false;
            }
            if ((Keyboard.IsKeyDown(Key.Down)) && (!buttonCaches[7]))
            {
                soundVolume = Math.Max(soundVolume - 1, 0);
                SetVolume(soundVolume);
            }
            if ((Keyboard.IsKeyDown(Key.Up)) && (!buttonCaches[8]))
            {
                soundVolume = Math.Min(soundVolume + 1, 10);
                SetVolume(soundVolume);
            }
            if ((Keyboard.IsKeyDown(Key.Escape)) && (!buttonCaches[3])) { this.Close(); return; } // Window exit condition
            if ((Keyboard.IsKeyDown(Key.Tab)) && (!buttonCaches[4]))// Game pause condition
            {
                paused = !paused;
                System.Windows.Forms.Cursor.Position = new Point(this.Left + centreX, this.Top + centreY);
            }
            // Using button caches, the if statement determines whether the button was not pressed in the last frame to prevent the game unpausing immediately since the button is still held
            if ((paused) && (playerControl.active)) // Bail out of game loop if game is paused
            {
                System.Windows.Forms.Cursor.Show();
                ButtonControl();
                DrawScreen(currentBoard, playerControl, Keyboard.IsKeyDown(Key.Space), goYpos < 360);
                return;
            }
            int mouseX = MousePosition.X - (this.Left + centreX); // Determine how far the mouse has moved horizontally since last frame
            int mouseY = MousePosition.Y - (this.Top + centreY); // Determine how far the mouse has moved vertically since last frame
            int squash = 1; // Set up squash value for a visual effect on the crosshair
            if (ActiveForm == this) // Determines if the application has focus so as not to trap the user's cursor
            {
                if (Math.Abs(crosshairY + mouseY) > screenHeight / 4) squash = -1; // Sets squash direction to flatten the crosshair if it is the furthest it can move away from the centre
                // Sets the width and height of the crosshair determined by the squash direction and the amount the crosshair has moved to create a visual representiion of its movmement
                crosshairWidth = Convert.ToInt32(MathUtilities.Clamp(MathUtilities.Lerp(crosshairWidth, 20 + (Math.Abs(mouseX) * squash / 2) - (Math.Abs(mouseY) * squash / 2), 0.5), 10, 30));
                crosshairHeight = Convert.ToInt32(MathUtilities.Clamp(MathUtilities.Lerp(crosshairHeight, 20 + (Math.Abs(mouseY) * squash / 2) - (Math.Abs(mouseX) * squash / 2), 0.5), 10, 30));
                // Sets the crosshair offset to allow for player aiming outside of the centre of the screen
                crosshairX = Convert.ToInt32(MathUtilities.Clamp(crosshairX + mouseX, -20, 20));
                crosshairY = Convert.ToInt32(MathUtilities.Clamp(crosshairY + mouseY, -screenHeight / 4, screenHeight / 4));
                // Bring the crosshair back to the centre of the screen gradually (this was altered to help with aiming, because it worsens the player's accuracy)
                crosshairX = Convert.ToInt32(crosshairX * 0); // Fix the crosshair to the centre of the view horizontally
                crosshairY = Convert.ToInt32(crosshairY * 1); // Do nothing to the vertical position of the crosshair
                System.Windows.Forms.Cursor.Position = new Point(this.Left + centreX, this.Top + centreY); // Position the cursor in the centre of the window
                System.Windows.Forms.Cursor.Hide(); // Hide the cursor
            }
            else // If the application doesn't have focus, don't move the player's view
            {
                mouseX = 0;
                mouseY = 0;
                System.Windows.Forms.Cursor.Show();
                paused = true;
            }
            if (playerControl.active) playerControl.HandleMovement(MathUtilities.Clamp(mouseX, -mouseBottleneck, mouseBottleneck), delta); // Instigate player input
            player.Update(delta); // Instigate player movmenet
            playerControl.HandlePlayer(); // Instigate player interface update
            //Parallel.For(0, entitiesList.Count, i => { entitiesList[i].Update(delta); } ); // Instigate movement updates to all active entities
            DrawScreen(currentBoard, playerControl, Keyboard.IsKeyDown(Key.Space), goYpos < 360); // Render the player's viewpoint
            //DrawMap(ref currentBoard); // Unused render of the current level from top down (for testing purposes)
            //DrawUI(ref currentBoard, ref player); // Unused render of the player's interface which was integrated into the DrawView function instead
            if (comboTimer > 0) comboTimer--;
            if ((comboTimer <= 0) && (combo > 0))
            {
                score += comboCache * combo;
                combo = 0; comboCache = 0; comboTimerMax = 0;
            }
            Weapon weapon = playerControl.weapons[playerControl.weapon];
            if ((Control.MouseButtons == MouseButtons.Left) && ((!buttonCaches[0]) || (weapon.automatic))) // Player has fired weapon
            {
                if ((!weapon.reloading) && (playerControl.cooldown <= 0) && (playerControl.active))
                {
                    if (weapon.ammoValue >= weapon.ammoCost)
                    {
                        weapon.ammoValue -= weapon.ammoCost;
                        playerControl.cooldown = weapon.cooldown;
                        //SoundPlayer mediaPlayerPunch = new System.Media.SoundPlayer(Properties.Resources.Punch); // Set up punch sound effect
                        //SoundPlayer mediaPlayerPistol = new System.Media.SoundPlayer(Properties.Resources.Pistol); // Set up pistol fire sound effect
                        if ((!weapon.hitToPlay) && (weapon.attackSound != null)) weapon.attackSound.Play(); // Play weapon attack sound effect
                        //mediaPlayerSound.Play("C:/Users/Joseph/Documents/Visual Studio 2015/Projects/Raycasting Engine CSharp/Raycasting Engine CSharp/bin/Pistol.wav");
                        if (weapon.projectile != EntityType.None)
                        {
                            entitiesList.Add(new Entity(weapon.projectile, currentBoard, player.xPos, player.yPos));
                            entitiesList[entitiesList.Count - 1].hsp = 0.1 * (Math.Cos(player.dir));
                            entitiesList[entitiesList.Count - 1].vsp = 0.1 * (Math.Sin(player.dir));
                        }
                        else
                        {
                            Entity closestEntity = null; // Set up variables to store which entity is closest
                            double closestDistance = 1000;
                            double closestMultiplier = 0;
                            Parallel.For(0, playerControl.targetedEntities.Count, i => // Move through all entities targeted by the player's crosshair
                            {
                                if (playerControl.targetedDistances[Math.Min(i, playerControl.targetedDistances.Count - 1)] < closestDistance) // Update the holder of the closest entity and the distance to it
                                {
                                    closestEntity = playerControl.targetedEntities[Math.Min(i, playerControl.targetedEntities.Count - 1)];
                                    closestDistance = playerControl.targetedDistances[Math.Min(i, playerControl.targetedDistances.Count - 1)];
                                    closestMultiplier = playerControl.targetedMultipliers[Math.Min(i, playerControl.targetedMultipliers.Count - 1)];
                                }
                            });
                            if (((closestDistance <= weapon.range) || (weapon.range == 0)) && (closestEntity != null) && (closestEntity.damagable) && (!closestEntity.dead)) // If the closest targeted entity is within the weapons range and able to be damaged
                            {
                                if ((weapon.hitToPlay) && (weapon.attackSound != null)) weapon.attackSound.Play(); // Play weapon attack sound effect
                                closestEntity.hsp += (weapon.knockback * (Math.Cos(player.dir))) / closestEntity.weight; // Push entity in the direction the player is facing
                                closestEntity.vsp += (weapon.knockback * (Math.Sin(player.dir))) / closestEntity.weight; // The weight attribute is used here to push lighter objects further and vice versa
                                if (closestEntity.entityType == EntityType.Basketball) closestEntity.zsp = Math.Max(closestEntity.zsp, 0) + 6; // Bounce basketballs
                                Random rnd = new Random(); // Set up random number generator
                                int sign = Math.Sign(rnd.Next(0, 2) - 0.5); // Randomly select a direction for squash and stretch
                                closestEntity.compression += 0.2 * sign; // Change immediate scale of the enities sprite
                                closestEntity.cDifference += 0.2 * sign; // Change the direction of scale of the entities sprite
                                closestEntity.white = 1; // Sprite will flash white briefly to indicate a hit
                                int damage = Convert.ToInt32(weapon.damage * closestMultiplier);
                                closestEntity.HP -= Convert.ToInt32(weapon.damage * closestMultiplier);
                                if (closestEntity.enemy) { score += damage; comboTimerMax = Math.Max(60 - combo, 30); comboTimer = comboTimerMax; combo++; comboCache += damage; }
                                //this.Text = damage.ToString();
                                if (closestEntity.HP > 0)
                                {
                                    entitiesList.Add(new Entity(EntityType.Slash, currentBoard, closestEntity.xPos - (closestEntity.hsp / 2), closestEntity.yPos - (closestEntity.vsp / 2)));
                                    entitiesList[entitiesList.Count - 1].zPos = -(crosshairY * closestDistance) / 3;
                                }
                            }
                            else // Unused code that shoots basketballs from the player's hands (for testing purposes)
                            {
                                /*entitiesList.Add(new Entity(EntityType.Basketball, ref currentBoard, player.xPos, player.yPos));
                                entitiesList[entitiesList.Count - 1].hsp = 0.2 * (Math.Cos(player.dir));
                                entitiesList[entitiesList.Count - 1].vsp = 0.2 * (Math.Sin(player.dir));
                                entitiesList[entitiesList.Count - 1].zPos = 0;
                                entitiesList[entitiesList.Count - 1].zsp = -4;*/
                            }
                        }
                        crosshairWidth = 40; // Visual indication of input through the crosshair size
                        crosshairHeight = 40;
                        if (weapon.name == "Fists") playerControl.Punch(); // Function that controls the position of the player's hands
                    }
                    else if ((playerControl.ammoValue >= weapon.ammoCost) && (!buttonCaches[0])) weapon.BeginReload();
                }
            }
            if ((Keyboard.IsKeyDown(Key.R)) && (!buttonCaches[6]) && (weapon.ammoValue < weapon.ammoMax) && (playerControl.ammoValue >= weapon.ammoCost)) weapon.BeginReload();
            if ((Control.MouseButtons == MouseButtons.Middle) && (!buttonCaches[2])) // Player has toggled weapon
            {
                if (!playerControl.weapons[playerControl.weapon].reloading)
                {
                    playerControl.weapon++;
                    playerControl.rightHandX = 16;
                    playerControl.rightHandY = 32;
                    playerControl.rightHandHsp = 0;
                    playerControl.rightHandVsp = 0;
                    if (playerControl.weapon >= playerControl.weapons.Count) playerControl.weapon = 0;
                }
            }
            warningLeft = false;
            warningRight = false;
            Parallel.For(0, entitiesList.Count, i => // Loops through all active entities 
            {
                if (i >= entitiesList.Count) return;
                Entity entity = entitiesList[i]; // Sets a reference to the entity for easy calling
                entity.Update(delta); // Instigate movement updates to all active entities
                if ((entity.moveSpeed > 0) && (!entity.dead)) // Entity movespeed dictates how fast it will chase the player, and so it doesn't need to move if this is zero
                {
                    double angle = (Math.PI * 1.5) - Math.Atan2(player.xPos - entity.xPos, player.yPos - entity.yPos); // Find direction towards the player (in radians)
                    angle = angle % (Math.PI * 2); // Shorten number if it is unnecessarily large (e.g 720 degrees is the same as 360 degrees)
                    if (angle < 0) angle += (Math.PI * 2); // Ensure the angle is positive
                    entity.hsp -= entity.moveSpeed * (Math.Cos(angle)); // Move entity towards the player
                    entity.vsp -= entity.moveSpeed * (Math.Sin(angle)); // Will be replaced by a more sophisticated tracking system when gameplay features are added
                }
                if ((entity.consumable) && (entity.CheckCollision(player, 0.6))) // If the player walks over an item they can pick up
                {
                    System.Media.SoundPlayer mediaPlayerItemPickup = new System.Media.SoundPlayer(Properties.Resources.ItemPickup); // Set up item pickup sound effect
                    mediaPlayerItemPickup.Play(); // Play item pickup sound effect
                    if (entity.entityType == EntityType.Key) playerControl.keysHeld++; // Increase key count if a key is picked up
                    if (entity.entityType == EntityType.Health) playerControl.hpValue = Math.Min(playerControl.hpValue + 25, playerControl.hpMax); // Increase health if a health kit is picked up
                    if (entity.entityType == EntityType.Ammo) playerControl.ammoValue += 16; // Increase ammo count if ammo is picked up
                    //entitiesList.RemoveAt(i); // Remove the object as it has now been collected
                    entity.garbage = true;
                }
                if ((entity.solid) && (Math.Sqrt(Math.Pow(entity.hsp, 2) + Math.Pow(entity.vsp, 2)) > 0.01)) // If the entity has collision and is moving with enough speed
                {
                    Parallel.For(0, entitiesList.Count, n => // Loop through all active entities (again)
                    {
                        if (n >= entitiesList.Count) return;
                        Entity targetEntity = entitiesList[n]; // Sets a reference to the entity for easy calling
                        // If the targeted entity is within a radius of the current entity and it isn't targeting itself (aka if the current entity has collided with the target)
                        if ((entity.CheckCollision(targetEntity, entity.hitboxSize / 2)) && (targetEntity != entity) && (targetEntity.solid))
                        {
                            double angle = (Math.PI * 1.5) - Math.Atan2(entity.xPos - targetEntity.xPos, entity.yPos - targetEntity.yPos); // Find direction towards the target (in radians)
                            angle = angle % (Math.PI * 2); // Shorten number if it is unnecessarily large (e.g 720 degrees is the same as 360 degrees)
                            if (angle < 0) angle += (Math.PI * 2); // Ensure the angle is positive
                            double speed = Math.Sqrt(Math.Pow(entity.hsp, 2) + Math.Pow(entity.vsp, 2)); // Find the speed the current entity is moving at
                            targetEntity.hsp += (speed * (Math.Cos(angle))) / targetEntity.weight; // Push the targeted entity away from the current entity
                            targetEntity.vsp += (speed * (Math.Sin(angle))) / targetEntity.weight; // The weight attribute is used here to push lighter objects further and vice versa
                            if (entity.entityType == EntityType.Basketball)
                            {
                                targetEntity.compression += 0.1; // Change immediate scale of the target enities sprite
                                targetEntity.cDifference += 0.1; // Change the direction of scale of the target entities sprite
                            }
                            /*if (targetEntity.entityType == EntityType.Basketball)
                            {
                                entity.compression += 0.1; // Change immediate scale of the target enities sprite
                                entity.cDifference += 0.1; // Change the direction of scale of the target entities sprite
                            }*/
                            if (entity.entityType == EntityType.Rocket) entity.Explode(entitiesList);
                        }
                    });
                }
                if ((entity.collided) && (entity.destroyOnImpact)) entity.Explode(entitiesList);
                double playerDistance = Math.Sqrt(Math.Pow(entity.xPos - player.xPos, 2) + Math.Pow(entity.yPos - player.yPos, 2));
                if ((playerDistance < 0.6) && (!entity.dead) && (!entity.projectile))
                {
                    if ((entity.enemy) && (playerControl.damageFrames <= 0)) { playerControl.hpValue -= entity.damage; playerControl.damageFrames = 60; }
                    double angle = (Math.PI * 1.5) - Math.Atan2(player.xPos - entity.xPos, player.yPos - entity.yPos); // Find direction away from the player
                    angle = angle % (Math.PI * 2); // Shorten number if it is unnecessarily large (e.g 720 degrees is the same as 360 degrees)
                    if (angle < 0) angle += (Math.PI * 2); // Ensure the angle is positive
                    double speed = Math.Sqrt(Math.Pow(entity.hsp, 2) + Math.Pow(entity.vsp, 2)); // Find the speed the current entity is moving at
                    if (entity.enemy)
                    {
                        entity.compression -= 0.3; // Change immediate scale of the target enities sprite
                        entity.cDifference -= 0.3; // Change the direction of scale of the target entities sprite
                        player.hsp -= (speed * (Math.Cos(angle))) / entity.weight; // Push the player away from self
                        player.vsp -= (speed * (Math.Sin(angle))) / entity.weight; // The weight attribute is used here to push lighter objects further and vice versa
                        speed = 0.3;
                    }
                    else speed = Math.Sqrt(Math.Pow(player.hsp, 2) + Math.Pow(player.vsp, 2)); // Find the speed the player is moving at
                    entity.hsp += (speed * (Math.Cos(angle))) / entity.weight; // Push self away from the player
                    entity.vsp += (speed * (Math.Sin(angle))) / entity.weight; // The weight attribute is used here to push lighter objects further and vice versa
                }
                if ((entity.damagable) && (entity.HP <= 0) && (entity.hpMax > 0) && (!entity.dead))
                {
                    entity.dead = true;
                    entity.ticker = 0;
                    entity.solid = false;
                    entity.bounce = 0;
                    entity.compression = 0;
                    entity.cDifference = 0;
                    entity.grip = 0.9;
                    entity.weight = 1;
                    entitiesList.Add(new Entity(EntityType.Poof, currentBoard, entity.xPos, entity.yPos));
                    entitiesList[entitiesList.Count - 1].zPos = -16;
                }
                if ((entity.enemy) && (currentBoard.environment == Environment.Haunted)) entity.opacity = Math.Min(0.6 - (Math.Sqrt(Math.Pow(entity.xPos - player.xPos, 2) + Math.Pow(entity.yPos - player.yPos, 2)) / 10), 0.7);
            });
            for (int i = 0; i < entitiesList.Count; i++) // Loops through all active entities 
            {
                if (i >= entitiesList.Count) continue;
                Entity entity = entitiesList[i]; // Sets a reference to the entity for easy calling
                if ((entity.releaseContents) && (entity.contents != EntityType.None))
                {
                    entitiesList.Add(new Entity(entity.contents, entity.currentBoard, entity.xPos, entity.yPos));
                    entity.contents = EntityType.None;
                }
                if (entity.garbage)
                {
                    entitiesList.RemoveAt(i);
                    i--;
                }
            }
            if ((Control.MouseButtons == MouseButtons.Right) && (!buttonCaches[1])) // Player has clicked to inspect a wall
            {
                bool interacted = false; // Unused Set up of indication of succesful interaction (would be used for an animation when one is added)
                // If the player is targeting a tile and it is within appropriate range
                if ((playerControl.targetedTile != null) && (playerControl.targetedDistance <= 3)) interacted = playerControl.targetedTile.Interact(ref playerControl); // Interact with the player's targeted
                crosshairWidth = 0; // Visual indication of input through the crosshair size
                crosshairHeight = 0;
                //currentBoard.GenerateLayout(); // Unused regeneration of level randomly (for testing purposes)
            }
            Parallel.For(0, currentBoard.boardWidth, x => // Loop through every horizontal strip of the map
            {
                Parallel.For(0, currentBoard.boardHeight, y => // Loop through every vertical tile of the horizontal strip
                {
                    Tile tile = currentBoard.BoardPoint(y, x); // Set a reference to the tile for easy calling
                    if (tile.doorType != DoorType.Static) tile.Update(); // Update the tile if it is interactable
                });
            });
            if (!playerControl.active)
            {
                goYpos = Math.Min(goYpos + 2, 360);
                if (goYpos >= 360)
                {
                    goOffset = MathUtilities.Lerp(goOffset, -30, 0.05);
                    goScale = MathUtilities.Lerp(goScale, 0.2, 0.05);
                    if ((Keyboard.IsKeyDown(Key.Tab)) && (!buttonCaches[4])) this.Close();
                }
                else
                {
                    goChange = MathUtilities.Lerp(goChange, (0.3 - goScale) * 0.6, 0.2);
                    goScale += goChange;
                }
            }
            ButtonControl(); // Call for control of button input
        }
        public void ButtonControl()
        {
            // These input caches keep track of whether each required button was held or not last frame
            // This is used to tell whether a button was just pressed or is being held down and prevents double inputs (e.g. pausing and then unpausing the game before taking your finger off the button)
            buttonCaches[0] = (Control.MouseButtons == MouseButtons.Left); // Left mouse button
            buttonCaches[1] = (Control.MouseButtons == MouseButtons.Right); // Right mouse button
            buttonCaches[2] = (Control.MouseButtons == MouseButtons.Middle); // Middle mouse button
            buttonCaches[3] = Keyboard.IsKeyDown(Key.Escape); // Escape key
            buttonCaches[4] = Keyboard.IsKeyDown(Key.Tab); // Tab key
            buttonCaches[5] = Keyboard.IsKeyDown(Key.Space); // Spcae Bar
            buttonCaches[6] = Keyboard.IsKeyDown(Key.R); // R Key
            buttonCaches[7] = Keyboard.IsKeyDown(Key.Down); // Down Arrow
            buttonCaches[8] = Keyboard.IsKeyDown(Key.Up); // Up Arrow
        }
        public void SetDebug(ref Board targetBoard, ref Player playerControl)
        {
            Entity targetPlayer = playerControl.entity; // Set a reference to the player's entity for easy calling
            //Window name including selected debug data
            //this.Text = "Raycasting Engine Alpha"; // No Data
            this.Text = "Raycasting Engine Alpha - FPS " + framerateAverage.ToString(); // FPS
            //this.Text = "Raycasting Engine Alpha - " + Math.Round(player.xPos, 5).ToString() + " " + Math.Round(player.yPos - 0, 5).ToString(); // Player Position
            //this.Text = "Raycasting Engine Alpha - " + Math.Round(player.hsp * 1000, 0).ToString() + " " + Math.Round(player.vsp * 1000, 0).ToString(); // Player Directional Speed
            //this.Text = "Raycasting Engine Alpha - " + Convert.ToInt32((Math.Sqrt(Math.Pow(player.hsp, 2) + Math.Pow(player.vsp, 2)) * 1000) / (60.0 / Math.Max(framerate, 1))).ToString(); // Player Speed
            //this.Text = "Raycasting Engine Alpha - " + Math.Round(player.planeX / 0.45, 5).ToString() + " " + Math.Round(player.planeY / 0.45, 5).ToString(); // Player Plane
            //this.Text = "Raycasting Engine Alpha - " + (player.dir * (180 / Math.PI)).ToString(); // Angle Difference
            //this.Text = "Raycasting Engine Alpha - " + angleDif.ToString(); // Angle Difference
            //this.Text = "Raycasting Engine Alpha - " + cache.ToString(); // Debug Cache
            //this.Text = "Raycasting Engine Alpha - " + keysHeld.ToString(); // Keys Held
            //this.Text = "Raycasting Engine Alpha - " + DrawingUtilities.TexturePixel(rgbValues, targetX, targetY, screenWidth, screenHeight).A.ToString(); // Targeted Pixel Alpha
            //this.Text = "Raycasting Engine Alpha - " + ((Math.Atan2(player.vsp, player.hsp) * (180 / Math.PI)) + 0).ToString(); // Player Speed Direction
            //this.Text = "Raycasting Engine Alpha - " + Math.Round(playerControl.vShift * 100, 0).ToString(); // Player Hand Shift
            //this.Text = "Raycasting Engine Alpha - " + targetBoard.seed; // Seed
            //this.Text = "Raycasting Engine Alpha - " + (MousePosition.X - (this.Left + centreX)).ToString(); // Mouse Movement
            //this.Text = "Raycasting Engine Alpha - " + ((playerControl.hpWidth / Convert.ToDouble(playerControl.hpMax)) * playerControl.hpValue).ToString(); // HP Bar Width
            //this.Text = "Raycasting Engine Alpha - " + currentBoard.locks.ToString(); // Key Counter
        }
        public void DrawScreen(Board targetBoard, Player playerControl, bool drawMap, bool drawView) // Main display loop to render the player's viewpoint and 
        {
            Entity targetPlayer = playerControl.entity; // Set a reference to the player's entity for easy calling

            Bitmap drawTarget = new Bitmap(screenWidth, screenHeight); // Set up the bitmap which will hold the render
            Graphics graphic = this.CreateGraphics(); // Create a graphics object to handle graphics functions

            Rectangle bounds = new Rectangle(0, 0, drawTarget.Width, drawTarget.Height); // Set up a rectangle representitive of the bitmap's size
            // Extract the bitmap's data for use in graphics functions
            System.Drawing.Imaging.BitmapData bmpData = drawTarget.LockBits(bounds, System.Drawing.Imaging.ImageLockMode.ReadWrite, drawTarget.PixelFormat);

            IntPtr ptr = bmpData.Scan0; // Find the address of the first scanline of the bitmap
            int bytes = Math.Abs(bmpData.Stride) * drawTarget.Height; // Calculate the amount of bytes needed to represent the image through RGBA format
            Byte[] rgbValues = new Byte[bytes]; // Set up a byte array to represent the image through RBGA format
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes); // Copy the bitmap's RBGA data into the byte array
            // A byte array is used in place of a bitmap so that editing colour values is a quicker process than using the slow built in graphical funtions
            // This is especially useful given the size of the render and the amount of calculations that need to be done to render it

            List<Ray> rayList = new List<Ray>(); // Set up a list to hold all rays needed for rendering
            for (int i = 0; i < screenWidth; i++) rayList.Add(null); // Add dummy values to the list to make it the appropriate size
            Ray[] rays = rayList.ToArray(); // Convert the list into an array so that it is compatible with parallelized for loops
            Entity[] entities = entitiesList.ToArray(); // Do the same to the entities list
            // Lists are incompatible with parallelized for loops because rays may be calculated out of order or even overwrite each other if completed at a simular time

            /*Byte[] blankCanvas = DrawingUtilities.LockBitmap(new Bitmap(screenWidth, screenHeight));
            for (int pos = 0; pos < blankCanvas.Length; pos += 4)
            {
                blankCanvas[pos] = 236;
                blankCanvas[pos + 1] = 84;
                blankCanvas[pos + 2] = 220;
                blankCanvas[pos + 3] = 255;
            }*/

            bool pauseScreen = ((paused) || (pScale > 0.01)) && (playerControl.active);
            bool useViewCache = (paused) && (playerControl.active) && (viewCache != null);

            if (useViewCache)
            {
                if (!pDesaturated)
                {
                    pDesaturated = true;
                    viewCache = DrawingUtilities.DiscolorSprite(viewCache.ToArray(), screenWidth, screenHeight, 0.5, 0.5);
                }
                rgbValues = viewCache.ToArray();
            }
            else if (drawView)
            {
                rgbValues = DrawView(rgbValues.ToArray(), bmpData.Stride, targetBoard, playerControl, pauseScreen);
                rgbValues = DrawUI(rgbValues.ToArray(), bmpData.Stride, targetBoard, playerControl, drawMap, pauseScreen);
                viewCache = rgbValues.ToArray();
            }
            if (pauseScreen) rgbValues = DrawPauseScreen(rgbValues.ToArray(), bmpData.Stride);
            if (!playerControl.active) rgbValues = DrawGameOverScreen(rgbValues.ToArray(), bmpData.Stride);

            // Unlock Bits
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);
            drawTarget.UnlockBits(bmpData);

            // Viewbox
            ViewBox.Size = new System.Drawing.Size(Convert.ToInt32((screenWidth / resolution) * upscale), Convert.ToInt32((screenHeight / resolution) * upscale));
            ViewBox.Image = DrawingUtilities.ResizeImage(drawTarget, Convert.ToInt32((screenWidth / resolution) * upscale), Convert.ToInt32((screenHeight / resolution) * upscale));

            // Misc
            graphic.Dispose();
            this.ClientSize = new System.Drawing.Size(Convert.ToInt32((screenWidth / resolution) * upscale), Convert.ToInt32((screenHeight / resolution) * upscale));
            this.Refresh();
            drawTarget.Dispose();
        }
        private Byte[] DrawView(Byte[] texture, int stride, Board targetBoard, Player playerControl, bool desaturate) // Main display loop to render the player's viewpoint and 
        {
            Entity player = playerControl.entity; // Set a reference to the player's entity for easy calling

            List<Ray> rayList = new List<Ray>(); // Set up a list to hold all rays needed for rendering
            for (int i = 0; i < screenWidth; i++) rayList.Add(null); // Add dummy values to the list to make it the appropriate size
            Ray[] rays = rayList.ToArray(); // Convert the list into an array so that it is compatible with parallelized for loops
            Entity[] entities = entitiesList.ToArray(); // Do the same to the entities list
            // Lists are incompatible with parallelized for loops because rays may be calculated out of order or even overwrite each other if completed at a simular time

            double baseDirX = Math.Cos(player.dir); // Horizontal component of the player's direction
            double baseDirY = Math.Sin(player.dir); // Vertical component of the player's direction

            //Raycasting
            Parallel.For(0, screenWidth, revScanX => // Loop through every vertical scanline of the bitmap
            {
                int scanX = screenWidth - 1 - revScanX; // Reverse the x-position of the current scanline as parallel for loops can only move in one direction
                int wallHeight = 0; // Determines the height of the wall, based on the distance it is from the player
                Tile tile = null; // Determines the tile scanned and to be passed into the ray object for rendering
                int side = 0; // Determines the side of a wall that is hit by the ray
                double wallX = 0; // Determines where the ray hits horizontally along the texture of the wall
                bool rayEnded = false; // Determines whether to end the raycasting loop (it may be repeated to find the walls behind walls with cutouts to look through)
                List<int> excludedX = new List<int>(); // Lists to hold the X and Y coordinates of tiles the ray has hit but must travel through since they are transparent
                List<int> excludedY = new List<int>();
                double rayDirX = 0; // Representation of the ray's direction in vector format to reduce trigonometry required and therefore improve performance
                double rayDirY = 0;
                int mapX = 0; // Integer representation of the point the ray has hit for use grabbing the tile's data such as texture
                int mapY = 0;
                double perpWallDist = 0; // Determines the ray's distance perpendicular to the camera plane, as the Euclidean distance causes a fisheye effect
                bool thin = false; // Determines whether the wall hit by the ray is thin and needs to be pushed back before rendered
                bool transparent = false; // Determines whether the wall hit by the ray has transparency (has a cutout to look through, like a window)
                double transparentChange = 1; // Is used to change the darkness value and make tiles seen through transparent walls appear darker for aestetic purposes
                double perpWallDistCache = 0; // Keeps trach of the distance of the previous ray for use in the calculation of darkness
                while (!rayEnded)
                {
                    double cameraX = (2 * (Convert.ToDouble(scanX) / screenWidth)) - 1; // X-coordinate in camera space (-1 being the left side of the screen, 0 being the centre, and 1 being the right side)
                    rayDirX = baseDirX + (playerControl.planeX * cameraX); // Representation of the ray's direction in vector format taking into account the player's direction vector and the camera position
                    rayDirY = baseDirY + (playerControl.planeY * cameraX);
                    mapX = Convert.ToInt32(Math.Floor(player.xPos)); // Integer conversions of the player's position coordinates
                    mapY = Convert.ToInt32(Math.Floor(player.yPos));
                    double sideDistX; // Lengths travelled in each direction from the current position to the next side of a tile
                    double sideDistY;
                    double deltaDistX = Math.Abs(1 / rayDirX); // Lengths travelled in each direction from one side of a tile to the next
                    double deltaDistY = Math.Abs(1 / rayDirY);
                    int stepX; // Determines which direction to move in each axis (etiher +1 or -1)
                    int stepY;
                    bool hit = false; // Determines whether a wall has been hit
                    // Calculates the direction to move in and how far to move to align the ray with the first side of a tile
                    if (rayDirX < 0) { stepX = -1; sideDistX = (player.xPos - mapX) * deltaDistX; }
                    else { stepX = 1; sideDistX = (mapX + 1 - player.xPos) * deltaDistX; }
                    // This is done so that always hits the edge of a tile and therefore
                    if (rayDirY < 0) { stepY = -1; sideDistY = (player.yPos - mapY) * deltaDistY; }
                    else { stepY = 1; sideDistY = (mapY + 1 - player.yPos) * deltaDistY; }
                    double doubleMapX = mapX;
                    double doubleMapY = mapY;
                    int cacheMapX = mapX;
                    int cacheMapY = mapY;
                    while (!hit)
                    {
                        if (sideDistX < sideDistY)
                        {
                            sideDistX += deltaDistX;
                            doubleMapX += stepX;
                            side = 0;
                        }
                        else
                        {
                            sideDistY += deltaDistY;
                            doubleMapY += stepY;
                            side = 1;
                        }
                        tile = currentBoard.BoardPoint(Math.Floor(doubleMapX), Math.Floor(doubleMapY));
                        if ((tile.solid) && !((excludedX.Contains(Convert.ToInt32(Math.Floor(doubleMapX)))) && (excludedY.Contains(Convert.ToInt32(Math.Floor(doubleMapY)))))) hit = true;
                    }
                    if ((tile.pushOffset != 0) && (tile.doorType != DoorType.Static)) { transparentChange = 1 - Math.Abs(tile.hOffset + tile.vOffset); cacheMapX = Convert.ToInt32(Math.Floor(doubleMapX)); cacheMapY = Convert.ToInt32(Math.Floor(doubleMapY)); }
                    if (side == 0)
                    {
                        double offset = 0;
                        if (tile.pushOffset != 0) offset = tile.pushOffset * Math.Sign(doubleMapX - player.xPos);
                        perpWallDist = ((doubleMapX + offset - player.xPos + ((1 - stepX) / 2)) / rayDirX);
                        wallX = player.yPos + (perpWallDist * rayDirY);
                        if ((tile.pushOffset != 0) && (((wallX < Math.Floor(doubleMapY)) && (currentBoard.BoardPoint(doubleMapX, doubleMapY - 1).ID != tile.ID)) || ((wallX > Math.Floor(doubleMapY + 1)) && (currentBoard.BoardPoint(doubleMapX, doubleMapY + 1).ID != tile.ID)))) { excludedX.Add(Convert.ToInt32(Math.Floor(doubleMapX))); excludedY.Add(Convert.ToInt32(Math.Floor(doubleMapY))); continue; }
                    }
                    else
                    {
                        double offset = 0;
                        if (tile.pushOffset != 0) offset = tile.pushOffset * Math.Sign(doubleMapY - player.yPos);
                        perpWallDist = ((doubleMapY + offset - player.yPos + ((1 - stepY) / 2)) / rayDirY);
                        wallX = MathUtilities.Clamp(player.xPos + (perpWallDist * rayDirX), 0, 64);
                        if ((tile.pushOffset != 0) && (((wallX < Math.Floor(doubleMapX)) && (currentBoard.BoardPoint(doubleMapX - 1, doubleMapY).ID != tile.ID)) || ((wallX > Math.Floor(doubleMapX + 1)) && (currentBoard.BoardPoint(doubleMapX + 1, doubleMapY).ID != tile.ID)))) { excludedX.Add(Convert.ToInt32(Math.Floor(doubleMapX))); excludedY.Add(Convert.ToInt32(Math.Floor(doubleMapY))); continue; }
                    }
                    mapX = Convert.ToInt32(Math.Floor(doubleMapX));
                    mapY = Convert.ToInt32(Math.Floor(doubleMapY));
                    wallX -= Math.Floor(wallX);
                    wallHeight = Math.Max(Convert.ToInt32(Math.Min(screenHeight / Math.Max(perpWallDist, 0), 1147483647)), 1);
                    double darkness = Math.Min(5 / perpWallDist, 1); // double darkness = Math.Min(wallHeight / (screenHeight / 6f), 1);
                    if (currentBoard.environment == Environment.Haunted) darkness = Math.Min(Math.Max(10 - perpWallDist, 0) / perpWallDist, 1);
                    double angle = ((Math.PI * 1.5) - Math.Atan2(player.xPos - doubleMapX, player.yPos - doubleMapY)) % (Math.PI * 2);
                    if (angle < 0) angle += (Math.PI * 2);
                    if (side == 1) darkness *= (Math.Min(Math.Abs(rayDirY) * 2, 1)) + 0.1;
                    if (side == 0) darkness *= (Math.Min(Math.Abs(rayDirX) * 2, 1)) + 0.1;
                    if (transparent) darkness *= 1 - (((perpWallDist - perpWallDistCache) / 10) * transparentChange);
                    if (rays[Convert.ToInt32(MathUtilities.Clamp(scanX, 0, screenWidth - 1))] == null) rays[Convert.ToInt32(MathUtilities.Clamp(scanX, 0, screenWidth - 1))] = new Ray(screenWidth - 1 - scanX, tile, wallHeight, darkness, perpWallDist, mapX, mapY, rayDirX, rayDirY, Convert.ToInt32(texWidth * wallX), side, wallX, thin);
                    else rays[Convert.ToInt32(MathUtilities.Clamp(scanX, 0, screenWidth - 1))].subRay = new Ray(screenWidth - 1 - scanX, tile, wallHeight, darkness, perpWallDist, mapX, mapY, rayDirX, rayDirY, Convert.ToInt32(texWidth * wallX), side, wallX, thin);
                    if ((tile.texture == new Byte[0]) || (tile.pushOffset != 0)) { excludedX.Add(mapX); excludedY.Add(mapY); transparent = true; perpWallDistCache = perpWallDist; }
                    else rayEnded = true;
                }
            });

            // Sorting
            for (int i = 0; i < entities.Length; i++) { entities[i].depth = Math.Abs(entities[i].xPos - player.xPos) + Math.Abs(entities[i].yPos - player.yPos); }
            Array.Sort(entities, delegate (Entity x, Entity y) { return x.depth.CompareTo(y.depth); });
            //entities = entities.OrderBy(o => -o.depth).ToList();
            for (int i = 0; i < entities.Length; i++) { entities[i].order = i + 1; }

            // Targeting stats
            int targetX = Convert.ToInt32(Math.Floor(Convert.ToDouble(rays.Length / 2)) + crosshairX);
            int targetY = Convert.ToInt32((screenHeight / 2) + crosshairY);
            playerControl.targetedTile = rays[targetX].tile;
            playerControl.targetedSide = rays[targetX].side;
            playerControl.targetedX = rays[targetX].mapX;
            playerControl.targetedY = rays[targetX].mapY;
            playerControl.targetedDistance = rays[targetX].perpWallDist;
            playerControl.targetedEntities.Clear();
            playerControl.targetedDistances.Clear();
            playerControl.targetedMultipliers.Clear();
            double targetedDistance = playerControl.targetedDistance;
            Entity[] targetedEntities = new Entity[entities.Count()];
            double[] targetedDistances = new double[entities.Count()];
            double[] targetedMultipliers = new double[entities.Count()];

            // Floorcasting
            //for (int i = 0; i < rays.Count; i++)
            Parallel.For(0, rays.Length, i =>
            {
                Ray ray = rays[i];
                if (ray.subRay != null) ray = ray.subRay;
                Color texel = Color.Red;

                // Floorcasting
                double floorXWall = 0;
                double floorYWall = 0;
                if (ray.side == 0)
                {
                    floorYWall = ray.mapY + ray.wallX;
                    if (ray.rayDirX > 0) floorXWall = ray.mapX;
                    if (ray.rayDirX < 0) floorXWall = ray.mapX + 1;
                }
                else if (ray.side == 1)
                {
                    floorXWall = ray.mapX + ray.wallX;
                    if (ray.rayDirY > 0) floorYWall = ray.mapY;
                    if (ray.rayDirY < 0) floorYWall = ray.mapY + 1;
                }
                double distWall = ray.perpWallDist / 2;
                double distPlayer = 0;
                double currentDist;
                int drawEnd = Math.Min((screenHeight / 2) + ray.wallHeight, screenHeight);
                //Parallel.For(drawEnd + 1, screenHeight + 1, scanY =>
                for (int scanY = drawEnd + 1; scanY <= screenHeight; scanY++)
                {
                    currentDist = screenHeight / Convert.ToDouble(2 * scanY - screenHeight);
                    double weight = (currentDist - distPlayer) / (distWall - distPlayer);
                    double currentFloorX = weight * floorXWall + (1 - weight) * player.xPos;
                    double currentFloorY = weight * floorYWall + (1 - weight) * player.yPos;
                    int floorTexX = Convert.ToInt32((currentFloorX * texWidth) % texWidth);
                    int floorTexY = Convert.ToInt32((currentFloorY * texHeight) % texHeight);
                    texel = DrawingUtilities.TexturePixel(currentBoard.floorTexture, Math.Max(floorTexX, 0), Math.Max(floorTexY, 0), texWidth, texHeight);
                    if (currentBoard.environment == Environment.Haunted) texel = ColorUtilities.Multiply(texel, Math.Min((Math.Max(5 - currentDist, 0) * 0.2) / currentDist, 1));
                    else texel = ColorUtilities.Multiply(texel, Math.Min(1 / currentDist, 1)); // texel = ColorUtilities.Multiply(texel, 1 - Math.Min(currentDist / 8f, 1));
                    DrawingUtilities.DrawPixel(ref texture, Math.Min(screenWidth - 1 - ray.position, screenWidth - 1), Math.Min(scanY, screenHeight - 1), stride, texel, 254, false);
                    texel = DrawingUtilities.TexturePixel(currentBoard.ceilingTexture, Math.Max(floorTexX, 0), Math.Max(floorTexY, 0), texWidth, texHeight);
                    if (currentBoard.environment == Environment.Haunted) texel = ColorUtilities.Multiply(texel, Math.Min((Math.Max(5 - currentDist, 0) * 0.2) / currentDist, 1));
                    else texel = ColorUtilities.Multiply(texel, Math.Min(1 / currentDist, 1));
                    DrawingUtilities.DrawPixel(ref texture, Math.Min(screenWidth - 1 - ray.position, screenWidth - 1), Math.Min(screenHeight - scanY, screenHeight - 1), stride, texel, 254, false);
                }//);
            });

            // Wallcasting
            //for (int i = 0; i < rays.Count; i++)
            Parallel.For(0, rays.Length, i =>
            {
                Ray ray = rays[i];
                Color texel = Color.Red;
                int hOffset = Convert.ToInt32(ray.tile.hOffset * texHeight);
                int vOffset = Convert.ToInt32(ray.tile.vOffset * texHeight);
                if (ray.tile.texture != new Byte[0])
                {
                    for (int wallY = Math.Max(ray.wallHeight - (screenHeight / 2), 0); wallY <= Math.Min(ray.wallHeight * 2, screenHeight) + Math.Max(ray.wallHeight - (screenHeight / 2), 0); wallY++)
                    {
                        int texY = Convert.ToInt32(Math.Floor(wallY * (texHeight / Convert.ToDouble(ray.wallHeight * 2)))) + vOffset;
                        int texYNext = Convert.ToInt32(Math.Floor((wallY + 1) * (texHeight / Convert.ToDouble(ray.wallHeight * 2)))) + vOffset;
                        int texYPrev = Convert.ToInt32(Math.Floor((wallY - 1) * (texHeight / Convert.ToDouble(ray.wallHeight * 2)))) + vOffset;
                        //int wallOffset = Convert.ToInt32(Math.Floor(wallY * (((ray.tile.vOffset) * ((ray.wallHeight * 2) + Math.Max(ray.wallHeight - (screenHeight / 2), 0))) / Convert.ToDouble(ray.wallHeight * 2)))) + vOffset;
                        texel = DrawingUtilities.TexturePixel(ray.tile.texture, ray.texX + hOffset, texY, texWidth, texHeight);
                        if ((ray.subRay != null) && (ray.perpWallDist > ray.subRay.perpWallDist)) continue;
                        bool outline = false;
                        i = Convert.ToInt32(MathUtilities.Clamp(i, 1, rays.Length - 2));
                        if ((hOffset != 0) && (((rays[i - 1].texX + hOffset < 0) || (rays[i - 1].texX + hOffset > texWidth)) || ((rays[i + 1].texX + hOffset < 0) || (rays[i + 1].texX + hOffset > texWidth)))) outline = true;
                        if ((rays[i].perpWallDist + 0.4 < rays[i - 1].perpWallDist) || (rays[i].perpWallDist + 0.4 < rays[i + 1].perpWallDist)) outline = true;
                        if (((rays[i + 1].subRay != null) && (rays[i + 1].perpWallDist > rays[i + 1].subRay.perpWallDist)) || ((rays[i - 1].subRay != null) && (rays[i - 1].perpWallDist > rays[i - 1].subRay.perpWallDist))) outline = false;
                        if (((wallY == 0) || (wallY == (ray.wallHeight * 2) + Math.Max(ray.wallHeight - (screenHeight / 2), 0))) || ((texel != Color.FromArgb(236, 84, 220)) && ((texYNext > texHeight) || (texYNext < 0) || (texYPrev > texHeight) || (texYPrev < 0)))) outline = true;
                        if ((texel != Color.FromArgb(236, 84, 220) && (texY <= texHeight) && (texY >= 0) && (ray.texX + hOffset <= texWidth) && (ray.texX + hOffset >= 0)))// || (outline))
                        {
                            if ((outline) && (texel == Color.FromArgb(236, 84, 220))) texel = DrawingUtilities.TexturePixel(texture, Math.Min(screenWidth - 1 - ray.position, screenWidth - 1), Math.Min(Math.Abs((screenHeight / 2) - (ray.wallHeight / 1) + (wallY * (1))), screenHeight - 1), screenWidth, screenHeight);
                            else texel = ColorUtilities.Multiply(ColorUtilities.Add(texel, (ray.side - 1) * 30), ray.darkness);
                            if (outline) texel = ColorUtilities.Multiply(ColorUtilities.Add(texel, (ray.side - 1) * 30), 0.1);
                            DrawingUtilities.DrawPixel(ref texture, Math.Min(screenWidth - 1 - ray.position, screenWidth - 1), Math.Min(Math.Abs((screenHeight / 2) - (ray.wallHeight / 1) + (wallY * (1))), screenHeight - 1), stride, texel, 255, false);
                        }
                    }
                }
            });
            Parallel.For(0, rays.Length, i =>
            {
                Ray ray = rays[i].subRay;
                Color texel = Color.Red;
                if ((ray != null) && (ray.tile.texture != new Byte[0]))
                {
                    for (int texY = Math.Max(ray.wallHeight - (screenHeight / 2), 0); texY <= Math.Min(ray.wallHeight * 2, screenHeight) + Math.Max(ray.wallHeight - (screenHeight / 2), 0); texY++)
                    {
                        texel = DrawingUtilities.TexturePixel(ray.tile.texture, ray.texX, Convert.ToInt32(Math.Floor(texY * (texHeight / Convert.ToDouble(ray.wallHeight * 2)))), texWidth, texHeight);
                        bool outline = false;
                        if (((i > 0) && (rays[i - 1].subRay != null) && (ray.perpWallDist + 0.4 < rays[i - 1].subRay.perpWallDist)) || (((i < rays.Count() - 1) && rays[i + 1].subRay != null) && (ray.perpWallDist + 0.4 < rays[i + 1].subRay.perpWallDist)) || (texY == 0) || (texY == Math.Min(ray.wallHeight * 2, screenHeight) + Math.Max(ray.wallHeight - (screenHeight / 2), 0))) outline = true;
                        if (texel != Color.FromArgb(236, 84, 220) && (((texHeight / (ray.wallHeight * 2)) * texY) <= texHeight))
                        {
                            if ((outline) && (texel == Color.FromArgb(236, 84, 220))) texel = DrawingUtilities.TexturePixel(texture, Math.Min(screenWidth - 1 - ray.position, screenWidth - 1), Math.Min(Math.Abs((screenHeight / 2) - (ray.wallHeight / 1) + (texY * (1))), screenHeight - 1), screenWidth, screenHeight);
                            else texel = ColorUtilities.Multiply(ColorUtilities.Add(texel, (ray.side - 1) * 30), ray.darkness);
                            if (outline) texel = ColorUtilities.Multiply(ColorUtilities.Add(texel, (ray.side - 1) * 30), 0.1);
                            DrawingUtilities.DrawPixel(ref texture, Math.Min(screenWidth - 1 - ray.position, screenWidth - 1), Math.Min(Math.Abs((screenHeight / 2) - (ray.wallHeight / 1) + (texY * (1))), screenHeight - 1), stride, texel, 254, true);
                        }
                    }
                }
            });

            // Spritecasting
            //for (int i = entities.Length - 1; i >= 0; i--)
            Parallel.For(0, entities.Length, i =>
            {
                Color texel = Color.Red;
                Entity entity = entities[entities.Length - 1 - i];
                double xRel = entity.xPos - player.xPos;
                double yRel = entity.yPos - player.yPos;
                double invDet = 1 / ((playerControl.planeX * baseDirY) - (baseDirX * playerControl.planeY));
                if (double.IsInfinity(invDet)) invDet = 1;
                double transformX = invDet * ((baseDirY * xRel) - (baseDirX * yRel));
                double transformY = invDet * ((-playerControl.planeY * xRel) + (playerControl.planeX * yRel));
                if (Math.Abs(transformX) < 0.01) transformX = 0.01 * Math.Sign(transformX);
                if (Math.Abs(transformY) < 0.01) transformY = 0.01 * Math.Sign(transformY);
                int spriteScreenX = Convert.ToInt32(Math.Round((screenWidth / 2) * (1 + (transformX / transformY))));
                int spriteHeight = Convert.ToInt32(Math.Abs(Convert.ToInt32(Math.Round(screenHeight / transformY))) * 2 * (1 - entity.compression));
                int offsetY = (Convert.ToInt32(Math.Abs(Convert.ToInt32(Math.Round(screenHeight / transformY))) * 2) - Convert.ToInt32(Math.Abs(Convert.ToInt32(Math.Round(screenHeight / transformY))) * 2 * (1 - entity.compression))) - Convert.ToInt32((entity.zPos / transformY) * 5);
                int drawStartY = Math.Max((screenHeight / 2) - (spriteHeight / 2) + (offsetY / 2), 0);
                int drawEndY = Math.Min((screenHeight / 2) + (Math.Abs(Convert.ToInt32(Math.Round(screenHeight / transformY)))), screenHeight - 0);
                int spriteWidth = Convert.ToInt32(Math.Abs(Convert.ToInt32(Math.Round(screenHeight / transformY))) * 2 * (entity.compression + 1));
                int drawStartX = (-spriteWidth / 2) + spriteScreenX;
                int drawEndX = (spriteWidth / 2) + spriteScreenX;
                if ((Math.Sqrt(Math.Pow(xRel, 2) + Math.Pow(yRel, 2)) <= 3) && (!entity.dead) && (entity.enemy))
                {
                    if (spriteScreenX <= 0) warningLeft = true;
                    if (spriteScreenX >= screenWidth) warningRight = true;
                }
                //Parallel.For(drawStartX, drawEndX, stripe =>
                for (int stripe = drawStartX; stripe < drawEndX; stripe++)
                {
                    if ((transformY > 0) && (stripe >= 0) && (stripe < screenWidth) && ((transformY < rays[Math.Min(stripe, rays.Length - 1)].perpWallDist) || ((rays[Math.Min(stripe, rays.Length - 1)].subRay != null) && (transformY < rays[Math.Min(stripe, rays.Length - 1)].subRay.perpWallDist))))
                    {
                        int texX = Math.Abs(Convert.ToInt32((stripe - drawStartX) * Math.Max((texWidth / Math.Max(spriteWidth, 0.01)), 0.01)));
                        for (int y = drawStartY; y < drawEndY; y++)
                        {
                            int d = ((y - (offsetY / 2)) * 256) - (screenHeight * 128) + (spriteHeight * 128);
                            int texY = Math.Abs((d * texHeight) / Math.Max(spriteHeight, 1)) / 256;
                            texel = DrawingUtilities.TexturePixel(entity.sprite, texX, texY, texWidth, texHeight);
                            if ((texel != Color.FromArgb(236, 84, 220)) && ((DrawingUtilities.TexturePixel(texture, stripe, y, screenWidth, screenHeight).A < 254 - entity.order) || (DrawingUtilities.TexturePixel(texture, stripe, y, screenWidth, screenHeight).A >= 254)))
                            {
                                if ((currentBoard.environment == Environment.Haunted) && (entity.enemy)) texel = ColorUtilities.Mix(ColorUtilities.Desaturate(texel, 0.2), Color.LightSeaGreen, 0.4);
                                if (entity.darken)
                                {
                                    if (currentBoard.environment == Environment.Haunted) texel = ColorUtilities.Multiply(texel, Math.Min(Math.Max(12 - entity.depth, 0) / entity.depth, 1));
                                    else texel = ColorUtilities.Multiply(texel, Math.Min(5 / entity.depth, 1));
                                }
                                if (entity.white > 0) texel = ColorUtilities.Mix(texel, Color.White, entity.white * 2);
                                if (entity.opacity < 1) texel = ColorUtilities.Mix(DrawingUtilities.TexturePixel(texture, stripe, y, screenWidth, screenHeight), texel, entity.opacity);
                                if (transformY < rays[Math.Min(stripe, rays.Length - 1)].perpWallDist) DrawingUtilities.DrawPixel(ref texture, stripe, y, stride, texel, 254 - entity.order, false);
                                else DrawingUtilities.DrawPixel(ref texture, stripe, y, stride, ColorUtilities.Multiply(texel, 1 - (((transformY - rays[Math.Min(stripe, rays.Length - 1)].perpWallDist) / 10)) * (1 - Math.Abs(rays[Math.Min(stripe, rays.Length - 1)].tile.hOffset + rays[Math.Min(stripe, rays.Length - 1)].tile.vOffset))), 254 - entity.order, true);
                                if ((stripe == targetX) && (y == targetY) && (transformY < targetedDistance) && (!targetedEntities.Contains(entity)))
                                {
                                    targetedEntities[i] = entity;
                                    targetedDistances[i] = transformY;
                                    // Cannot use colour in switch statement
                                    if (entity.damageMap == null) targetedMultipliers[i] = 1;
                                    else
                                    {
                                        Color damageColor = DrawingUtilities.TexturePixel(entity.damageMap, texX, texY, texWidth, texHeight);
                                        if (damageColor == Color.FromArgb(255, 255, 45, 45)) targetedMultipliers[i] = 0.5; // Red ( weak hit / lower damage )
                                        else if (damageColor == Color.FromArgb(255, 255, 134, 45)) targetedMultipliers[i] = 1; // Orange ( average hit / normal damage )
                                        else if (damageColor == Color.FromArgb(255, 63, 227, 67)) targetedMultipliers[i] = 1.5; // Green ( strong hit / higher damage )
                                        else targetedMultipliers[i] = 1; // No Color ( error / normal damage )
                                    }
                                }
                            }
                            else if ((entity.zPos > 1) && (y == drawEndY - 1) && (y != screenHeight - 1) && (stripe > drawStartX + ((drawEndX - drawStartX) / 3) + (entity.zPos / 10)) && (stripe < drawEndX - ((drawEndX - drawStartX) / 3) - (entity.zPos / 10))) DrawingUtilities.DrawPixel(ref texture, stripe, y, stride, Color.Black, 254, true);
                        }
                    }
                }//);
            });

            // Targeting Stats
            for (int i = 0; i < targetedEntities.Length; i++)
            {
                if (targetedEntities[i] != null)
                {
                    playerControl.targetedEntities.Add(targetedEntities[i]);
                    playerControl.targetedDistances.Add(targetedDistances[i]);
                    playerControl.targetedMultipliers.Add(targetedMultipliers[i]);
                }
            }

            // Crosshair
            for (int i = 0; i < crosshairWidth; i++) if (Math.Abs(i - (crosshairWidth / 2)) > (crosshairWidth / 6))
            {
                DrawingUtilities.DrawPixel(ref texture, targetX - (crosshairWidth / 2) + i, targetY, stride, ColorUtilities.Negative(DrawingUtilities.TexturePixel(texture, targetX - (crosshairWidth / 2) + i, targetY, screenWidth, screenHeight)), 255, false);
                DrawingUtilities.DrawPixel(ref texture, targetX - (crosshairWidth / 2) + i, targetY - 1, stride, ColorUtilities.Negative(DrawingUtilities.TexturePixel(texture, targetX - (crosshairWidth / 2) + i, targetY - 1, screenWidth, screenHeight)), 255, false);
            }
            for (int i = 0; i < crosshairHeight; i++) if (Math.Abs(i - (crosshairHeight / 2)) > (crosshairHeight / 6))
            {
                DrawingUtilities.DrawPixel(ref texture, targetX, targetY - (crosshairHeight / 2) + i, stride, ColorUtilities.Negative(DrawingUtilities.TexturePixel(texture, targetX, targetY - (crosshairHeight / 2) + i, screenWidth, screenHeight)), 255, false);
                DrawingUtilities.DrawPixel(ref texture, targetX - 1, targetY - (crosshairHeight / 2) + i, stride, ColorUtilities.Negative(DrawingUtilities.TexturePixel(texture, targetX - 1, targetY - (crosshairHeight / 2) + i, screenWidth, screenHeight)), 255, false);
            }

            //if (desaturate) DrawingUtilities.DiscolorSprite(texture, screenWidth, screenHeight, 1 - Math.Min(pScale * 2, 0.5), 1 - Math.Min(pScale * 2, 0.5));
            if (!playerControl.active) DrawingUtilities.DiscolorSprite(texture, screenWidth, screenHeight, 1 - (goYpos / 480), 1 - (goYpos / 240));

            return texture;
        }
        public Byte[] DrawUI(Byte[] texture, int stride, Board targetBoard, Player playerControl, bool drawMap, bool desaturate)
        {
            Entity player = playerControl.entity; // Set a reference to the player's entity for easy calling

            // Hands
            Byte[] leftHandGraphic;
            Byte[] rightHandGraphic;
            if ((playerControl.leftHandY > playerControl.leftHandBaseY - 4) || (playerControl.hand) || (playerControl.punch <= 0)) leftHandGraphic = DrawingUtilities.LockBitmap(Properties.Resources.LeftHandFist);
            else leftHandGraphic = DrawingUtilities.LockBitmap(Properties.Resources.LeftHandPunch);
            if ((playerControl.rightHandY > playerControl.rightHandBaseY - 4) || (!playerControl.hand) || (playerControl.punch <= 0)) rightHandGraphic = DrawingUtilities.LockBitmap(Properties.Resources.RightHandFist);
            else rightHandGraphic = DrawingUtilities.LockBitmap(Properties.Resources.RightHandPunch);
            if (!playerControl.hand) DrawingUtilities.DrawSprite(ref texture, stride, screenWidth, screenHeight, leftHandGraphic, (screenWidth / 2) - 48 + Convert.ToInt32(playerControl.leftHandX), screenHeight - 16 + Convert.ToInt32(playerControl.leftHandY), 64, 48, AlignH.Left, AlignV.Bottom);
            DrawingUtilities.DrawSprite(ref texture, stride, screenWidth, screenHeight, rightHandGraphic, (screenWidth / 2) + 48 + Convert.ToInt32(playerControl.rightHandX), screenHeight - 16 + Convert.ToInt32(playerControl.rightHandY), 64, 48, AlignH.Right, AlignV.Bottom);
            if (playerControl.hand) DrawingUtilities.DrawSprite(ref texture, stride, screenWidth, screenHeight, leftHandGraphic, (screenWidth / 2) - 48 + Convert.ToInt32(playerControl.leftHandX), screenHeight - 16 + Convert.ToInt32(playerControl.leftHandY), 64, 48, AlignH.Left, AlignV.Bottom);

            // Map
            if ((drawMap) && (!paused) && (playerControl.active))
            {
                DrawingUtilities.DiscolorSprite(texture, screenWidth, screenHeight, 0.7, 0.7);
                int startX = (screenWidth / 2) - (currentBoard.boardWidth / 2);
                int startY = (screenHeight / 2) - (currentBoard.boardHeight / 2);
                for (int x = startX; x < Math.Min(startX + currentBoard.boardWidth, screenWidth); x++)
                {
                    for (int y = startY; y < Math.Min(startY + currentBoard.boardHeight, screenHeight); y++)
                    {
                        Color texel = Color.White;
                        if ((x - startX == Math.Floor(player.xPos)) && (y - startY == Math.Floor(player.yPos))) texel = Color.Red;
                        else if (currentBoard.boardLayout[y - startY, x - startX].ID == 7) texel = Color.Green;
                        else if (currentBoard.boardLayout[y - startY, x - startX].ID != 0) texel = Color.White;
                        else texel = Color.Blue;
                        DrawingUtilities.DrawPixel(ref texture, x, y, stride, texel, 255, false);
                        //ColorNegative(DrawingUtilities.TexturePixel(rgbValues, x, y, screenWidth, screenHeight))
                    }
                }
                for (int i = 0; i < entitiesList.Count; i++)
                {
                    if (entitiesList[i].entityType == EntityType.Key) DrawingUtilities.DrawPixel(ref texture, startX + Convert.ToInt32(entitiesList[i].xPos - 0.5), startY + Convert.ToInt32(entitiesList[i].yPos - 0.5), stride, Color.Orange, 255, false);
                }
            }

            // HP Bar
            DrawingUtilities.DrawSprite(ref texture, stride, screenWidth, screenHeight, playerControl.interfaceSprites[0], (screenWidth / 2) - (playerControl.hpWidth / 2), screenHeight, 8, 8, AlignH.Right, AlignV.Bottom);
            DrawingUtilities.DrawSprite(ref texture, stride, screenWidth, screenHeight, playerControl.interfaceSprites[1], (screenWidth / 2) + (playerControl.hpWidth / 2), screenHeight, 8, 8, AlignH.Left, AlignV.Bottom);
            int hpDisplay = Convert.ToInt32((playerControl.hpWidth / Convert.ToDouble(playerControl.hpMax)) * playerControl.hpDisplay);
            int hpTrailDisplay = Math.Max(Convert.ToInt32((playerControl.hpWidth / Convert.ToDouble(playerControl.hpMax)) * playerControl.hpTrail) - hpDisplay, 0);
            int currentPos = 0;
            List<int> hpSegmentValues = new List<int>();
            for (int i = 0; i < playerControl.hpSegments; i++)
            {
                hpSegmentValues.Add(currentPos);
                currentPos += playerControl.hpWidth / playerControl.hpSegments;
                hpSegmentValues.Add(currentPos - 1);
            }
            currentPos = (screenWidth / 2) - (playerControl.hpWidth / 2);
            for (int xPos = currentPos; xPos < currentPos + hpDisplay; xPos++)
            {
                if ((xPos == currentPos) || (xPos == currentPos + hpDisplay - 1) || (hpSegmentValues.Contains(xPos - currentPos))) DrawingUtilities.DrawSprite(ref texture, stride, screenWidth, screenHeight, playerControl.interfaceSprites[4], xPos, screenHeight, 1, 8, AlignH.Left, AlignV.Bottom);
                else DrawingUtilities.DrawSprite(ref texture, stride, screenWidth, screenHeight, playerControl.interfaceSprites[3], xPos, screenHeight, 1, 8, AlignH.Left, AlignV.Bottom);
            }
            currentPos += hpDisplay;
            for (int xPos = currentPos; xPos < currentPos + hpTrailDisplay; xPos++)
            {
                if ((xPos == currentPos) || (xPos == currentPos + hpTrailDisplay - 1)) DrawingUtilities.DrawSprite(ref texture, stride, screenWidth, screenHeight, playerControl.interfaceSprites[6], xPos, screenHeight, 1, 8, AlignH.Left, AlignV.Bottom);
                else DrawingUtilities.DrawSprite(ref texture, stride, screenWidth, screenHeight, playerControl.interfaceSprites[5], xPos, screenHeight, 1, 8, AlignH.Left, AlignV.Bottom);
            }
            currentPos += hpTrailDisplay;
            for (int xPos = currentPos; xPos < (screenWidth / 2) + (playerControl.hpWidth / 2); xPos++)
            {
                DrawingUtilities.DrawSprite(ref texture, stride, screenWidth, screenHeight, playerControl.interfaceSprites[2], xPos, screenHeight, 1, 8, AlignH.Left, AlignV.Bottom);
            }

            // Ammo Bar
            if ((playerControl.weapons.Count > playerControl.weapon) && (playerControl.weapons[playerControl.weapon] != null) && (playerControl.weapons[playerControl.weapon].ammoMax > 0))
            {
                Weapon weapon = playerControl.weapons[playerControl.weapon];
                int ammoWidth = (weapon.ammoMax * 2) + weapon.ammoMax + weapon.ammoSegments - 2;
                DrawingUtilities.DrawSprite(ref texture, stride, screenWidth, screenHeight, playerControl.interfaceSprites[7], (screenWidth / 2) - (ammoWidth / 2), screenHeight - 8, 4, 4, AlignH.Right, AlignV.Bottom);
                DrawingUtilities.DrawSprite(ref texture, stride, screenWidth, screenHeight, playerControl.interfaceSprites[8], (screenWidth / 2) + (ammoWidth / 2), screenHeight - 8, 4, 4, AlignH.Left, AlignV.Bottom);
                currentPos = (screenWidth / 2) - (ammoWidth / 2);
                for (int i = 0; i < weapon.ammoMax; i++)
                {
                    if (i < weapon.ammoValue) DrawingUtilities.DrawSprite(ref texture, stride, screenWidth, screenHeight, playerControl.interfaceSprites[10], currentPos, screenHeight - 8, 2, 4, AlignH.Left, AlignV.Bottom);
                    else DrawingUtilities.DrawSprite(ref texture, stride, screenWidth, screenHeight, playerControl.interfaceSprites[11], currentPos, screenHeight - 8, 2, 4, AlignH.Left, AlignV.Bottom);
                    DrawingUtilities.DrawSprite(ref texture, stride, screenWidth, screenHeight, playerControl.interfaceSprites[9], currentPos + 2, screenHeight - 8, 1, 4, AlignH.Left, AlignV.Bottom);
                    currentPos += 3;
                    if (((i + 1) % (weapon.ammoMax / weapon.ammoSegments) == 0) && (i < weapon.ammoMax - 1))
                    {
                        DrawingUtilities.DrawSprite(ref texture, stride, screenWidth, screenHeight, playerControl.interfaceSprites[9], currentPos, screenHeight - 8, 1, 4, AlignH.Left, AlignV.Bottom);
                        currentPos++;
                    }
                }
            }

            // Score Display
            Color backColor = Color.FromArgb(255, 98, 36, 97);
            DrawingUtilities.DrawSprite(ref texture, stride, screenWidth, screenHeight, DrawingUtilities.LockBitmap(Properties.Resources.ScoreDisplay), screenWidth / 2, 0, 72, 17, AlignH.Center, AlignV.Top);
            string tempText = score.ToString();
            for (int i = 0; i < tempText.Length; i++) DrawingUtilities.DrawText(ref texture, stride, screenWidth, screenHeight, tempText[tempText.Length - 1 - i].ToString(), (screenWidth / 2) + 21 - (i * 7) - Convert.ToInt32(Math.Floor(i / 3.0) * 3), 0, fontTeapot, Color.White, 1);
            for (int i = tempText.Length; i < 7; i++) DrawingUtilities.DrawText(ref texture, stride, screenWidth, screenHeight, "0", (screenWidth / 2) + 21 - (i * 7) - Convert.ToInt32(Math.Floor(i / 3.0) * 3), 0, fontTeapot, backColor, 1);
            int comboDisplay = 0;
            if (comboTimerMax > 0) comboDisplay = Convert.ToInt32((50.0 / comboTimerMax) * comboTimer);
            for (int x = 0; x < comboDisplay; x++) DrawingUtilities.DrawPixel(ref texture, (screenWidth / 2) - 25 + x, 9, stride, Color.White, 255, false);
            for (int x = comboDisplay; x < 50; x++) DrawingUtilities.DrawPixel(ref texture, (screenWidth / 2) - 25 + x, 9, stride, backColor, 255, false);
            Byte[] tempTexture;
            if (tempText.Length > 3) tempTexture = DrawingUtilities.LockBitmap(Properties.Resources.ScoreComma);
            else tempTexture = DrawingUtilities.LockBitmap(Properties.Resources.ScoreCommaBack);
            DrawingUtilities.DrawSprite(ref texture, stride, screenWidth, screenHeight, tempTexture, (screenWidth / 2) + 3, 6, 4, 5, AlignH.Left, AlignV.Top);
            if (tempText.Length > 6) tempTexture = DrawingUtilities.LockBitmap(Properties.Resources.ScoreComma);
            else tempTexture = DrawingUtilities.LockBitmap(Properties.Resources.ScoreCommaBack);
            DrawingUtilities.DrawSprite(ref texture, stride, screenWidth, screenHeight, tempTexture, (screenWidth / 2) - 21, 6, 4, 5, AlignH.Left, AlignV.Top);
            if (comboTimerMax > 0) tempTexture = DrawingUtilities.LockBitmap(Properties.Resources.ScoreCombo);
            else tempTexture = DrawingUtilities.LockBitmap(Properties.Resources.ScoreComboBack);
            DrawingUtilities.DrawSprite(ref texture, stride, screenWidth, screenHeight, tempTexture, (screenWidth / 2) - 19, 11, 26, 5, AlignH.Left, AlignV.Top);
            tempText = comboCache.ToString();
            if (comboTimerMax <= 0) tempText = "";
            for (int i = 0; i < tempText.Length; i++) DrawingUtilities.DrawText(ref texture, stride, screenWidth, screenHeight, tempText[tempText.Length - 1 - i].ToString(), (screenWidth / 2) - 3 - (i * 5) - Convert.ToInt32(Math.Floor(i / 3.0) * 3), 11, fontHaptic, Color.White, 1);
            for (int i = tempText.Length; i < 3; i++) DrawingUtilities.DrawText(ref texture, stride, screenWidth, screenHeight, "0", (screenWidth / 2) - 3 - (i * 5) - Convert.ToInt32(Math.Floor(i / 3.0) * 3), 11, fontHaptic, backColor, 1);
            tempText = combo.ToString();
            if (comboTimerMax <= 0) tempText = "";
            for (int i = 0; i < tempText.Length; i++) DrawingUtilities.DrawText(ref texture, stride, screenWidth, screenHeight, tempText[tempText.Length - 1 - i].ToString(), (screenWidth / 2) + 13 - (i * 5) - Convert.ToInt32(Math.Floor(i / 3.0) * 3), 11, fontHaptic, Color.White, 1);
            for (int i = tempText.Length; i < 2; i++) DrawingUtilities.DrawText(ref texture, stride, screenWidth, screenHeight, "0", (screenWidth / 2) + 13 - (i * 5) - Convert.ToInt32(Math.Floor(i / 3.0) * 3), 11, fontHaptic, backColor, 1);

            // Enemy Warnings
            if (warningLeft) DrawingUtilities.DrawSprite(ref texture, stride, screenWidth, screenHeight, DrawingUtilities.LockBitmap(Properties.Resources.WarningLeft), 8, screenHeight / 2, 22, 16, AlignH.Left, AlignV.Center);
            if (warningRight) DrawingUtilities.DrawSprite(ref texture, stride, screenWidth, screenHeight, DrawingUtilities.LockBitmap(Properties.Resources.WarningRight), screenWidth - 8, screenHeight / 2, 22, 16, AlignH.Right, AlignV.Center);

            // Debug Text
            DrawingUtilities.DrawText(ref texture, stride, screenWidth, screenHeight, "Keys: " + playerControl.keysHeld.ToString(), 1, 0, fontTeapot, Color.White, 1);
            DrawingUtilities.DrawText(ref texture, stride, screenWidth, screenHeight, "Ammo: " + Math.Round(playerControl.ammoDisplay).ToString(), 1, 9, fontTeapot, Color.White, 1);
            //DrawingUtilities.DrawText(ref texture, stride, screenWidth, screenHeight, "Using " + playerControl.weapons[playerControl.weapon].name, 1, 9, fontTeapot, Color.White, 1);
            DrawingUtilities.DrawText(ref texture, stride, screenWidth, screenHeight, "Using " + playerControl.weapons[playerControl.weapon].name, 1, 18, fontTeapot, Color.White, 1);

            //if (desaturate) texture = DrawingUtilities.DiscolorSprite(texture, screenWidth, screenHeight, 1 - Math.Min(pScale * 2, 0.5), 1 - Math.Min(pScale * 2, 0.5));s
            if (!playerControl.active) texture = DrawingUtilities.DiscolorSprite(texture, screenWidth, screenHeight, 1 - (goYpos / 480), 1 - (goYpos / 240));

            return texture;
        }
        public Byte[] DrawPauseScreen(Byte[] texture, int stride)
        {
            //Paused
            //rgbValues = DrawingUtilities.DiscolorSprite(rgbValues, screenWidth, screenHeight, 1 - Math.Min(pScale * 2, 0.5), 1 - Math.Min(pScale * 2, 0.5));
            texture = DrawingUtilities.DrawSpriteScaled(texture, stride, screenWidth, screenHeight, DrawingUtilities.LockBitmap(Properties.Resources.Paused), screenWidth / 2, 42, pScale, pScale, 512, 256, AlignH.Center, AlignV.Center);
            if (paused)
            {
                int xOffset = -64;
                texture = DrawingUtilities.DrawTextOutline(texture, stride, screenWidth, screenHeight, ">Resume", (screenWidth / 2) + xOffset + Convert.ToInt32((pScale - 0.3) * 20), (screenHeight / 2) + 8, fontTeapot, Color.Black, 1, 1, 20);
                DrawingUtilities.DrawText(ref texture, stride, screenWidth, screenHeight, ">Resume", (screenWidth / 2) + xOffset + Convert.ToInt32((pScale - 0.3) * 20), (screenHeight / 2) + 8, fontTeapot, Color.White, 1);
                texture = DrawingUtilities.DrawTextOutline(texture, stride, screenWidth, screenHeight, "Options", (screenWidth / 2) + xOffset + Convert.ToInt32((pScale - 0.3) * 40), (screenHeight / 2) + 18, fontTeapot, Color.Black, 1, 1, 20);
                DrawingUtilities.DrawText(ref texture, stride, screenWidth, screenHeight, "Options", (screenWidth / 2) + xOffset + Convert.ToInt32((pScale - 0.3) * 40), (screenHeight / 2) + 18, fontTeapot, Color.White, 1);
                texture = DrawingUtilities.DrawTextOutline(texture, stride, screenWidth, screenHeight, "Exit", (screenWidth / 2) + xOffset + Convert.ToInt32((pScale - 0.3) * 60), (screenHeight / 2) + 28, fontTeapot, Color.Black, 1, 1, 20);
                DrawingUtilities.DrawText(ref texture, stride, screenWidth, screenHeight, "Exit", (screenWidth / 2) + xOffset + Convert.ToInt32((pScale - 0.3) * 60), (screenHeight / 2) + 28, fontTeapot, Color.White, 1);
                texture = DrawingUtilities.DrawTextOutline(texture, stride, screenWidth, screenHeight, "Volume: " + soundVolume.ToString(), 4, 4, fontTeapot, Color.Black, 1, 1, 20);
                DrawingUtilities.DrawText(ref texture, stride, screenWidth, screenHeight, "Volume: " + soundVolume.ToString(), 4, 4, fontTeapot, Color.White, 1);
            }
            return texture;
        }
        public Byte[] DrawGameOverScreen(Byte[] texture, int stride)
        {
            // Game Over
            texture = DrawingUtilities.DrawSpriteScaled(texture, stride, screenWidth, screenHeight, DrawingUtilities.LockBitmap(Properties.Resources.BloodTexture), 0, Convert.ToInt32(goYpos) - 360, 1, 1, 240, 240, AlignH.Left, AlignV.Top);
            string text = "Dungeon Seed was " + currentBoard.seed;
            DrawingUtilities.DrawText(ref texture, stride, screenWidth, screenHeight, text.Substring(0, Convert.ToInt32(Math.Min(Math.Abs(goOffset), text.Length))), 16, (screenHeight / 2) + 2, fontTeapot, Color.White, 1);
            text = "Your Score was " + DrawingUtilities.FormatNumer(score);
            DrawingUtilities.DrawText(ref texture, stride, screenWidth, screenHeight, text.Substring(0, Convert.ToInt32(Math.Min(Math.Abs(goOffset), text.Length))), 16, (screenHeight / 2) + 12, fontTeapot, Color.White, 1);
            texture = DrawingUtilities.DrawSpriteScaled(texture, stride, screenWidth, screenHeight, DrawingUtilities.LockBitmap(Properties.Resources.GameOver), screenWidth / 2, (screenHeight / 2) + Convert.ToInt32(goOffset), goScale, goScale, 512, 256, AlignH.Center, AlignV.Center);
            return texture;
        }
        public void DrawMap(ref Board targetBoard)
        {
            int width = targetBoard.layoutWidth;
            int height = targetBoard.layoutHeight;
            Bitmap drawTarget = new Bitmap(width, height);
            Graphics graphic = this.CreateGraphics();

            Rectangle bounds = new Rectangle(0, 0, width, height);
            //System.Drawing.Imaging.BitmapData m_BitmapData = drawTarget.LockBits(bounds, Imaging.ImageLockMode.ReadWrite, Imaging.PixelFormat.Format24bppRgb);
            System.Drawing.Imaging.BitmapData bmpData = drawTarget.LockBits(bounds, System.Drawing.Imaging.ImageLockMode.ReadWrite, drawTarget.PixelFormat);

            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * drawTarget.Height;
            Byte[] rgbValues = new Byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

            //Map
            Color texel = Color.White;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (((x == 1) && (y == 1)) || ((x == width - 2) && (y == height - 2))) texel = Color.Green;
                    else if (currentBoard.boardConcept[y, x] == 4) texel = Color.FromArgb(255, 49, 162, 242);
                    else if (currentBoard.boardConcept[y, x] == 1) texel = Color.White;
                    else if (currentBoard.boardConcept[y, x] == 2) texel = Color.Red;
                    else texel = Color.Blue;
                    /*if (tile == Color.FromArgb(255, 255, 255, 255)) room[y, x] = 0; // Blank Space
                    else if (tile == Color.FromArgb(255, 27, 38, 50)) room[y, x] = 1; // Base Wall
                    else if (tile == Color.FromArgb(255, 47, 72, 78)) room[y, x] = 2; // Decorative Wall
                    else if (tile == Color.FromArgb(255, 235, 137, 49)) room[y, x] = 4; // Push Wall
                    else if (tile == Color.FromArgb(255, 0, 87, 132)) room[y, x] = 6; // Gate
                    else if (tile == Color.FromArgb(255, 49, 162, 242)) room[y, x] = 7; // Locked Gate
                    else if (tile == Color.FromArgb(255, 178, 220, 239)) room[y, x] = 8; // Unlocked Gate
                    else if (tile == Color.FromArgb(255, 190, 38, 51)) room[y, x] = -1; // Enemy Spawn
                    else if (tile == Color.FromArgb(255, 163, 206, 39)) room[y, x] = -4; // Base Loot
                    else if (tile == Color.FromArgb(255, 68, 137, 26)) room[y, x] = -5; // Locked Loot
                    else if (tile == Color.FromArgb(255, 7, 203, 111)) room[y, x] = -6; // Hidden Loot*/
                    DrawingUtilities.DrawPixel(ref rgbValues, x, y, bmpData.Stride, texel, 255, false);
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);
            drawTarget.UnlockBits(bmpData);

            ViewBox.Size = new System.Drawing.Size(width * 16, height * 16);
            ViewBox.Image = DrawingUtilities.ResizeImage(drawTarget, width * 16, height * 16);

            graphic.Dispose();
            this.ClientSize = new System.Drawing.Size(width * 16, height * 16);
            this.Refresh();
            drawTarget.Dispose();
        }
        private void PictureBox1_Click(object sender, EventArgs e) { }
        private void GUIBox_Click(object sender, EventArgs e) { }
        public void SetVolume(int volume)
        {
            uint CurrVol = 0;
            waveOutGetVolume(IntPtr.Zero, out CurrVol);
            uint NewVolumeAllChannels = (short.MaxValue / 5) * Convert.ToUInt32(MathUtilities.Clamp(volume, 0, 10));
            waveOutSetVolume(IntPtr.Zero, NewVolumeAllChannels);
        }
    }
}
