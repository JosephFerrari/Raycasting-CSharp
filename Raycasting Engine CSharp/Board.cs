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

namespace Raycasting_Engine_CSharp
{
    public class Board
    {
        public int boardWidth;
        public int boardHeight;
        public int layoutWidth;
        public int layoutHeight;
        public int roomWidth;
        public int roomHeight;
        public Environment environment;
        public Byte[] floorTexture;
        public Byte[] ceilingTexture;
        public int[,] boardConcept = new int[12, 16] { { 05, 05, 05, 05, 01, 01, 02, 01, 01, 01, 01, 02, 01, 01, 01, 01 },
                                                       { 05, 00, 00, 00, 01, 00, 00, 00, 06, 00, 00, 00, 00, 00, 01, 01 },
                                                       { 05, 00, 00, 00, 00, 00, 00, 00, 06, 00, 00, 00, 00, 00, 00, 01 },
                                                       { 05, 00, 00, 00, 00, 00, 00, 00, 01, 00, 00, 00, 00, 00, 00, 02 },
                                                       { 01, 01, 00, 00, 00, 00, 00, 00, 07, 00, 00, 00, 00, 00, 00, 01 },
                                                       { 01, 00, 00, 00, 00, 00, 00, 00, 01, 00, 00, 01, 01, 07, 01, 01 },
                                                       { 01, 00, 00, 00, 01, 04, 01, 01, 01, 00, 00, 01, 00, 00, 00, 01 },
                                                       { 02, 00, 00, 00, 01, 00, 00, 00, 06, 00, 00, 02, 00, 00, 00, 03 },
                                                       { 01, 00, 00, 00, 01, 01, 00, 00, 06, 00, 00, 01, 00, 00, 00, 01 },
                                                       { 01, 04, 01, 08, 01, 04, 00, 00, 06, 00, 00, 04, 00, 01, 04, 01 },
                                                       { 01, 04, 04, 00, 00, 04, 01, 02, 01, 00, 00, 01, 01, 00, 00, 01 },
                                                       { 01, 01, 01, 01, 01, 01, 01, 01, 01, 01, 01, 02, 01, 01, 01, 01 } };
        public Tile[,] boardLayout;
        public int[,] roomLayout;
        public int roomsHorizontally = 3;
        public int roomsVertically = 3;
        public string seed = "";
        public int floor = 0;
        public int locks = 0;
        public List<Entity> entitiesList = new List<Entity>();
        public Board(int boardWidth, int boardHeight, Environment environment)
        {
            this.boardWidth = boardWidth;
            this.boardHeight = boardHeight;
            Random rnd = new Random(); // "16461"
            seed = Convert.ToString(rnd.Next(0, 10)) + Convert.ToString(rnd.Next(0, 10)) + Convert.ToString(rnd.Next(0, 10)) + Convert.ToString(rnd.Next(0, 10)) + Convert.ToString(rnd.Next(0, 10));
            GenerateLayout();
            boardLayout = new Tile[this.boardHeight, this.boardWidth];
            List<double[]> lockedDoors = new List<double[]>();
            List<double[]> enemySpawns = new List<double[]>();
            List<double[]> baseLoot = new List<double[]>();
            List<double[]> lockedLoot = new List<double[]>();
            List<double[]> hiddenLoot = new List<double[]>();
            for (int x = 0; x < this.boardWidth; x++)
            {
                for (int y = 0; y < this.boardHeight; y++)
                {
                    if (boardConcept[y, x] == 7) lockedDoors.Add(new double[] { y + 0.5, x + 0.5 });
                    if (boardConcept[y, x] == -1) enemySpawns.Add(new double[] { y + 0.5, x + 0.5 });
                    if (boardConcept[y, x] == -4) baseLoot.Add(new double[] { y + 0.5, x + 0.5 });
                    if (boardConcept[y, x] == -5) lockedLoot.Add(new double[] { y + 0.5, x + 0.5 });
                    if (boardConcept[y, x] == -6) hiddenLoot.Add(new double[] { y + 0.5, x + 0.5 });
                }
            }
            List<double[]> keySpawns = new List<double[]>();
            List<double[]> barrelSpawns = new List<double[]>();
            List<double[]> ammoSpawns = new List<double[]>();
            List<double[]> healthSpawns = new List<double[]>();
            if ((locks > baseLoot.Count) || (locks + lockedDoors.Count > baseLoot.Count + hiddenLoot.Count)) GameForm.ActiveForm.Close(); // panic
            rnd = new Random(Convert.ToInt32(seed));
            for (int i = 0; i < locks; i++) // Place the necessary exit keys in base loot
            {
                int item = rnd.Next(0, baseLoot.Count);
                keySpawns.Add(baseLoot[item]);
                baseLoot.RemoveAt(item);
            }
            int lengthCache = Math.Min(Convert.ToInt32(Math.Ceiling(lockedDoors.Count * 0.75)), baseLoot.Count);
            for (int i = 0; i < lengthCache; i++) // Place 75% of keys in base loot
            {
                int item = rnd.Next(0, baseLoot.Count);
                keySpawns.Add(baseLoot[item]);
                baseLoot.RemoveAt(item);
                lockedDoors.RemoveAt(0);
            }
            lengthCache = lockedDoors.Count;
            for (int i = 0; i < lengthCache; i++) // Place 25% of keys in hidden loot
            {
                int item = rnd.Next(0, hiddenLoot.Count);
                keySpawns.Add(hiddenLoot[item]);
                hiddenLoot.RemoveAt(item);
                lockedDoors.RemoveAt(0);
            }

            // NOTE : SOMETHING IS CAUSING RANDOM ERRORS ON STARTUP, MUST BE DOWN TO THESE LOOPS, LOCKED DOORS ARE BEING UNACCOUNTED FOR
            //locks = lockedDoors.Count;

            // Base Loot - Out in the open
            FillSpawns(ref baseLoot, 0.60, ref barrelSpawns, ref rnd); // Fill 60% of base loot with barrels
            FillSpawns(ref baseLoot, 0.75, ref ammoSpawns, ref rnd); // Fill 30% of base loot with ammunition
            FillSpawns(ref baseLoot, 1.00, ref healthSpawns, ref rnd); // Fill 10% of base loot with health
            // Locked Loot - Locked behind a gate requiring a key
            FillSpawns(ref lockedLoot, 0.50, ref healthSpawns, ref rnd); // Fill 50% of base loot with health
            FillSpawns(ref lockedLoot, 0.75, ref ammoSpawns, ref rnd); // Fill 30% of base loot with ammunition
            FillSpawns(ref lockedLoot, 1.00, ref barrelSpawns, ref rnd); // Fill 20% of base loot with barrels
            // Hidden Loot - Hidden behind a pushwall
            FillSpawns(ref hiddenLoot, 0.60, ref ammoSpawns, ref rnd); // Fill 60% of base loot with ammunition
            FillSpawns(ref hiddenLoot, 0.50, ref healthSpawns, ref rnd); // Fill 20% of base loot with health
            FillSpawns(ref hiddenLoot, 1.00, ref barrelSpawns, ref rnd); // Fill 20% of base loot with barrels

            for (int i = 0; i < keySpawns.Count; i++) { entitiesList.Add(new Entity(EntityType.Key, this, keySpawns[i][1], keySpawns[i][0])); }
            for (int i = 0; i < barrelSpawns.Count; i++) { entitiesList.Add(new Entity(EntityType.Barrel, this, barrelSpawns[i][1], barrelSpawns[i][0])); }
            for (int i = 0; i < ammoSpawns.Count; i++) { entitiesList.Add(new Entity(EntityType.Ammo, this, ammoSpawns[i][1], ammoSpawns[i][0])); }
            for (int i = 0; i < healthSpawns.Count; i++) { entitiesList.Add(new Entity(EntityType.Health, this, healthSpawns[i][1], healthSpawns[i][0])); }
            for (int i = 0; i < enemySpawns.Count; i++)
            {
                if (rnd.Next(0, 3) == 1) entitiesList.Add(new Entity(EntityType.FlyingEnemy, this, enemySpawns[i][1], enemySpawns[i][0]));
                else entitiesList.Add(new Entity(EntityType.Freaker, this, enemySpawns[i][1], enemySpawns[i][0]));
            }
            for (int x = 0; x < this.boardWidth; x++)
            {
                for (int y = 0; y < this.boardHeight; y++)
                {
                    boardLayout[y, x] = TilePreset(Math.Max(boardConcept[y, x], 0), environment);
                }
            }
            this.environment = environment;
            if (environment == Environment.Sewer) { floorTexture = DrawingUtilities.LockBitmap(Properties.Resources.WoodenPlanks); ceilingTexture = DrawingUtilities.LockBitmap(Properties.Resources.GreyBrickWall); }
            else if (environment == Environment.Ice) { floorTexture = DrawingUtilities.LockBitmap(Properties.Resources.Ice); ceilingTexture = DrawingUtilities.LockBitmap(Properties.Resources.GreyBrickWall); }
            else if (environment == Environment.Haunted) { floorTexture = DrawingUtilities.LockBitmap(Properties.Resources.GreyBrickWall); ceilingTexture = DrawingUtilities.LockBitmap(Properties.Resources.StoneWall); }
            else { floorTexture = DrawingUtilities.LockBitmap(Properties.Resources.WoodenPlanks); ceilingTexture = DrawingUtilities.LockBitmap(Properties.Resources.GreyBrickWall); }
        }
        public void FillSpawns(ref List<double[]> spawnLocation, double percentage, ref List<double[]> itemLocation, ref Random rnd)
        {
            int lengthCache = spawnLocation.Count;
            for (int i = 0; i < Math.Floor(lengthCache * percentage); i++)
            {
                int item = rnd.Next(0, spawnLocation.Count);
                itemLocation.Add(spawnLocation[item]);
                spawnLocation.RemoveAt(item);
            }
        }
        public Tile BoardPoint(double x, double y)
        {
            x = Math.Floor(x);
            y = Math.Floor(y - 0);
            if ((x < 0) || (x >= boardWidth) || (y < 0) || (y >= boardHeight)) return TilePreset(1, environment);
            return boardLayout[Convert.ToInt32(y), Convert.ToInt32(x)];
        }
        public void GenerateLayout()
        {
            layoutWidth = (roomsHorizontally * 2) + 1;
            layoutHeight = (roomsVertically * 2) + 1;
            roomWidth = 11; roomHeight = 11;
            boardWidth = (roomsHorizontally * roomWidth) + roomsHorizontally + 1;
            boardHeight = (roomsVertically * roomHeight) + roomsVertically + 1;
            roomLayout = new int[layoutHeight, layoutWidth];
            for (int x = 0; x < layoutWidth; x++)
            {
                for (int y = 0; y < layoutHeight; y++)
                {
                    if ((x % 2 == 0) || (y % 2 == 0)) roomLayout[y, x] = 1;
                    else roomLayout[y, x] = 0;
                    roomLayout[y, x] = 1;
                }
            }
            RandomiseBoard();
            boardConcept = roomLayout;
            GenerateRooms();
            FillRooms();
        }
        public void GenerateRooms()
        {
            boardConcept = new int[boardHeight, boardWidth];
            for (int x = 0; x <= roomsHorizontally; x++)
            {
                for (int y = 0; y < boardHeight; y++)
                {
                    boardConcept[y, x * (roomWidth + 1)] = 1;
                }
            }
            for (int y = 0; y <= roomsVertically; y++)
            {
                for (int x = 0; x < boardWidth; x++)
                {
                    boardConcept[y * (roomHeight + 1), x] = 1;
                }
            }
            for (int x = 1; x < roomsHorizontally; x++)
            {
                for (int y = 0; y < roomsVertically; y++)
                {
                    if (roomLayout[((y + 1) * 2) - 1, (x * 2)] == 0) boardConcept[(y * roomHeight) + y + (roomHeight / 2) + 1, (x * roomWidth) + x] = 0;
                }
            }
            for (int x = 0; x < roomsHorizontally; x++)
            {
                for (int y = 1; y < roomsVertically; y++)
                {
                    if (roomLayout[(y * 2), ((x + 1) * 2) - 1] == 0) boardConcept[(y * roomHeight) + y, (x * roomWidth) + x + (roomWidth / 2) + 1] = 0;
                }
            }
        }
        public void FillRooms()
        {
            Random rnd = new Random(Convert.ToInt32(seed));
            for (int x = 1; x < layoutWidth; x += 2)
            {
                for (int y = 1; y < layoutHeight; y += 2)
                {
                    if ((x == 1) && (y == 1)) continue;
                    bool left = roomLayout[y, x - 1] == 0;
                    bool right = roomLayout[y, x + 1] == 0;
                    bool up = roomLayout[y - 1, x] == 0;
                    bool down = roomLayout[y + 1, x] == 0;
                    int[,] room = GetRoom(left, right, up, down, 0, rnd);
                    for (int subX = 0; subX < roomWidth; subX++)
                    {
                        for (int subY = 0; subY < roomHeight; subY++)
                        {
                            boardConcept[1 + (((y - 1) / 2) * (roomHeight + 1)) + subY, 1 + (((x - 1) / 2) * (roomWidth + 1)) + subX] = room[subY, subX];
                            //if (room[subY, subX] == -1) entitiesList.Add(new Entity(EntityType.Freaker, this, 1.5 + (((x - 1) / 2) * (roomWidth + 1)) + subX, 1.5 + (((y - 1) / 2) * (roomHeight + 1)) + subY));
                            //if (room[subY, subX] == -2) entitiesList.Add(new Entity(EntityType.Barrel, this, 1.5 + (((x - 1) / 2) * (roomWidth + 1)) + subX, 1.5 + (((y - 1) / 2) * (roomHeight + 1)) + subY));
                        }
                    }
                }
            }
        }
        public int[,] GetRoom(bool left, bool right, bool up, bool down, int special, Random rnd)
        {
            int connections = Convert.ToInt32(left) + Convert.ToInt32(right) + Convert.ToInt32(up) + Convert.ToInt32(down);
            int type = 0;
            if (connections == 2)
            {
                if (((left) && (right)) || ((up) && (down))) type = 1;
                else type = 2;
            }
            else if (connections == 3) type = 3;
            else if (connections == 4) type = 4;
            int variant = rnd.Next(0, 3);
            byte[] roomRaw = DrawingUtilities.CopySection(DrawingUtilities.LockBitmap(Properties.Resources.RoomPresets), 117, 78, 1 + (variant * (roomWidth + 2)), 1 + (type * (roomHeight + 2)), roomWidth, roomHeight);
            int[,] room = new int[roomHeight, roomWidth];
            for (int x = 0; x < roomWidth; x++)
            {
                for (int y = 0; y < roomHeight; y++)
                {
                    Color tile = DrawingUtilities.TexturePixel(roomRaw, x, y, roomWidth, roomHeight);
                    room[y, x] = 0; // Would have used switch here but it cant be used for colour and its a one time process so it doesn't matter that its slow
                    if (tile == Color.FromArgb(255, 255, 255, 255)) room[y, x] = 0; // Blank Space
                    else if (tile == Color.FromArgb(255, 27, 38, 50)) room[y, x] = 1; // Base Wall
                    else if (tile == Color.FromArgb(255, 47, 72, 78)) room[y, x] = 2; // Decorative Wall
                    else if (tile == Color.FromArgb(255, 235, 137, 49)) room[y, x] = 4; // Push Wall
                    else if (tile == Color.FromArgb(255, 0, 87, 132)) room[y, x] = 6; // Gate
                    else if (tile == Color.FromArgb(255, 49, 162, 242)) room[y, x] = 7; // Locked Gate
                    else if (tile == Color.FromArgb(255, 178, 220, 239)) room[y, x] = 8; // Unlocked Gate
                    else if (tile == Color.FromArgb(255, 190, 38, 51)) room[y, x] = -1; // Enemy Spawn
                    else if (tile == Color.FromArgb(255, 163, 206, 39)) room[y, x] = -4; // Base Loot
                    else if (tile == Color.FromArgb(255, 68, 137, 26)) room[y, x] = -5; // Locked Loot
                    else if (tile == Color.FromArgb(255, 7, 203, 111)) room[y, x] = -6; // Hidden Loot
                    else room[y, x] = 0;
                }
            }
            bool currentLeft = false;
            bool currentRight = false;
            bool currentUp = false;
            bool currentDown = false;
            switch (type)
            {
                case 0:
                    currentDown = true;
                    break;
                case 1:
                    currentLeft = true;
                    currentRight = true;
                    break;
                case 2:
                    currentRight = true;
                    currentDown = true;
                    break;
                case 3:
                    currentLeft = true;
                    currentRight = true;
                    currentUp = true;
                    break;
                case 4:
                    currentLeft = true;
                    currentRight = true;
                    currentUp = true;
                    currentDown = true;
                    break;
                case 5:
                    currentLeft = true;
                    currentRight = true;
                    currentUp = true;
                    currentDown = true;
                    break;
            }
            while ((currentLeft != left) || (currentRight != right) || (currentUp != up) || (currentDown != down))
            {
                int[,] roomCopy = room.Clone() as int[,];
                for (int x = 0; x < roomWidth; x++)
                {
                    for (int y = 0; y < roomHeight; y++)
                    {
                        double centre = (roomWidth - 1) / 2.0;
                        double relX = x - centre;
                        double relY = y - centre;
                        double rotX = relY;
                        double rotY = -relX;
                        int newX = Convert.ToInt32(rotX + centre);
                        int newY = Convert.ToInt32(rotY + centre);
                        room[y, x] = roomCopy[MathUtilities.Clamp(newY, 0, 10), MathUtilities.Clamp(newX, 0, 10)];
                    }
                }
                bool cache = currentLeft;
                currentLeft = currentDown;
                currentDown = currentRight;
                currentRight = currentUp;
                currentUp = cache;
            }
            return room;
        }
        public void RandomiseBoard()
        {
            int x = 1; int y = 1;
            int xSub = 1; int ySub = 1;
            List<int> remainingWallX = new List<int>();
            List<int> remainingWallY = new List<int>();
            List<int> visitedWallX = new List<int>();
            List<int> visitedWallY = new List<int>();
            Random rnd = new Random(Convert.ToInt32(seed));
            roomLayout[y, x] = 0;
            if ((x > 1) && (roomLayout[y, x - 1] == 1)) { remainingWallX.Add(x - 1); remainingWallY.Add(y); visitedWallX.Add(x - 1); visitedWallY.Add(y); }
            if ((x < (roomsHorizontally * 2) - 1) && (roomLayout[y, x + 1] == 1)) { remainingWallX.Add(x + 1); remainingWallY.Add(y); visitedWallX.Add(x + 1); visitedWallY.Add(y); }
            if ((y > 1) && (roomLayout[y - 1, x] == 1)) { remainingWallX.Add(x); remainingWallY.Add(y - 1); visitedWallX.Add(x); visitedWallY.Add(y - 1); }
            if ((y < (roomsVertically * 2) - 1) && (roomLayout[y + 1, x] == 1)) { remainingWallX.Add(x); remainingWallY.Add(y + 1); visitedWallX.Add(x); visitedWallY.Add(y + 1); }
            while (remainingWallX.Count > 0)
            {
                int wall = rnd.Next(0, remainingWallX.Count());
                xSub = remainingWallX[wall];
                ySub = remainingWallY[wall];
                int orientation = 0;
                if (ySub % 2 == 1) orientation = 1;
                int side1; int side2;
                if (orientation == 0)
                {
                    x = xSub;
                    side1 = roomLayout[ySub - 1, xSub];
                    side2 = roomLayout[ySub + 1, xSub];
                    if ((side1 == 1) && (side2 == 0)) y = ySub - 1;
                    if ((side1 == 0) && (side2 == 1)) y = ySub + 1;
                }
                else
                {
                    y = ySub;
                    side1 = roomLayout[ySub, xSub - 1];
                    side2 = roomLayout[ySub, xSub + 1];
                    if ((side1 == 1) && (side2 == 0)) x = xSub - 1;
                    if ((side1 == 0) && (side2 == 1)) x = xSub + 1;
                }
                if (((side1 == 1) && (side2 == 0)) || ((side1 == 0) && (side2 == 1)))
                {
                    roomLayout[ySub, xSub] = 0;
                    roomLayout[y, x] = 0;
                    if ((x > 1) && (roomLayout[y, x - 1] == 1)) { remainingWallX.Add(x - 1); remainingWallY.Add(y); visitedWallX.Add(x - 1); visitedWallY.Add(y); }
                    if ((x < (roomsHorizontally * 2) - 1) && (roomLayout[y, x + 1] == 1)) { remainingWallX.Add(x + 1); remainingWallY.Add(y); visitedWallX.Add(x + 1); visitedWallY.Add(y); }
                    if ((y > 1) && (roomLayout[y - 1, x] == 1)) { remainingWallX.Add(x); remainingWallY.Add(y - 1); visitedWallX.Add(x); visitedWallY.Add(y - 1); }
                    if ((y < (roomsVertically * 2) - 1) && (roomLayout[y + 1, x] == 1)) { remainingWallX.Add(x); remainingWallY.Add(y + 1); visitedWallX.Add(x); visitedWallY.Add(y + 1); }
                }
                remainingWallX.RemoveAt(wall);
                remainingWallY.RemoveAt(wall);
            }
            /*int centreX = Convert.ToInt32(Math.Floor(((width * 2) + 1) / 2.0)); int centreY = Convert.ToInt32(Math.Floor(((height * 2) + 1) / 2.0));
            roomLayout[centreY, centreX] = 0; roomLayout[centreY + 1, centreX] = 0; roomLayout[centreY - 1, centreX] = 0; roomLayout[centreY, centreX + 1] = 0; roomLayout[centreY, centreX - 1] = 0;
            for (int xPos = 1; xPos < (width * 2) - 0; xPos++)
            {
                for (int yPos = 1; yPos < (height * 2) - 0; yPos++)
                {
                    if (((xPos % 2 == 0) ^ (yPos % 2 == 0)) && (rnd.Next(0, 50) == 0)) roomLayout[yPos, xPos] = 0;
                }
            }
            for (int xPos = 1; xPos < (width * 2) - 0; xPos++)
            {
                for (int yPos = 1; yPos < (height * 2) - 0; yPos++)
                {
                    if ((xPos % 2 == 1) && (yPos % 2 == 1))
                    {
                        int exits = 0;
                        if (roomLayout[yPos, xPos - 1] == 0) exits++;
                        if (roomLayout[yPos, xPos + 1] == 0) exits++;
                        if (roomLayout[yPos - 1, xPos] == 0) exits++;
                        if (roomLayout[yPos + 1, xPos] == 0) exits++;
                        if (exits == 1)
                        {
                            if ((rnd.Next(0, 3) == 0))
                            {
                                if ((roomLayout[yPos, xPos - 1] == 0) && (xPos < (width * 2) - 1)) roomLayout[yPos, xPos + 1] = 0;
                                else if ((roomLayout[yPos, xPos + 1] == 0) && (xPos > 1)) roomLayout[yPos, xPos - 1] = 0;
                                else if ((roomLayout[yPos - 1, xPos] == 0) && (yPos < (height * 2) - 1)) roomLayout[yPos + 1, xPos] = 0;
                                else if ((roomLayout[yPos + 1, xPos] == 0) && (yPos > 1)) roomLayout[yPos - 1, xPos] = 0;
                                else roomLayout[yPos, xPos] = 0; // 2
                            }
                            else roomLayout[yPos, xPos] = 0; // 2
                        }
                    }
                }
            }*/
        }
        public Tile TilePreset(int ID, Environment environment)
        {
            switch (ID)
            {
                case 00:
                    return new Tile(ID, null, 0, DoorType.Static, false); // Blank space
                case 01:
                    switch (environment) { // Base wall
                        case Environment.Sewer: return new Tile(ID, Properties.Resources.RedBrickWall, 0, DoorType.Static, false); // Red brick wall
                        case Environment.Ice: return new Tile(ID, Properties.Resources.BlueBrickWall, 0, DoorType.Static, false); // Blue brick wall
                        case Environment.Haunted: return new Tile(ID, Properties.Resources.WoodenPlanks, 0, DoorType.Static, false); // Wooden wall
                    } break;
                case 02:
                    switch (environment) { // Base wall decorative variant 1
                        case Environment.Sewer: return new Tile(ID, Properties.Resources.RedBrickWallVariant1, 0, DoorType.Static, false); // Red brick wall with banner
                        case Environment.Ice: return new Tile(ID, Properties.Resources.BlueBrickWall, 0, DoorType.Static, false); // Blue brick wall
                        case Environment.Haunted: return new Tile(ID, Properties.Resources.WoodenPlanks, 0, DoorType.Static, false); // Wooden wall
                    } break;
                case 03:
                    switch (environment) { // Base wall decorative variant 2
                        case Environment.Sewer: return new Tile(ID, Properties.Resources.RedBrickWallVariant2, 0, DoorType.Static, false); // Red brick wall with photo
                        case Environment.Ice: return new Tile(ID, Properties.Resources.BlueBrickWall, 0, DoorType.Static, false); // Blue brick wall
                        case Environment.Haunted: return new Tile(ID, Properties.Resources.WoodenPlanks, 0, DoorType.Static, false); // Wooden wall
                    } break;
                case 04:
                    switch (environment) { // Base push wall
                        case Environment.Sewer: return new Tile(ID, Properties.Resources.RedBrickWall, 0, DoorType.PushLong, false); // Red push wall
                        case Environment.Ice: return new Tile(ID, Properties.Resources.BlueBrickWall, 0, DoorType.PushLong, false); // Blue push wall
                        case Environment.Haunted: return new Tile(ID, Properties.Resources.WoodenPlanks, 0, DoorType.PushLong, false); // Wooden push wall
                    } break;
                case 05:
                    switch (environment) { // Alternate wall
                        case Environment.Sewer: return new Tile(ID, Properties.Resources.StoneWall, 0, DoorType.Static, false); // Stone wall
                        case Environment.Ice: return new Tile(ID, Properties.Resources.WoodenPlanks, 0, DoorType.Static, false); // Wooden planks
                        case Environment.Haunted: return new Tile(ID, Properties.Resources.StoneWall, 0, DoorType.Static, false); // Stone wall
                    } break;
                case 06:
                    switch (environment) { // Base fence
                        case Environment.Sewer: return new Tile(ID, Properties.Resources.Bars, 0.3, DoorType.Static, false); // Thin steel bars
                        case Environment.Ice: return new Tile(ID, Properties.Resources.Bars, 0.5, DoorType.Static, false); // Thin steel bars
                        case Environment.Haunted: return new Tile(ID, Properties.Resources.Bars, 0.5, DoorType.Static, false); // Thin steel bars
                    } break;
                case 07:
                    switch (environment) { // Locked horizontal door
                        case Environment.Sewer: return new Tile(ID, Properties.Resources.GateLocked, 0.5, DoorType.HorizontalRight, true); // Thin steel bars gate
                        case Environment.Ice: return new Tile(ID, Properties.Resources.GateLocked, 0.5, DoorType.HorizontalRight, true); // Thin steel bars gate
                        case Environment.Haunted: return new Tile(ID, Properties.Resources.GateLocked, 0.5, DoorType.HorizontalRight, true); // Thin steel bars gate
                    } break;
                case 08:
                    switch (environment) { // Unlocked vertical gate
                        case Environment.Sewer: return new Tile(ID, Properties.Resources.Gate, 0.5, DoorType.VerticalUp, false); // Thin steel bars gate
                        case Environment.Ice: return new Tile(ID, Properties.Resources.Gate, 0.5, DoorType.VerticalUp, false); // Thin steel bars gate
                        case Environment.Haunted: return new Tile(ID, Properties.Resources.Gate, 0.5, DoorType.VerticalUp, false); // Thin steel bars gate
                    } break;
            }
            return new Tile(0, Properties.Resources.StoneWall, 0, DoorType.Static, false); // Default to blank space
        }
    }
}
