using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3DTest
{
    public partial class Form1 : Form
    {
        int recording = -1;
        bool screenshot = false;
        int shotCount = 0;

        double blockSize = 64;
        double maxDistance = 80;
        double playerSpeed = 4;
        Bitmap display;

        Vector2 playerPos = new Vector2(32, 32);
        private Angle playerAngle = 0;

        Angle FOV = 60;
        Vector2 resolution = new Vector2(320, 200);
        Dictionary<Vector2, Tile> tileMap = new Dictionary<Vector2, Tile>();
        Dictionary<Vector2, Entity> doors = new Dictionary<Vector2, Entity>();
        Dictionary<Vector2, Entity> blocks = new Dictionary<Vector2, Entity>();
        List<Entity> gameentites = new List<Entity>();

        public void AddTile(string id, double X, double Y)
        {
            tileMap.Add(new Vector2(X * blockSize, Y * blockSize), new Tile(id, new Vector2(X * blockSize, Y * blockSize)));
        }

        public void AddDoor(string id, double X, double Y)
        {
            Entity door = new Entity(id, new Vector2(X * blockSize, Y * blockSize));
            door.data.Add("Closing", true);
            door.data.Add("Played", false);
            door.data.Add("OffSetX", 0.0);
            door.data.Add("OffSetY", 0.0);
            door.data.Add("Timer", 0);
            doors.Add(new Vector2(X * blockSize, Y * blockSize), door);
        }

        public void AddBlock(string id, double X, double Y)
        {
            Entity block = new Entity(id, new Vector2(X * blockSize, Y * blockSize));
            block.data.Add("Direction", 1);
            blocks.Add(new Vector2(X * blockSize, Y * blockSize), block);
        }

        public Form1()
        {
            InitializeComponent();

            pictureBox1.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

            Textures.LoadTexture("Blue", "Textures/WallBlue.png");
            Textures.LoadTexture("Dirt", "Textures/WallDirt.png");
            Textures.LoadTexture("Stone", "Textures/WallStone.png");
            Textures.LoadTexture("Blue90", "Textures/WallBlue90.png");
            Textures.LoadTexture("Dirt90", "Textures/WallDirt90.png");
            Textures.LoadTexture("Stone90", "Textures/WallStone90.png");
            Textures.LoadTexture("DoorBlue", "Textures/DoorBlue.png");
            Textures.LoadTexture("DoorBlue2", "Textures/DoorBlue2.png");
            Textures.LoadTexture("Statue", "Textures/Statue.png");
            Textures.LoadTexture("Lamp", "Textures/Lamp.png");
            Textures.LoadTexture("Table", "Textures/Table.png");
            Tiles.AddTile("Blue", new TileType("Blue", "Blue90"));
            Tiles.AddTile("Dirt", new TileType("Dirt", "Dirt90"));
            Tiles.AddTile("Stone", new TileType("Stone", "Stone90"));
            Entities.AddEntity("Door", new EntityType(EntityType.EntityClass.DOOR, false, new List<string>() { "DoorBlue", "DoorBlue2" }));
            Entities.AddEntity("Door90", new EntityType(EntityType.EntityClass.DOOR90, false, new List<string>() { "DoorBlue", "DoorBlue2" }));
            Entities.AddEntity("Block", new EntityType(EntityType.EntityClass.BLOCK, false, new List<string>() { "Stone", "Stone90" }));
            Entities.AddEntity("Statue", new EntityType(EntityType.EntityClass.SPRITE, true, new List<string>() { "Statue"}));
            Entities.AddEntity("Lamp", new EntityType(EntityType.EntityClass.SPRITE, true, new List<string>() { "Lamp" }));
            Entities.AddEntity("Table", new EntityType(EntityType.EntityClass.SPRITE, true, new List<string>() { "Table" }));


            Bitmap level = (Bitmap)Image.FromFile("levelmap.bmp");
            for (int x = 0; x < level.Width; x++)
            {
                for (int y = 0; y < level.Height; y++)
                {
                    Color c = level.GetPixel(x, y);
                    if (c.G == 255)
                    {
                        if (c.B == 0) AddTile("Blue", x, y);
                        if (c.B == 1) AddTile("Stone", x, y);
                        if (c.B == 2) AddTile("Dirt", x, y);
                    } 
                    else if (c.B == 255)
                    {
                        if (c.R == 0) AddDoor("Door", x, y);
                        if (c.R == 1) AddDoor("Door90", x, y);
                        if (c.R == 2)
                        {
                            AddBlock("Block", x, y);
                            AddTile("Stone", x, y);
                        }
                        if (c.R == 3)
                        {
                            AddBlock("Block", x, y);
                            AddTile("Dirt", x, y);
                        }
                        if (c.R == 4)
                        {
                            AddBlock("Block", x, y);
                            AddTile("Blue", x, y);
                        }
                        if (c.R == 5)
                        {
                            gameentites.Add(new Entity("Statue", new Vector2(x * 64, y * 64)));
                        }
                        if (c.R == 6)
                        {
                            gameentites.Add(new Entity("Lamp", new Vector2(x * 64, y * 64)));
                        }
                        if (c.R == 7)
                        {
                            gameentites.Add(new Entity("Table", new Vector2(x * 64, y * 64)));
                        }
                    }
                    else if (c.R == 255)
                    {
                        playerPos = new Vector2((x+0.5) * 64, (y+0.5) * 64);
                    }
                }
            }

            playerAngle = 270;
            Thread thr = new Thread(Draw);
            thr.Start();

        }

        bool released = false;

        public void Draw()
        {


            while (!this.Visible) ;
            while (this.Visible)
            {

                Vector2 newPos = new Vector2(playerPos.X, playerPos.Y);

                if (keysPressed.Contains(Keys.A))
                {
                    playerAngle -= playerSpeed / 3;
                }
                if (keysPressed.Contains(Keys.D))
                {
                    playerAngle += playerSpeed / 3;
                }
                if (keysPressed.Contains(Keys.W))
                {
                    newPos.Y += Math.Cos(playerAngle.GetRad()) * playerSpeed;
                    newPos.X += Math.Sin(playerAngle.GetRad()) * playerSpeed;
                }
                if (keysPressed.Contains(Keys.S))
                {
                    newPos.Y -= Math.Cos(playerAngle.GetRad()) * playerSpeed;
                    newPos.X -= Math.Sin(playerAngle.GetRad()) * playerSpeed;
                }
                if (keysPressed.Contains(Keys.E))
                {
                    newPos.Y -= Math.Sin(playerAngle.GetRad()) * playerSpeed;
                    newPos.X += Math.Cos(playerAngle.GetRad()) * playerSpeed;
                }
                if (keysPressed.Contains(Keys.Q))
                {
                    newPos.Y += Math.Sin(playerAngle.GetRad()) * playerSpeed;
                    newPos.X -= Math.Cos(playerAngle.GetRad()) * playerSpeed;
                }

                Vector2 optPos = new Vector2(newPos.X - newPos.X % blockSize, newPos.Y - newPos.Y % blockSize);

                if (doors.ContainsKey(optPos))
                {
                    if ((bool)doors[optPos].data["Closing"])
                    {
                        newPos = new Vector2(playerPos.X, playerPos.Y);
                    }
                }
                if (tileMap.ContainsKey(optPos))
                {
                    newPos = new Vector2(playerPos.X, playerPos.Y);
                }
                if (blocks.ContainsKey(optPos))
                {
                    newPos = new Vector2(playerPos.X, playerPos.Y);
                }

                playerPos = newPos;
                if (keysPressed.Contains(Keys.Space))
                {
                    if (!released)
                    {
                        released = true;
                        foreach (Entity door in doors.Values)
                        {
                            double dist = door.Position.Dist(playerPos);
                            if (dist < 150)
                            {
                                bool closing = !(bool)door.data["Closing"];
                                double timer = (int)door.data["Timer"];
                                if (!closing)
                                {
                                    Audio song = new Audio("Sounds/DoorOpen.wav", false);
                                    song.Play();
                                }
                                door.data["Closing"] = closing;
                                door.data["Played"] = false;
                                door.data["Timer"] = 0;
                            }
                        }
                        foreach (Vector2 key in blocks.Keys)
                        {
                            Entity block = blocks[key];
                            double dist = new Vector2(block.Position.X + blockSize / 2, block.Position.Y + blockSize / 2).Dist(playerPos);
                            if (dist < 150)
                            {
                                double x = 0;
                                double y = 0;
                                if ((playerAngle < 45) || (playerAngle >= 315)) { x = 0; y = blockSize * 2; }
                                else if (playerAngle >= 45 && playerAngle < 135) { x = blockSize * 2; y = 0; }
                                else if (playerAngle >= 135 && playerAngle < 225) { x = 0; y = -blockSize * 2; }
                                else if (playerAngle >= 225 && playerAngle < 315) { x = -blockSize * 2; y = 0; }

                                Tile t = tileMap[key];
                                t.Position.Y += y;
                                t.Position.X += x;
                                block.Position.X += x;
                                block.Position.Y += y;

                                tileMap.Remove(key);
                                blocks.Remove(key);
                                key.Y += y;
                                key.X += x;
                                blocks.Add(key, block);
                                tileMap.Add(key, t);
                                break;
                            }
                        }
                    }
                }
                else if (released)
                {
                    released = false;
                }



                foreach (Entity door in doors.Values)
                {
                    bool closing = (bool)door.data["Closing"];
                    if (closing)
                    {
                        double dist = door.Position.Dist(playerPos);
                        if (Entities.GetEntity(door.EntityType).Type == EntityType.EntityClass.DOOR)
                        {
                            double offset = (double)door.data["OffSetX"];
                            if (offset > 0)
                            {
                                if (!(bool)door.data["Played"])
                                {
                                    Audio song = new Audio("Sounds/DoorClose.wav", false);
                                    song.Play();
                                    door.data["Played"] = true;
                                }
                                offset -= 2;
                            }
                            door.data["OffSetX"] = offset;
                        }
                        else
                        {
                            double offset = (double)door.data["OffSetY"];
                            if (offset > 0)
                            {
                                if (!(bool)door.data["Played"])
                                {
                                    Audio song = new Audio("Sounds/DoorClose.wav", false);
                                    song.Play();
                                    door.data["Played"] = true;
                                }
                                offset -= 2;
                            }
                            door.data["OffSetY"] = offset;
                        }
                    }
                    else
                    {
                        if (Entities.GetEntity(door.EntityType).Type == EntityType.EntityClass.DOOR)
                        {
                            double offset = (double)door.data["OffSetX"];
                            if (offset < 64)
                            {
                                offset += 2;
                                door.data["OffSetX"] = offset;
                            }
                            else
                            {
                                int timer = (int)door.data["Timer"];
                                timer++;
                                door.data["Timer"] = timer;
                                if (timer > 255) door.data["Closing"] = true;
                            }
                        }
                        else
                        {
                            double offset = (double)door.data["OffSetY"];
                            if (offset < 64)
                            {
                                offset += 2;
                                door.data["OffSetY"] = offset;
                            }
                            else
                            {
                                int timer = (int)door.data["Timer"];
                                timer++;
                                door.data["Timer"] = timer;
                                if (timer > 255) door.data["Closing"] = true;
                            }
                        }
                    }
                }
                DrawFrame();
            }
        }

        public void DrawFrame()
        {
            display = new Bitmap((int)resolution.X, (int)resolution.Y);
            using (Graphics g1 = Graphics.FromImage(display))
            {
                g1.Clear(Color.Purple);
                g1.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g1.FillRectangle(Brushes.Cyan, new RectangleF(0, 0, (float)resolution.X, (float)(resolution.Y / 2)));
                g1.FillRectangle(Brushes.Gray, new RectangleF(0, (float)(resolution.Y / 2), (float)resolution.X, (float)(resolution.Y / 2)));
                double angle = playerAngle - FOV / 2;
                double angleAdd = FOV / resolution.X;
                double[] zBuffer = new double[(int)resolution.X];
                for (double x = 0; x < resolution.X; x++)
                {
                    var result = CastRay(angle + angleAdd * x);
                    if (result.success)
                    {
                        List<(Entity entity, Vector2 point, bool vertical)> entities = result.entities.OrderByDescending(t => t.point.Dist(playerPos)).ToList();
                        var point = GetTexturePointAndDistance(playerPos, result.point, angleAdd * x, result.vertical);
                        Vector2 position = new Vector2(result.point.X - result.point.X % blockSize, result.point.Y - result.point.Y % blockSize);
                        Bitmap texture;
                        if (result.vertical)
                        {
                            if (doors.ContainsKey(new Vector2(position.X - blockSize, position.Y)))
                                texture = Entities.GetTexture(doors[new Vector2(position.X - blockSize, position.Y)].EntityType, 1);
                            else if (doors.ContainsKey(new Vector2(position.X, position.Y)))
                                texture = Entities.GetTexture(doors[new Vector2(position.X, position.Y)].EntityType, 1);
                            else texture = Tiles.GetTexture(result.tile.Type, true);
                        }
                        else
                        {
                            if (doors.ContainsKey(new Vector2(position.X, position.Y - blockSize)))
                                texture = Entities.GetTexture(doors[new Vector2(position.X, position.Y - blockSize)].EntityType, 1);
                            else if (doors.ContainsKey(new Vector2(position.X, position.Y)))
                                texture = Entities.GetTexture(doors[new Vector2(position.X, position.Y)].EntityType, 1);
                            else texture = Tiles.GetTexture(result.tile.Type, false);
                        }
                        double dist = point.distance;
                        dist = dist * Math.Cos(((Angle)(playerAngle- angle)).GetRad());
                        double size = blockSize / dist;
                        if (point.distance > 1)
                        {
                            size *= resolution.Y;
                            g1.DrawImage(texture, new RectangleF((float)x - 1, (float)(resolution.Y / 2 - size / 2), 2, (float)size), new RectangleF((float)point.point, 0, 1, texture.Height), GraphicsUnit.Pixel);
                            zBuffer[(int)x] = point.distance;
                        }

                        foreach ((Entity entity, Vector2 position, bool vertical) entity in entities)
                        {
                            Bitmap textur = Entities.GetTexture(entity.entity.EntityType, 0);
                            if (Entities.GetEntity(entity.entity.EntityType).Type == EntityType.EntityClass.DOOR ||
                                Entities.GetEntity(entity.entity.EntityType).Type == EntityType.EntityClass.DOOR90)
                            {
                                var pont = GetTexturePointAndDistance(playerPos, new Vector2(entity.position.X - (double)entity.entity.data["OffSetX"], entity.position.Y - (double)entity.entity.data["OffSetY"]), angleAdd * x, entity.vertical);

                                double siz = blockSize / pont.distance;
                                if (pont.distance > 1)
                                {
                                    zBuffer[(int)x] = pont.distance;
                                    siz *= resolution.Y;
                                    Rectangle drawRec = new RectangleF((float)x - 1, (float)((resolution.Y / 2 - siz / 2) * Math.Cos(Angle.GetRad(angleAdd))), 2, (float)siz);
                                    g1.DrawImage(textur, drawRec, new RectangleF((float)pont.point, 0, 1, textur.Height), GraphicsUnit.Pixel);
                                }
                            }
                        }
                    }
                }

                foreach (Entity entity in gameentites)
                {
                    Vector2 spritePosWorld = new Vector2(entity.Position.X, entity.Position.Y);
                    spritePosWorld.X+=blockSize/2;
                    spritePosWorld.Y += blockSize/2;
                    Vector2 spritePosView = new Vector2(spritePosWorld.X - playerPos.X, spritePosWorld.Y - playerPos.Y);
                    double distSprite = spritePosWorld.Dist(playerPos);
                    double cameraPosX = Math.Atan2(spritePosView.Y, spritePosView.X);
                    Angle realAngle = 0;
                    realAngle.SetRad(Math.Atan2(spritePosView.Y, spritePosView.X));
                    realAngle += playerAngle;
                    realAngle -= 90;
                    double a = (double)realAngle;
                    //Console.WriteLine(a);
                    if (a > 320)
                    {
                        a = 360 - a;
                        cameraPosX = resolution.X / 2 + (a) / (FOV / 2) * resolution.X / 2 - blockSize / (distSprite / 100);
                        double siz = blockSize / distSprite;
                        siz *= resolution.Y;
                        for (int x = 0; x < blockSize; x++)
                        {
                            int realX = (int)(cameraPosX + x * siz / blockSize);
                            Rectangle drawRec = new Rectangle(realX, this.resolution.Y / 2 - siz / 2, siz / (blockSize / 2), siz);
                            if (drawRec.Width < 1) drawRec.Width = 1;
                            if ((realX > 0) && (realX < resolution.X))
                            {
                                if (distSprite < zBuffer[realX])
                                    g1.DrawImage(Entities.GetTexture(entity.EntityType, 0), drawRec, new Rectangle(x, 0, 2, 64), GraphicsUnit.Pixel);
                            }
                        }
                    }
                    else if (a < 30)
                    {
                        a = 30 - a;
                        cameraPosX = (a) / (FOV / 2) * resolution.X / 2 - blockSize/(distSprite/100);
                        double siz = blockSize / distSprite;
                        siz *= resolution.Y;
                        for (int x = 0; x < blockSize; x++)
                        {
                            int realX = (int)(cameraPosX + x * siz / blockSize);

                            Rectangle drawRec = new Rectangle(realX, this.resolution.Y / 2 - siz / 2, siz / (blockSize / 2), siz);
                            if (drawRec.Width < 1) drawRec.Width = 1;
                            if ((realX > 0) && (realX < resolution.X))
                            {
                                if (distSprite < zBuffer[realX])
                                    g1.DrawImage(Entities.GetTexture(entity.EntityType, 0), drawRec, new Rectangle(x, 0, 2, 64), GraphicsUnit.Pixel);
                            }
                        }
                    }
                }
                if (screenshot)
                {
                    if (!System.IO.Directory.Exists("Shots")) System.IO.Directory.CreateDirectory("Shots");
                    display.Save("Shots\\" + shotCount + ".png");
                    shotCount++;
                }
                if (recording >= 0)
                {
                    if (!System.IO.Directory.Exists("Recs")) System.IO.Directory.CreateDirectory("Recs");
                    display.Save("Recs\\" + recording + ".png");
                    recording++;
                    g1.FillEllipse(Brushes.Red, new RectangleF(10, 10, 32, 32));
                }
            }
            SetPictureBox(display);
        }

        public delegate void _setPictureBox(Bitmap bmp);
        public void SetPictureBox(Bitmap bmp)
        {
            if (pictureBox1.InvokeRequired)
            {
                try
                {
                    pictureBox1.Invoke(new _setPictureBox(SetPictureBox), bmp);
                }
                catch
                {

                }
            }
            else pictureBox1.Image = bmp;
        }

        public (double point, double distance) GetTexturePointAndDistance(Vector2 position, Vector2 collision, double castAngle, bool vertical)
        {
            double distance = position.Dist(collision);
            if (vertical)
            {
                return (collision.Y % blockSize, distance);
            }
            else
            {
                return (collision.X % blockSize, distance);
            }
        }

        public (bool success, Vector2 point, Tile tile, bool vertical, List<(Entity entity, Vector2 point, bool vertical)> entities) CastRay(Angle angle)
        {
            var resultHorizontal = CastRayHorizontal(angle);
            var resultVertical = CastRayVertical(angle);
            List<(Entity entity, Vector2 point, bool vertical)> entities = new List<(Entity entity, Vector2 point, bool vertical)>();
            entities.AddRange(resultHorizontal.entities);
            entities.AddRange(resultVertical.entities);

            if (resultHorizontal.success)
            {

                if (!resultVertical.success)
                    return (resultHorizontal.success, resultHorizontal.point, resultHorizontal.tile, false, entities);


                if (resultHorizontal.point.Dist(playerPos) < resultVertical.point.Dist(playerPos))
                    return (resultHorizontal.success, resultHorizontal.point, resultHorizontal.tile, false, entities);
                else return (resultVertical.success, resultVertical.point, resultVertical.tile, true, entities);
            }

            if (resultVertical.success)
            {
                return (resultVertical.success, resultVertical.point, resultVertical.tile, true, entities);
            }

            return (false, null, null, false, entities);
        }

        public (bool success, Vector2 point, Tile tile, List<(Entity entity, Vector2 point, bool vertical)> entities) CastRayHorizontal(Angle angle)
        {
            int retries = 0;
            Vector2 rayPoint = new Vector2(playerPos.X, playerPos.Y);
            Tile tile = TileIntersects(rayPoint);
            List<(Entity entity, Vector2 point, bool vertical)> entities = new List<(Entity entity, Vector2 point, bool vertical)>();

            Vector2 temp = getMidPointHorizontal(rayPoint, angle);
            Entity door = DoorIntersects(temp);
            if (door != null)
                if (Entities.GetEntity(door.EntityType).Type == EntityType.EntityClass.DOOR)
                    entities.Add((door, temp, false));
            
            if (tile != null)
                return (true, rayPoint, tile, entities);

            while (retries < maxDistance)
            {
                retries++;
                rayPoint = getPointHorizontal(rayPoint, angle);
                tile = TileIntersects(rayPoint);
                if (door != null)
                    if (Entities.GetEntity(door.EntityType).Type == EntityType.EntityClass.DOOR)
                        entities.Add((door, temp, false));
                temp = getMidPointHorizontal(rayPoint, angle);
                door = DoorIntersects(temp);
                if (tile != null)
                {
                    return (true, rayPoint, tile, entities);
                }
            }

            return (false, null, null, entities);
        }

        public (bool success, Vector2 point, Tile tile, List<(Entity entity, Vector2 point, bool vertical)> entities) CastRayVertical(Angle angle)
        {
            int retries = 0;
            Vector2 rayPoint = new Vector2(playerPos.X, playerPos.Y);
            Tile tile = TileIntersects(rayPoint);
            List<(Entity entity, Vector2 point, bool vertical)> entities = new List<(Entity entity, Vector2 point, bool vertical)>();
            Vector2 temp = getMidPointVertical(rayPoint, angle);
            Entity door = DoorIntersects(temp);

            if (door != null)
                if (Entities.GetEntity(door.EntityType).Type == EntityType.EntityClass.DOOR90)
                    entities.Add((door, temp, true));
            if (tile != null) return (true, rayPoint, tile, entities);
            while (retries < maxDistance)
            {
                retries++;
                rayPoint = getPointVertical(rayPoint, angle);
                tile = TileIntersects(rayPoint);
                if (door != null)
                    if (Entities.GetEntity(door.EntityType).Type == EntityType.EntityClass.DOOR90)
                        entities.Add((door, temp, true));

                temp = getMidPointVertical(rayPoint, angle);
                door = DoorIntersects(temp);
                if (tile != null)
                {
                    return (true, rayPoint, tile, entities);
                }
            }

            return (false, null, null, entities);
        }


        public Entity DoorIntersects(Vector2 point)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y < 1; y++)
                {
                    Vector2 tPos = new Vector2(point.X - point.X % blockSize + x * blockSize, point.Y - point.Y % blockSize + y * blockSize);
                    if (doors.ContainsKey(tPos))
                    {
                        if (DoorIntersects(doors[tPos], point))
                            return doors[tPos];
                    }
                }
            }

            return null;
        }

        public bool DoorIntersects(Entity tile, Vector2 point)
        {
            return ((tile.Position.X + (double)tile.data["OffSetX"] <= point.X) && (tile.Position.X + blockSize >= point.X) &&
                (tile.Position.Y + (double)tile.data["OffSetY"] <= point.Y) && (tile.Position.Y + blockSize >= point.Y));
        }


        public Tile TileIntersects(Vector2 point)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y < 1; y++)
                {
                    Vector2 tPos = new Vector2(point.X - point.X % blockSize + x * blockSize, point.Y - point.Y % blockSize + y * blockSize);
                    if (tileMap.ContainsKey(tPos))
                    {
                        if (TileIntersects(tileMap[tPos], point))
                            return tileMap[tPos];
                    }
                }
            }

            return null;
        }

        public bool TileIntersects(Tile tile, Vector2 point)
        {
            return ((tile.Position.X <= point.X) && (tile.Position.X + blockSize >= point.X) &&
                (tile.Position.Y <= point.Y) && (tile.Position.Y + blockSize >= point.Y));
        }

        public Vector2 getMidPointHorizontal(Vector2 current, Angle angle)
        {
            Vector2 pos2 = null;
            double Y;
            double blockSizeHalf = blockSize / 2;
            switch ((int)angle / 90)
            {
                case 0:
                    pos2 = new Vector2(current.X, current.Y - current.Y % blockSizeHalf + blockSizeHalf);
                    break;
                case 1:
                    Y = current.Y % blockSizeHalf;
                    pos2 = new Vector2(current.X, current.Y - current.Y % blockSizeHalf);
                    if (Y <= 0) pos2.Y -= blockSizeHalf;
                    break;
                case 2:
                    Y = current.Y % blockSizeHalf;
                    pos2 = new Vector2(current.X, current.Y - current.Y % blockSizeHalf);
                    if (Y <= 0) pos2.Y -= blockSizeHalf;
                    break;
                case 3:
                    pos2 = new Vector2(current.X, current.Y - current.Y % blockSizeHalf + blockSizeHalf);
                    break;
            }
            double A = Angle.GetRad(90);
            double B = Angle.GetRad(angle);
            double c = Vector2.Dist(current, pos2);
            double b = Math.Abs((c / Math.Sin(Math.PI - A - B)) * Math.Sin(B));
            switch ((int)angle / 90)
            {
                case 0:
                    pos2.X += b;
                    break;
                case 1:
                    pos2.X += b;
                    break;
                case 2:
                    pos2.X -= b;
                    break;
                case 3:
                    pos2.X -= b;
                    break;
            }
            return pos2;
        }

        public Vector2 getMidPointVertical(Vector2 current, Angle angle)
        {
            Vector2 pos2 = null;
            double X;
            double blockSizeHalf = blockSize / 2;
            switch ((int)angle / 90)
            {
                case 0:
                    pos2 = new Vector2(current.X - current.X % blockSizeHalf + blockSizeHalf, current.Y);
                    break;
                case 1:
                    pos2 = new Vector2(current.X - current.X % blockSizeHalf + blockSizeHalf, current.Y);
                    break;
                case 2:
                    X = current.X % blockSize;
                    pos2 = new Vector2(current.X - current.X % blockSizeHalf, current.Y);
                    if (X <= 0) pos2.X -= blockSizeHalf;
                    break;
                case 3:
                    X = current.X % blockSize;
                    pos2 = new Vector2(current.X - current.X % blockSizeHalf, current.Y);
                    if (X <= 0) pos2.X -= blockSizeHalf;
                    break;
            }
            double A = Angle.GetRad(90);
            double B = Angle.GetRad(angle);
            double c = Vector2.Dist(current, pos2);
            double b = Math.Abs((c / Math.Cos(Math.PI - A - B)) * Math.Cos(B));
            switch ((int)angle / 90)
            {
                case 0:
                    pos2.Y += b;
                    break;
                case 1:
                    pos2.Y -= b;
                    break;
                case 2:
                    pos2.Y -= b;
                    break;
                case 3:
                    pos2.Y += b;
                    break;
            }
            return pos2;
        }

        public Vector2 getPointHorizontal(Vector2 current, Angle angle)
        {
            Vector2 pos2 = null;
            double Y;
            switch ((int)angle / 90)
            {
                case 0:
                    pos2 = new Vector2(current.X, current.Y - current.Y % blockSize + blockSize);
                    break;
                case 1:
                    Y = current.Y % blockSize;
                    pos2 = new Vector2(current.X, current.Y - current.Y % blockSize);
                    if (Y <= 0) pos2.Y -= blockSize;
                    break;
                case 2:
                    Y = current.Y % blockSize;
                    pos2 = new Vector2(current.X, current.Y - current.Y % blockSize);
                    if (Y <= 0) pos2.Y -= blockSize;
                    break;
                case 3:
                    pos2 = new Vector2(current.X, current.Y - current.Y % blockSize + blockSize);
                    break;
            }
            double A = Angle.GetRad(90);
            double B = Angle.GetRad(angle);
            double c = Vector2.Dist(current, pos2);
            double b = Math.Abs((c / Math.Sin(Math.PI - A - B)) * Math.Sin(B));
            switch ((int)angle / 90)
            {
                case 0:
                    pos2.X += b;
                    break;
                case 1:
                    pos2.X += b;
                    break;
                case 2:
                    pos2.X -= b;
                    break;
                case 3:
                    pos2.X -= b;
                    break;
            }
            return pos2;
        }

        public Vector2 getPointVertical(Vector2 current, Angle angle)
        {
            Vector2 pos2 = null;
            double X;
            switch ((int)angle / 90)
            {
                case 0:
                    pos2 = new Vector2(current.X - current.X % blockSize + blockSize, current.Y);
                    break;
                case 1:
                    pos2 = new Vector2(current.X - current.X % blockSize + blockSize, current.Y);
                    break;
                case 2:
                    X = current.X % blockSize;
                    pos2 = new Vector2(current.X - current.X % blockSize, current.Y);
                    if (X <= 0) pos2.X -= blockSize;
                    break;
                case 3:
                    X = current.X % blockSize;
                    pos2 = new Vector2(current.X - current.X % blockSize, current.Y);
                    if (X <= 0) pos2.X -= blockSize;
                    break;
            }
            double A = Angle.GetRad(90);
            double B = Angle.GetRad(angle);
            double c = Vector2.Dist(current, pos2);
            double b = Math.Abs((c / Math.Cos(Math.PI - A - B)) * Math.Cos(B));
            switch ((int)angle / 90)
            {
                case 0:
                    pos2.Y += b;
                    break;
                case 1:
                    pos2.Y -= b;
                    break;
                case 2:
                    pos2.Y -= b;
                    break;
                case 3:
                    pos2.Y += b;
                    break;
            }
            return pos2;
        }

        public List<Keys> keysPressed = new List<Keys>();

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (!keysPressed.Contains(e.KeyCode))
                keysPressed.Add(e.KeyCode);
            if (e.Modifiers == Keys.Alt)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    SwitchFullScreen();
                }
            }
            if (e.KeyCode == Keys.F12)
            {
                screenshot = true;
            }
            if (e.KeyCode == Keys.F11)
            {
                recording = 0;
            }
        }

        bool fullscreen = false;

        public void SwitchFullScreen()
        {
            fullscreen = !fullscreen;
            if (fullscreen)
            {
                originalBounds = this.Bounds;
                this.FormBorderStyle = FormBorderStyle.None;
                this.Focus();
                this.BringToFront();
                this.TopMost = true;
                this.Bounds = Screen.PrimaryScreen.Bounds;
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.FixedSingle;
                this.WindowState = FormWindowState.Normal;
                this.TopMost = false;
                this.Bounds = originalBounds;
            }
        }

        public Rectangle originalBounds;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0112) // WM_SYSCOMMAND
            {
                // Check your window state here
                if (m.WParam == new IntPtr(0xF030)) // Maximize event - SC_MAXIMIZE from Winuser.h
                {
                    if (!fullscreen) SwitchFullScreen();
                }
            }
            base.WndProc(ref m);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Audio song = new Audio("Sounds/Level1.wav", true);
            song.Play();
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (keysPressed.Contains(e.KeyCode))
                keysPressed.Remove(e.KeyCode);
        }
    }
}