using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DTest.Rendering
{
    public static class Renderer3D
    {

        public static double blockSize = 64;
        public static Angle FOV = 60;
        public static double maxDistance = 80;
        public static Vector2 resolution = new Vector2(320, 200);

        public static Bitmap DrawFrame()
        {
            Bitmap target = new Bitmap((int)resolution.X, (int)resolution.Y);
            using (Graphics graphics = Graphics.FromImage(target))
            {
                graphics.Clear(Color.Purple);
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                graphics.FillRectangle(Brushes.Cyan, new RectangleF(0, 0, (float)resolution.X, (float)(resolution.Y / 2)));
                graphics.FillRectangle(Brushes.Gray, new RectangleF(0, (float)(resolution.Y / 2), (float)resolution.X, (float)(resolution.Y / 2)));
                double angle = Program.game.PlayerAngle - FOV / 2;
                double angleAdd = FOV / resolution.X;
                double[] zBuffer = new double[(int)resolution.X];
                for (double x = 0; x < resolution.X; x++)
                {
                    var result = CastRay(angle + angleAdd * x);
                    if (result.success)
                    {
                        List<(Entity entity, Vector2 point, bool vertical)> entities = result.entities.OrderByDescending(t => t.point.Dist(Program.game.PlayerPos)).ToList();
                        var texturePointAndDistance = GetTexturePointAndDistance(Program.game.PlayerPos, result.point, angleAdd * x, result.vertical);
                        Vector2 position = new Vector2(result.point.X - result.point.X % blockSize, result.point.Y - result.point.Y % blockSize);
                        Bitmap texture;
                        if (result.vertical)
                        {
                            if (Program.game.Doors.ContainsKey(new Vector2(position.X - blockSize, position.Y)))
                                texture = Entities.GetTexture(Program.game.Doors[new Vector2(position.X - blockSize, position.Y)].EntityType, 1);
                            else if (Program.game.Doors.ContainsKey(new Vector2(position.X, position.Y)))
                                texture = Entities.GetTexture(Program.game.Doors[new Vector2(position.X, position.Y)].EntityType, 1);
                            else texture = Tiles.GetTexture(result.tile.Type, true);
                        }
                        else
                        {
                            if (Program.game.Doors.ContainsKey(new Vector2(position.X, position.Y - blockSize)))
                                texture = Entities.GetTexture(Program.game.Doors[new Vector2(position.X, position.Y - blockSize)].EntityType, 1);
                            else if (Program.game.Doors.ContainsKey(new Vector2(position.X, position.Y)))
                                texture = Entities.GetTexture(Program.game.Doors[new Vector2(position.X, position.Y)].EntityType, 1);
                            else texture = Tiles.GetTexture(result.tile.Type, false);
                        }
                        double distance = texturePointAndDistance.distance;
                        distance = distance * Math.Cos(((Angle)(Program.game.PlayerAngle - angle)).GetRad());
                        double size = blockSize / distance;
                        if (texturePointAndDistance.distance > 1)
                        {
                            size *= resolution.Y;
                            graphics.DrawImage(texture, new RectangleF((float)x - 1, (float)(resolution.Y / 2 - size / 2), 2, (float)size), new RectangleF((float)texturePointAndDistance.point, 0, 1, texture.Height), GraphicsUnit.Pixel);
                            zBuffer[(int)x] = texturePointAndDistance.distance;
                        }

                        foreach ((Entity entity, Vector2 position, bool vertical) entity in entities)
                        {
                            Bitmap textur = Entities.GetTexture(entity.entity.EntityType, 0);
                            if (Entities.GetEntity(entity.entity.EntityType).Type == EntityType.EntityClass.DOOR ||
                                Entities.GetEntity(entity.entity.EntityType).Type == EntityType.EntityClass.DOOR90)
                            {
                                var pont = GetTexturePointAndDistance(Program.game.PlayerPos, new Vector2(entity.position.X - (double)entity.entity.data["OffSetX"], entity.position.Y - (double)entity.entity.data["OffSetY"]), angleAdd * x, entity.vertical);

                                double siz = blockSize / pont.distance;
                                if (pont.distance > 1)
                                {
                                    zBuffer[(int)x] = pont.distance;
                                    siz *= resolution.Y;
                                    Rectangle drawRec = new RectangleF((float)x - 1, (float)((resolution.Y / 2 - siz / 2) * Math.Cos(Angle.GetRad(angleAdd))), 2, (float)siz);
                                    graphics.DrawImage(textur, drawRec, new RectangleF((float)pont.point, 0, 1, textur.Height), GraphicsUnit.Pixel);
                                }
                            }
                        }
                    }
                }

                foreach (Entity entity in Program.game.GameEntities)
                {
                    Vector2 spritePosWorld = new Vector2(entity.Position.X, entity.Position.Y);
                    spritePosWorld.X += blockSize / 2;
                    spritePosWorld.Y += blockSize / 2;
                    Vector2 spritePosView = new Vector2(spritePosWorld.X - Program.game.PlayerPos.X, spritePosWorld.Y - Program.game.PlayerPos.Y);
                    double cameraPosX = Math.Atan2(spritePosView.Y, spritePosView.X);
                    Angle realAngle = 0;
                    realAngle.SetRad(Math.Atan2(spritePosView.Y, spritePosView.X));
                    realAngle += Program.game.PlayerAngle;
                    realAngle -= 90;
                    double distSprite = spritePosWorld.Dist(Program.game.PlayerPos) - blockSize * Math.Cos(realAngle.GetRad());
                    double a = (double)realAngle;
                    if (a > 320)
                    {
                        a = 360 - a;
                        cameraPosX = resolution.X / 2 + (a) / (FOV / 2) * resolution.X / 2 - blockSize / (distSprite / 100);
                        double siz = blockSize / distSprite;
                        siz *= resolution.Y;
                        for (int x = 0; x < blockSize; x++)
                        {
                            int realX = (int)(cameraPosX + x * siz / blockSize);
                            Rectangle drawRec = new Rectangle(realX, resolution.Y / 2 - siz / 2, siz / (blockSize / 2), siz);
                            if (drawRec.Width < 1) drawRec.Width = 1;
                            if ((realX > 0) && (realX < resolution.X))
                            {
                                if (distSprite < zBuffer[realX])
                                    graphics.DrawImage(Entities.GetTexture(entity.EntityType, 0), drawRec, new Rectangle(x, 0, 2, 64), GraphicsUnit.Pixel);
                            }
                        }
                    }
                    else if (a < 30)
                    {
                        a = 30 - a;
                        cameraPosX = (a) / (FOV / 2) * resolution.X / 2 - blockSize / (distSprite / 100);
                        double siz = blockSize / distSprite;
                        siz *= resolution.Y;
                        for (int x = 0; x < blockSize; x++)
                        {
                            int realX = (int)(cameraPosX + x * siz / blockSize);

                            Rectangle drawRec = new Rectangle(realX, resolution.Y / 2 - siz / 2, siz / (blockSize / 2), siz);
                            if (drawRec.Width < 1) drawRec.Width = 1;
                            if ((realX > 0) && (realX < resolution.X))
                            {
                                if (distSprite < zBuffer[realX])
                                    graphics.DrawImage(Entities.GetTexture(entity.EntityType, 0), drawRec, new Rectangle(x, 0, 2, 64), GraphicsUnit.Pixel);
                            }
                        }
                    }
                }
            }
            return target;
        }

        public static (double point, double distance) GetTexturePointAndDistance(Vector2 position, Vector2 collision, double castAngle, bool vertical)
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

        public static (bool success, Vector2 point, Tile tile, bool vertical, List<(Entity entity, Vector2 point, bool vertical)> entities) CastRay(Angle angle)
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


                if (resultHorizontal.point.Dist(Program.game.PlayerPos) < resultVertical.point.Dist(Program.game.PlayerPos))
                    return (resultHorizontal.success, resultHorizontal.point, resultHorizontal.tile, false, entities);
                else return (resultVertical.success, resultVertical.point, resultVertical.tile, true, entities);
            }

            if (resultVertical.success)
            {
                return (resultVertical.success, resultVertical.point, resultVertical.tile, true, entities);
            }

            return (false, null, null, false, entities);
        }

        public static (bool success, Vector2 point, Tile tile, List<(Entity entity, Vector2 point, bool vertical)> entities) CastRayHorizontal(Angle angle)
        {
            int retries = 0;
            Vector2 rayPoint = new Vector2(Program.game.PlayerPos.X, Program.game.PlayerPos.Y);
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

        public static (bool success, Vector2 point, Tile tile, List<(Entity entity, Vector2 point, bool vertical)> entities) CastRayVertical(Angle angle)
        {
            int retries = 0;
            Vector2 rayPoint = new Vector2(Program.game.PlayerPos.X, Program.game.PlayerPos.Y);
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


        public static Entity DoorIntersects(Vector2 point)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y < 1; y++)
                {
                    Vector2 tPos = new Vector2(point.X - point.X % blockSize + x * blockSize, point.Y - point.Y % blockSize + y * blockSize);
                    if (Program.game.Doors.ContainsKey(tPos))
                    {
                        if (DoorIntersects(Program.game.Doors[tPos], point))
                            return Program.game.Doors[tPos];
                    }
                }
            }

            return null;
        }

        public static bool DoorIntersects(Entity tile, Vector2 point)
        {
            return ((tile.Position.X + (double)tile.data["OffSetX"] <= point.X) && (tile.Position.X + blockSize >= point.X) &&
                (tile.Position.Y + (double)tile.data["OffSetY"] <= point.Y) && (tile.Position.Y + blockSize >= point.Y));
        }


        public static Tile TileIntersects(Vector2 point)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y < 1; y++)
                {
                    Vector2 tPos = new Vector2(point.X - point.X % blockSize + x * blockSize, point.Y - point.Y % blockSize + y * blockSize);
                    if (Program.game.TileMap.ContainsKey(tPos))
                    {
                        if (TileIntersects(Program.game.TileMap[tPos], point))
                            return Program.game.TileMap[tPos];
                    }
                }
            }

            return null;
        }

        public static bool TileIntersects(Tile tile, Vector2 point)
        {
            return ((tile.Position.X <= point.X) && (tile.Position.X + blockSize >= point.X) &&
                (tile.Position.Y <= point.Y) && (tile.Position.Y + blockSize >= point.Y));
        }

        public static Vector2 getMidPointHorizontal(Vector2 current, Angle angle)
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

        public static Vector2 getMidPointVertical(Vector2 current, Angle angle)
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

        public static Vector2 getPointHorizontal(Vector2 current, Angle angle)
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

        public static Vector2 getPointVertical(Vector2 current, Angle angle)
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
    }
}
