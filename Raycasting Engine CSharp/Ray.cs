using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycasting_Engine_CSharp
{
    public class Ray
    {
        public int position;
        public Tile tile;
        public int wallHeight;
        public double darkness;
        public double perpWallDist;
        public int mapX;
        public int mapY;
        public double rayDirX;
        public double rayDirY;
        public int texX;
        public int side;
        public double wallX;
        public bool thin;
        public Ray subRay = null;
        public Ray(int position, Tile tile, int wallHeight, double darkness, double perpWallDist, int mapX, int mapY, double rayDirX, double rayDirY, int texX, int side, double wallX, bool thin)
        {
            this.position = position;
            this.tile = tile;
            this.wallHeight = wallHeight;
            this.darkness = darkness;
            this.perpWallDist = perpWallDist;
            this.mapX = mapX;
            this.mapY = mapY;
            this.rayDirX = rayDirX;
            this.rayDirY = rayDirY;
            this.texX = texX;
            this.side = side;
            this.wallX = wallX;
            this.thin = thin;
        }
    }
}
