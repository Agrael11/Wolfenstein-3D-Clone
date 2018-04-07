using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3DTest
{
    public class Game
    {
        public Graphics Graphics;
        public double BlockSize = 64;
        public Angle PlayerAngle = 0;
        public double PlayerSpeed = 4;
        public Vector2 PlayerPos = new Vector2(32, 32);
        public List<Keys> Pressedkeys = new List<Keys>();

        public Dictionary<Vector2, Tile> TileMap = new Dictionary<Vector2, Tile>();
        public Dictionary<Vector2, Entity> Doors = new Dictionary<Vector2, Entity>();
        public Dictionary<Vector2, Entity> Blocks = new Dictionary<Vector2, Entity>();
        public List<Entity> GameEntities = new List<Entity>();


        private bool spaceReleased = false;




        public void Init()
        {
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
            Entities.AddEntity("Statue", new EntityType(EntityType.EntityClass.SPRITE, true, new List<string>() { "Statue" }));
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
                        switch (c.B)
                        {
                            case 0: AddTile("Blue", x, y); break;
                            case 1: AddTile("Stone", x, y); break;
                            case 2: AddTile("Dirt", x, y); break;
                        }
                    }
                    else if (c.B == 255)
                    {
                        switch (c.R)
                        {
                            case 0: AddDoor("Door", x, y); break;
                            case 1: AddDoor("Door90", x, y); break;
                            case 2: AddBlock("Block", "Stone", x, y); break;
                            case 3: AddBlock("Block", "Dirt", x, y); break;
                            case 4: AddBlock("Block", "Blue", x, y); break;
                            case 5: AddEntity("Statue", x, y); break;
                            case 6: AddEntity("Lamp", x, y); break;
                            case 7: AddEntity("Table", x, y); break;
                        }
                    }
                    else if (c.R == 255)
                    {
                        PlayerPos = new Vector2((x + 0.5) * 64, (y + 0.5) * 64);
                        switch (c.G)
                        {
                            case 0: PlayerAngle = 0; break;
                            case 1: PlayerAngle = 90; break;
                            case 2: PlayerAngle = 180; break;
                            case 3: PlayerAngle = 270; break;
                        }
                    }
                }
            }
        }

        public void AddTile(string id, double X, double Y)
        {
            TileMap.Add(new Vector2(X * BlockSize, Y * BlockSize), new Tile(id, new Vector2(X * BlockSize, Y * BlockSize)));
        }

        public void AddDoor(string id, double X, double Y)
        {
            Entity door = new Entity(id, new Vector2(X * BlockSize, Y * BlockSize));
            door.data.Add("Closing", true);
            door.data.Add("Played", false);
            door.data.Add("OffSetX", 0.0);
            door.data.Add("OffSetY", 0.0);
            door.data.Add("Timer", 0);
            Doors.Add(new Vector2(X * BlockSize, Y * BlockSize), door);
        }

        public void AddBlock(string blockId, string tileId, double X, double Y)
        {
            Blocks.Add(new Vector2(X * BlockSize, Y * BlockSize), new Entity(blockId, new Vector2(X * BlockSize, Y * BlockSize)));
            AddTile(tileId, X, Y);
        }

        public void AddEntity(string id, double X, double Y)
        {
            GameEntities.Add(new Entity(id, new Vector2(X * BlockSize, Y * BlockSize)));
        }




        public void Update()
        {
            Vector2 newPosition = new Vector2(PlayerPos.X, PlayerPos.Y);

            if (Pressedkeys.Contains(Keys.A))
            {
                PlayerAngle -= PlayerSpeed / 3;
            }
            if (Pressedkeys.Contains(Keys.D))
            {
                PlayerAngle += PlayerSpeed / 3;
            }
            if (Pressedkeys.Contains(Keys.W))
            {
                newPosition.Y += Math.Cos(PlayerAngle.GetRad()) * PlayerSpeed;
                newPosition.X += Math.Sin(PlayerAngle.GetRad()) * PlayerSpeed;
            }
            if (Pressedkeys.Contains(Keys.S))
            {
                newPosition.Y -= Math.Cos(PlayerAngle.GetRad()) * PlayerSpeed;
                newPosition.X -= Math.Sin(PlayerAngle.GetRad()) * PlayerSpeed;
            }
            if (Pressedkeys.Contains(Keys.E))
            {
                newPosition.Y -= Math.Sin(PlayerAngle.GetRad()) * PlayerSpeed;
                newPosition.X += Math.Cos(PlayerAngle.GetRad()) * PlayerSpeed;
            }
            if (Pressedkeys.Contains(Keys.Q))
            {
                newPosition.Y += Math.Sin(PlayerAngle.GetRad()) * PlayerSpeed;
                newPosition.X -= Math.Cos(PlayerAngle.GetRad()) * PlayerSpeed;
            }

            Vector2 keyPosition = new Vector2(newPosition.X - newPosition.X % BlockSize, newPosition.Y - newPosition.Y % BlockSize);

            if (Doors.ContainsKey(keyPosition))
            {
                if ((bool)Doors[keyPosition].data["Closing"])
                {
                    newPosition = new Vector2(PlayerPos.X, PlayerPos.Y);
                }
            }
            if (TileMap.ContainsKey(keyPosition))
            {
                newPosition = new Vector2(PlayerPos.X, PlayerPos.Y);
            }
            if (Blocks.ContainsKey(keyPosition))
            {
                newPosition = new Vector2(PlayerPos.X, PlayerPos.Y);
            }

            PlayerPos = newPosition;
            if (Pressedkeys.Contains(Keys.Space))
            {
                if (!spaceReleased)
                {
                    spaceReleased = true;
                    foreach (Entity door in Doors.Values)
                    {
                        double dist = door.Position.Dist(PlayerPos);
                        if (dist < 150)
                        {
                            bool closing = !(bool)door.data["Closing"];
                            double timer = (int)door.data["Timer"];
                            if (!closing)
                            {
                                Audio sound = new Audio("Sounds/DoorOpen.wav", false);
                                sound.Play();
                            }
                            door.data["Closing"] = closing;
                            door.data["Played"] = false;
                            door.data["Timer"] = 0;
                        }
                    }
                    foreach (Vector2 key in Blocks.Keys)
                    {
                        Entity block = Blocks[key];
                        double distance = new Vector2(block.Position.X + BlockSize / 2, block.Position.Y + BlockSize / 2).Dist(PlayerPos);
                        if (distance < 150)
                        {
                            double x = 0;
                            double y = 0;
                            if ((PlayerAngle < 45) || (PlayerAngle >= 315)) { x = 0; y = BlockSize * 2; }
                            else if (PlayerAngle >= 45 && PlayerAngle < 135) { x = BlockSize * 2; y = 0; }
                            else if (PlayerAngle >= 135 && PlayerAngle < 225) { x = 0; y = -BlockSize * 2; }
                            else if (PlayerAngle >= 225 && PlayerAngle < 315) { x = -BlockSize * 2; y = 0; }

                            Tile tile = TileMap[key];
                            tile.Position.Y += y;
                            tile.Position.X += x;
                            block.Position.X += x;
                            block.Position.Y += y;

                            TileMap.Remove(key);
                            Blocks.Remove(key);
                            key.Y += y;
                            key.X += x;
                            Blocks.Add(key, block);
                            TileMap.Add(key, tile);
                            break;
                        }
                    }
                }
            }
            else if (spaceReleased)
            {
                spaceReleased = false;
            }



            foreach (Entity door in Doors.Values)
            {
                bool closing = (bool)door.data["Closing"];
                if (closing)
                {
                    double dist = door.Position.Dist(PlayerPos);
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
        }

        public void Draw()
        {
            Rendering.Renderer3D.DrawFrame();
            Graphics.DrawImage(Rendering.Renderer3D.DrawFrame(), new Point(0,0));
        }
    }
}
