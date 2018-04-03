using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DTest
{
    public static class Textures
    {
        private static Dictionary<String, Bitmap> textures = new Dictionary<string, Bitmap>();

		public static void LoadTexture(string ID, string File)
        {
            Bitmap bmp = (Bitmap)Image.FromFile(File);
            if (textures.ContainsKey(ID)) textures[ID] = bmp;
            else textures.Add(ID, bmp);
        }

		public static Bitmap GetTexture(string ID)
        {
            return textures[ID];
        }
    }
}
