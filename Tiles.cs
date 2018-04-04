using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DTest
{
    public struct TileType
    {
        public string Texture;
        public string Texture90;

        public TileType(string texture, string texture90)
        {
            Texture = texture;
            Texture90 = texture90;
        }

    }

    public class Tile
    {
        public string Type;
        public Vector2 Position;

        public Tile(string type, Vector2 position)
        {
            Type = type;
            Position = position;
        }
    }

    public static class Tiles
    {
        private static Dictionary<string, TileType> tiles = new Dictionary<string, TileType>();

        public static void AddTile(string ID, TileType tile)
        {
            if (tiles.ContainsKey(ID)) tiles[ID] = tile;
            else tiles.Add(ID, tile);
        }

        public static TileType GetTile(string ID)
        {
            return tiles[ID];
        }

        public static Bitmap GetTexture(string ID, bool ninety)
        {
            if (ninety) return Textures.GetTexture(tiles[ID].Texture90);
            return Textures.GetTexture(tiles[ID].Texture);
        }
    }
}
