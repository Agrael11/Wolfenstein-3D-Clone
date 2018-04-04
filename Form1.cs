using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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
        double maxDistance = 200;
        double playerSpeed = 7;
        Bitmap display;

        Vector2 playerPos = new Vector2(32, 32);
        private Angle playerAngle = 0;

        Angle FOV = 60;
        Vector2 resolution = new Vector2(320, 200);
        Dictionary<Vector2, Tile> tileMap = new Dictionary<Vector2, Tile>();
        Dictionary<Vector2, Entity> doors = new Dictionary<Vector2, Entity>();

        public void AddTile(string id, double X, double Y)
        {
            tileMap.Add(new Vector2(X * blockSize, Y * blockSize), new Tile(id, new Vector2(X * blockSize, Y * blockSize)));
        }

        public void AddDoor(string id, double X, double Y)
        {
            Entity door = new Entity(id, new Vector2(X * blockSize, Y * blockSize));
            door.data.Add("OffSetX", 0.0);
            door.data.Add("OffSetY", 0.0);
            door.data.Add("Closing", true);
            door.data.Add("Timer", 0);
            doors.Add(new Vector2(X * blockSize, Y * blockSize), door);
        }

        public Form1()
        {
            InitializeComponent();

            pictureBox1.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

            Textures.LoadTexture("Blue", "WallBlue.png");
            Textures.LoadTexture("Dirt", "WallDirt.png");
            Textures.LoadTexture("Stone", "WallStone.png");
            Textures.LoadTexture("Blue90", "WallBlue90.png");
            Textures.LoadTexture("Dirt90", "WallDirt90.png");
            Textures.LoadTexture("Stone90", "WallStone90.png");
            Textures.LoadTexture("DoorBlue", "DoorBlue.png");
            Textures.LoadTexture("DoorBlue2", "DoorBlue2.png");
            Tiles.AddTile("Blue", new TileType("Blue", "Blue90"));
            Tiles.AddTile("Dirt", new TileType("Dirt", "Dirt90"));
            Tiles.AddTile("Stone", new TileType("Stone", "Stone90"));
            Entities.AddEntity("Door", new EntityType(EntityType.EntityClass.DOOR, false, new List<string>() { "DoorBlue", "DoorBlue2" }));
            Entities.AddEntity("Door90", new EntityType(EntityType.EntityClass.DOOR90, false, new List<string>() { "DoorBlue", "DoorBlue2" }));

            Bitmap level = (Bitmap)Image.FromFile("levelmap.bmp");
            for (int x = 0; x < level.Width; x++)
            {
                for (int y = 0; y < level.Height; y++)
                {
                    if (level.GetPixel(x, y).B == 132) AddTile("Blue", x, y);
                    else if (level.GetPixel(x, y).R == 164) AddTile("Dirt", x, y);
                    else if (level.GetPixel(x, y).B == 157) AddTile("Stone", x, y);
                    else if (level.GetPixel(x, y).B == 157) AddTile("Stone", x, y);
                    else if (level.GetPixel(x, y).B == 255)
                    {
                        if (level.GetPixel(x, y).R == 255)
                            AddDoor("Door90", x, y);
                        else AddDoor("Door", x, y);
                    }
                }
            }

            playerAngle = 270;
            playerPos = new Vector2(68 * 64, 47.5 * 64);
            Thread thr = new Thread(Draw);
            thr.Start();

        }

        public void Draw()
        {
            while (!this.Visible) ;
            while (this.Visible)
            {
                foreach (Entity door in doors.Values)
                {
                    bool closing = (bool)door.data["Closing"];
                    if (closing)
                    {
                        if (Entities.GetEntity(door.EntityType).Type == EntityType.EntityClass.DOOR)
                        {
                            double offset = (double)door.data["OffSetX"];
                            if (offset > 0) offset -= 2;
                            door.data["OffSetX"] = offset;
                        }
                        else
                        {
                            double offset = (double)door.data["OffSetY"];
                            if (offset > 0) offset -= 2;
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
                //door1.OffSetX++;
                //door1.OffSetX %= 60;
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
                for (double x = 0; x <= resolution.X; x++)
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
                        double size = blockSize / point.distance;
                        if (point.distance > 1)
                        {
                            size *= resolution.Y;
                            g1.DrawImage(texture, new RectangleF((float)x - 2, (float)((resolution.Y / 2 - size / 2) * Math.Cos(Angle.GetRad(angleAdd))), 4, (float)size), new RectangleF((float)point.point, 0, 1, texture.Height), GraphicsUnit.Pixel);
                        }

                        foreach ((Entity entity, Vector2 position, bool vertical) entity in entities)
                        {
                            Bitmap textur = Entities.GetTexture(entity.entity.EntityType, 0);
                            var pont = GetTexturePointAndDistance(playerPos, new Vector2(entity.position.X - (double)entity.entity.data["OffSetX"], entity.position.Y - (double)entity.entity.data["OffSetY"]), angleAdd * x, entity.vertical);
                            double siz = blockSize / pont.distance;
                            if (pont.distance > 1)
                            {
                                siz *= resolution.Y;
                                g1.DrawImage(textur, new RectangleF((float)x - 2, (float)((resolution.Y / 2 - siz / 2) * Math.Cos(Angle.GetRad(angleAdd))), 4, (float)siz), new RectangleF((float)pont.point, 0, 1, textur.Height), GraphicsUnit.Pixel);
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

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine($"Angle {(double)playerAngle}; X: {playerPos.X}; Y: {playerPos.Y} ");
            Vector2 newPos = new Vector2(playerPos.X, playerPos.Y);

            if (e.KeyCode == Keys.A)
            {
                playerAngle -= playerSpeed / 4;
            }
            if (e.KeyCode == Keys.D)
            {
                playerAngle += playerSpeed / 4;
            }
            if (e.KeyCode == Keys.W)
            {
                newPos.Y += Math.Cos(playerAngle.GetRad()) * playerSpeed;
                newPos.X += Math.Sin(playerAngle.GetRad()) * playerSpeed;
            }
            if (e.KeyCode == Keys.S)
            {
                newPos.Y -= Math.Cos(playerAngle.GetRad()) * playerSpeed;
                newPos.X -= Math.Sin(playerAngle.GetRad()) * playerSpeed;
            }
            if (e.KeyCode == Keys.E)
            {
                newPos.Y -= Math.Sin(playerAngle.GetRad()) * playerSpeed;
                newPos.X += Math.Cos(playerAngle.GetRad()) * playerSpeed;
            }
            if (e.KeyCode == Keys.Q)
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

            playerPos = newPos;
            if (e.KeyCode == Keys.Space)
            {
                foreach (Entity door in doors.Values)
                {
                    double dist = door.Position.Dist(playerPos);
                    if (dist < 150)
                    {
                        bool closing = !(bool)door.data["Closing"];
                        door.data["Closing"] = closing;
                        door.data["Timer"] = 0;
                    }
                }
            }
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
    }
}