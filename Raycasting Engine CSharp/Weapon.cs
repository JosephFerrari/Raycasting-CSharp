using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;

namespace Raycasting_Engine_CSharp
{
    public class Weapon
    {
        public string name;
        public Player player;
        public int ammoCost;
        public int ammoMax;
        public int ammoValue;
        public int ammoSegments;
        public bool automatic;
        public int reloadTime;
        public int reloadCounter = 0;
        public bool reloading = false;
        public int damage;
        public double knockback;
        public int range;
        public EntityType projectile;
        public int hitboxWidth; // Crosshair hitbox width for reference during hitscan
        public int hitboxHeight; // Crosshair hitbox height for reference during hitscan
        public int cooldown;
        public SoundPlayer attackSound;
        public SoundPlayer reloadSound;
        public bool hitToPlay;
        public Weapon(string name, Player player, int ammoCost, int ammoMax, int ammoSegments, bool automatic, int reloadTime, int damage, double knockback, int range, EntityType projectile, int hitboxWidth, int hitboxHeight, int cooldown, SoundPlayer attackSound, SoundPlayer reloadSound, bool hitToPlay)
        {
            this.name = name;
            this.player = player;
            this.ammoCost = ammoCost;
            this.ammoMax = ammoMax;
            ammoValue = ammoMax;
            this.ammoSegments = ammoSegments;
            this.automatic = automatic;
            this.reloadTime = reloadTime;
            this.damage = damage;
            this.knockback = knockback;
            this.range = range;
            this.projectile = projectile;
            this.hitboxWidth = hitboxWidth;
            this.hitboxHeight = hitboxHeight;
            this.cooldown = cooldown;
            this.attackSound = attackSound;
            this.reloadSound = reloadSound;
            this.hitToPlay = hitToPlay;
        }
        public void Reload()
        {
            reloadCounter++;
            if (reloadCounter >= reloadTime)
            {
                reloadCounter = 0;
                if (player.ammoValue >= 1)
                {
                    ammoValue = Math.Min(ammoValue + 1, ammoMax);
                    player.ammoValue = Math.Max(player.ammoValue - 1, 0);
                }
                else reloading = false;
                if (ammoValue >= ammoMax)
                {
                    ammoValue = ammoMax;
                    reloading = false;
                }
            }
        }
        public void BeginReload()
        {
            if ((!reloading) && (player.ammoValue >= 1))
            {
                if (reloadSound != null) reloadSound.Play();
                reloading = true;
            }
        }
    }
}
