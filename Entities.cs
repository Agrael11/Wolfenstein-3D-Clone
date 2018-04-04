using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DTest
{
    public class EntityType
    {
        public enum EntityClass { DOOR, DOOR90, BLOCK, SPRITE }

        public EntityClass Type;
        public bool Transparent;
        public List<string> Texture;

        public EntityType(EntityClass type, bool transparent, List<string> texture)
        {
            Type = type;
            Transparent = transparent;
            Texture = texture;
        }
    }
    public class Entity
    {
        public string EntityType;
        public Vector2 Position;
        public Dictionary<string, object> data = new Dictionary<string, object>();

        public Entity(string type, Vector2 position)
        {
            EntityType = type;
            Position = position;
        }
    }

    public static class Entities
    {
        private static Dictionary<string, EntityType> entities = new Dictionary<string, EntityType>();

        public static void AddEntity(string ID, EntityType type)
        {
            if (entities.ContainsKey(ID))
                entities[ID] = type;
            else entities.Add(ID, type);
        }

        public static EntityType GetEntity(string ID)
        {
            return entities[ID];
        }

        public static Bitmap GetTexture(string ID, int index)
        {
            return Textures.GetTexture(entities[ID].Texture[index]);
        }
    }
}
