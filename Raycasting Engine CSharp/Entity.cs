using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Raycasting_Engine_CSharp
{
    public class Entity
    {
        public EntityType entityType; // Type of entity
        public Board currentBoard; // Board for reference during collision
        public double xPos; // X-axis position
        public double yPos; // Y-axis position
        public double zPos; // Z-axis position
        public double dir = 0; //Direction
        public double moveSpeed = 0; // Movement speed
        public double hsp = 0; // X-axis speed
        public double vsp = 0; // Y-axis speed
        public double zsp = 0; // Z-axis speed
        public double gravity = 0.2; // Gravity value applied to zsp when in air
        public Byte[] sprite = null; // Bitlocked sprite
        public Byte[] damageMap = null;
        public Animation animation = null; // Sprite animation
        public int frame = 0; // Animation frame
        public double opacity = 1; // Opacity of sprite
        public double compression = 0; // Compression for squash and stretch
        public double cDifference = 0; // Compression addition
        public double ticker = 0; // Increasing value for breathing
        public double depth; // Depth value for reference during rendering
        public bool darken = true; // Whether the sprite can be darkened when rendering
        public int order; // Sprite order for reference during rendering
        public double bounce = 0; // Value applied to speed on collision
        public double grip = 0.5; // Value applied to speed per frame
        public double weight = 1; // Multiplier for knockback effects
        public double hitboxSize = 0; // Size of hitbox
        public bool breathe = false; // Entity breathes
        public double white = 0; // White tint value
        public bool consumable = false; // Avaiable to pick up by player
        public bool solid = true; // Collides with other entities on impact
        public int HP = 0; // Health value
        public int hpMax = 0; // Maximum health value
        public bool damagable = false; // Can be attacked by the player
        public bool enemy = false; // Can attack the player
        public int damage = 0; // Damage applied to player on attack
        public bool dead = false; // Is living
        public bool corpse = false; // Is a corpse
        public double scale = 1; // Scale of sprite
        public bool projectile = false; // Is a projectile that damages on impact
        public bool destroyOnImpact = false; // Destroys on first collsion
        public bool garbage = false; // Whether entity is finished with and ready to be thrown away
        public bool collided = false; // Whether the entity collided with a wall the previous frame
        public EntityType contents = EntityType.None; // Item contained within and released when destroyed
        public bool releaseContents = false; // Whether to release the contents
        public Entity(EntityType entityType, Board currentBoard, double xPos, double yPos)
        {
            this.entityType = entityType;
            this.currentBoard = currentBoard;
            this.xPos = xPos;
            this.yPos = yPos;
            switch (entityType)
            {
                case EntityType.Player: switch (currentBoard.environment) { // Player
                        case Environment.Haunted: // Default
                        case Environment.Sewer: moveSpeed = 0.5; bounce = 0; grip = 0.4; hitboxSize = 0.5; solid = false; sprite = DrawingUtilities.LockBitmap(Properties.Resources.Freaker); break; // Default
                        case Environment.Ice: moveSpeed = 0.01; bounce = 0.6; grip = 0.999; hitboxSize = 0.5; solid = false; sprite = DrawingUtilities.LockBitmap(Properties.Resources.Freaker); break; // Slip
                    } break;
                case EntityType.Freaker: switch (currentBoard.environment) { // Enemy - Freaker
                        case Environment.Haunted: // Ghost Freak
                        case Environment.Sewer: moveSpeed = 0.004; bounce = 0.8; grip = 0.8; breathe = true; hitboxSize = 0.4; HP = 100; enemy = true; damage = 20; sprite = DrawingUtilities.LockBitmap(Properties.Resources.Freaker); damageMap = DrawingUtilities.LockBitmap(Properties.Resources.FreakerDamageMap); break; // Standard Freak
                        case Environment.Ice: moveSpeed = 0.0005; bounce = 0.8; grip = 0.99; weight = 4; breathe = true; hitboxSize = 0.4; HP = 100; enemy = true; damage = 20; sprite = DrawingUtilities.LockBitmap(Properties.Resources.Freaker); damageMap = DrawingUtilities.LockBitmap(Properties.Resources.FreakerDamageMap); break; // Ice Freak
                    } break;
                case EntityType.FlyingEnemy: switch (currentBoard.environment) { // Enemy - Flying
                        case Environment.Haunted: // Ghost Flying Enemy
                        case Environment.Sewer: moveSpeed = 0.001; bounce = 0.8; grip = 0.99; weight = 2; breathe = true; hitboxSize = 0.4; HP = 60; enemy = true; damage = 10; sprite = DrawingUtilities.LockBitmap(Properties.Resources.FlyingEnemy); damageMap = DrawingUtilities.LockBitmap(Properties.Resources.FlyingEnemyDamageMap); break; // Standard Flying Enemy
                        case Environment.Ice: moveSpeed = 0.001; bounce = 0.8; grip = 0.99; weight = 2; breathe = true; hitboxSize = 0.4; HP = 60; enemy = true; damage = 10; sprite = DrawingUtilities.LockBitmap(Properties.Resources.FlyingEnemy); damageMap = DrawingUtilities.LockBitmap(Properties.Resources.FlyingEnemyDamageMap); break; // Ice Flying Enemy
                    }
                    break;
                case EntityType.Barrel: switch (currentBoard.environment) { // Object - Barrel
                        case Environment.Haunted: // Wooden
                        case Environment.Sewer: moveSpeed = 0; bounce = 0.8; grip = 0.6; hitboxSize = 0.3; HP = 100; sprite = DrawingUtilities.LockBitmap(Properties.Resources.WoodenBarrel); break; // Wooden
                        case Environment.Ice: moveSpeed = 0; bounce = 0.8; grip = 0.99; weight = 4; hitboxSize = 0.3; HP = 100; sprite = DrawingUtilities.LockBitmap(Properties.Resources.MetalBarrel); break; // Metal
                    } break;
                case EntityType.Key: // Consumable - Key
                    moveSpeed = 0; bounce = 0.4; grip = 0.6; hitboxSize = 0.3; sprite = DrawingUtilities.LockBitmap(Properties.Resources.Key); solid = false; consumable = true; compression = 1; break; // Golden Key
                case EntityType.Health: // Consumable - Health
                    bounce = 0.4; grip = 0.6; hitboxSize = 0.3; sprite = DrawingUtilities.LockBitmap(Properties.Resources.HealthPack); solid = false; consumable = true; compression = 1; break; // Health Kit
                case EntityType.Ammo: // Consumable - Ammo
                    bounce = 0.4; grip = 0.6; hitboxSize = 0.3; sprite = DrawingUtilities.LockBitmap(Properties.Resources.AmmoBox); solid = false; consumable = true; compression = 1; break; // Ammo Crate
                case EntityType.Basketball: switch (currentBoard.environment) { // Object - Basketball
                        case Environment.Haunted:
                        case Environment.Sewer: bounce = 0.99; grip = 0.995; weight = 4; hitboxSize = 0.2; damagable = true; breathe = false; sprite = DrawingUtilities.LockBitmap(Properties.Resources.Basketball); zPos = 32; compression = 1; break; // Basketball
                        case Environment.Ice: bounce = 0.999; grip = 0.999; weight = 4; hitboxSize = 0.2; damagable = true; breathe = false; sprite = DrawingUtilities.LockBitmap(Properties.Resources.Basketball); zPos = 32; compression = 1; break; // Basketball
                    } break;
                case EntityType.Poof: // Effect - Poof
                    grip = 0; darken = false; animation = new Animation(Properties.Resources.Poof, 64, 64, 6, 6, 32, 2, false); solid = false; break; // White Smoke Cloud
                case EntityType.Slash: // Effect - Slash
                    grip = 0; darken = false; animation = new Animation(Properties.Resources.Slash, 64, 64, 4, 3, 12, 1, false); solid = false; break; // White Burst
                case EntityType.Rocket: // Projectile - Rocket
                    zPos = 24; damage = 200; gravity = 0; bounce = 1; grip = 1; hitboxSize = 0.5; sprite = DrawingUtilities.LockBitmap(Properties.Resources.Basketball); projectile = true; destroyOnImpact = true; break; // Rocket
                default: grip = 0; sprite = DrawingUtilities.LockBitmap(Properties.Resources.Freaker); break; // Backup Object
            }
            if (HP > 0) damagable = true;
            hpMax = HP;
        }
        public void Update(double delta)
        {
            if (garbage) return;
            // Direction Handling
            dir = dir % (Math.PI * 2);
            if (dir < 0) dir += (Math.PI * 2);
            // Z-Axis Handling
            if (zPos > 0) zsp -= gravity;
            if (zsp != 0) ZMove(ref zsp);
            zPos += zsp;
            // Perform collision checks (one for each corner of the hitbox)
            if ((hsp != 0) || (vsp != 0)) // Don't bother if the entity isn't even moving
            {
                bool collision = false; // Unused boolean that represents whether a collision has been detected
                collision = Move(ref hsp, ref vsp, hitboxSize / 2, hitboxSize / 2);
                collision = Move(ref hsp, ref vsp, hitboxSize / 2, -hitboxSize / 2);
                collision = Move(ref hsp, ref vsp, -hitboxSize / 2, hitboxSize / 2);
                collision = Move(ref hsp, ref vsp, -hitboxSize / 2, -hitboxSize / 2);
            }
            // X-Axis + Y-Axis Movement Handling
            xPos += hsp; yPos += vsp; // Add velocity to position
            hsp *= grip; vsp *= grip; // Lower velocity by friction
            hsp = MathUtilities.Clamp(hsp, -0.3, 0.3); vsp = MathUtilities.Clamp(vsp, -0.3, 0.3); // Restrict speed to prevent clipping through walls
            // Round speed to zero if it is really small (more visually appealing when using ice friction)
            if (Math.Abs(hsp) < 0.0001) hsp = 0;
            if (Math.Abs(vsp) < 0.0001) vsp = 0;
            // Compression Handling
            if ((breathe) && (!dead)) ticker += 0.2; // Ticker and trigonometry to create breathing wave pattern
            if ((!dead) || (corpse)) cDifference = MathUtilities.Lerp(cDifference, ((Math.Sin(ticker) / 20) - compression) * 0.8, 0.2);
            else
            {
                white = 1;
                cDifference = MathUtilities.Lerp(cDifference, (1 - compression) * 0.8, 0.05);
                if (compression > 0.9)
                {
                    corpse = true;
                    if (entityType == EntityType.Barrel)
                    {
                        sprite = DrawingUtilities.LockBitmap(Properties.Resources.WoodenBarrelBroken);
                        Random rnd = new Random();
                        if (rnd.Next(0, 2) == 1)
                        {
                            rnd = new Random();
                            if (rnd.Next(0, 2) == 1) contents = EntityType.Health;
                            else contents = EntityType.Ammo;
                        }
                    }
                    else sprite = DrawingUtilities.LockBitmap(Properties.Resources.FreakerCorpse);
                    releaseContents = true;
                }
            }
            compression = MathUtilities.Clamp(compression + cDifference, -1, 1); // Difference used to create squash and stretch
            // White Value Handling
            if (white > 0) white -= 0.1; // White flash used to highlight an attack
            else white = 0;
            // Sprite Handling
            if (animation != null)
            {
                if (animation.frames.Count > frame) sprite = animation.frames[frame];
                frame += animation.speed;
                if (frame >= animation.frames.Count)
                {
                    if (animation.loop) frame = 0;
                    else garbage = true;
                }
            }
        }
        public bool CheckCollision(Entity entity, double hitbox)
        {
            if ((Math.Sqrt(Math.Pow(entity.xPos - xPos, 2) + Math.Pow(entity.yPos - yPos, 2)) < hitbox)) return true;
            else return false;
        }
        bool Move(ref double hsp, ref double vsp, double xOffset, double yOffset)
        {
            bool collision = false;
            if ((hsp != 0) && (currentBoard.BoardPoint(xPos + xOffset + hsp, yPos + yOffset).solid))
            {
                double loop = 0;
                while ((!currentBoard.BoardPoint(xPos + xOffset + Math.Sign(hsp), yPos + yOffset).solid) && (xPos > 0) && (xPos < currentBoard.boardWidth) && (loop <= Math.Abs(hsp)))
                {
                    xPos += Math.Sign(hsp) / 100.0;
                    loop += 0.01;
                }
                hsp *= -bounce;
                collision = true;
            }
            if ((vsp != 0) && (currentBoard.BoardPoint(xPos + xOffset, yPos + yOffset + vsp).solid))
            {
                double loop = 0;
                while ((!currentBoard.BoardPoint(xPos + xOffset, yPos + yOffset + Math.Sign(vsp)).solid) && (yPos > 0) && (yPos < currentBoard.boardHeight) && (loop <= Math.Abs(vsp)))
                {
                    yPos += Math.Sign(vsp) / 100.0;
                    loop += 0.01;
                }
                vsp *= -bounce;
                collision = true;
            }
            collided = collision;
            return collision;
        }
        void ZMove(ref double zsp)
        {
            if ((zPos + zsp < 0) || (zPos + zsp > 64))
            {
                if (zPos + zsp < 0) zPos = 0;
                if (zPos + zsp > 64) zPos = 64;
                zsp *= -0.9;
                if (Math.Abs(zsp) < 2) zsp = 0;
                else compression = MathUtilities.Clamp(Math.Abs(zsp - 2) / 2, 0, 0.5);
                hsp *= grip;
                vsp *= grip;
            }
        }
        public void Explode(List<Entity> entitiesList)
        {
            Parallel.For(0, entitiesList.Count, n => // Loop through all active entities (again again)
            {
                Entity subTargetEntity = entitiesList[n]; // Sets a reference to the entity for easy calling
                double distance = Math.Sqrt(Math.Pow(xPos - subTargetEntity.xPos, 2) + Math.Pow(yPos - subTargetEntity.yPos, 2));
                if (distance <= 3)
                {
                    double subAngle = (Math.PI * 1.5) - Math.Atan2(xPos - subTargetEntity.xPos, yPos - subTargetEntity.yPos); // Find direction towards the target (in radians)
                    subAngle = subAngle % (Math.PI * 2); // Shorten number if it is unnecessarily large (e.g 720 degrees is the same as 360 degrees)
                    if (subAngle < 0) subAngle += (Math.PI * 2); // Ensure the angle is positive
                    subTargetEntity.hsp += (((3 - distance) / 3) * (Math.Cos(subAngle))) / subTargetEntity.weight; // Push the targeted entity away from the current entity
                    subTargetEntity.vsp += (((3 - distance) / 3) * (Math.Sin(subAngle))) / subTargetEntity.weight; // The weight attribute is used here to push lighter objects further and vice versa
                    Random rnd = new Random(); // Set up random number generator
                    int sign = Math.Sign(rnd.Next(0, 2) - 0.5); // Randomly select a direction for squash and stretch
                    subTargetEntity.compression += 0.2 * sign; // Change immediate scale of the enities sprite
                    subTargetEntity.cDifference += 0.2 * sign; // Change the direction of scale of the entities sprite
                    subTargetEntity.white = 1; // Sprite will flash white briefly to indicate a hit
                    subTargetEntity.HP -= Convert.ToInt32(damage * ((3 - distance) / 3));
                }
            });
            garbage = true;
        }
    }
}
