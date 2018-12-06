using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Cincinnatus
{
    public static class Utility
    {
        /// <summary>
        /// Opens an image file and returns the read image.
        /// Returns null on failure (e.g. file did not exist or wrong format).
        /// </summary>
        /// <param name="path">file path to image</param>
        public static Image TryGetImage(string path)
        {
            try { return Image.FromFile(path); }
            catch (Exception) { return null; }
        }

        /// <summary>
        /// Transforms cammel case names into normal text by adding spaces before capitals
        /// </summary>
        /// <param name="str">string to transform</param>
        public static string TransformCammelCase(string str)
        {
            StringBuilder b = new StringBuilder();

            if (str.Length != 0) b.Append(str[0]);
            for (int i = 1; i < str.Length; i++)
            {
                if (char.IsUpper(str[i])) b.Append(' ');
                b.Append(str[i]);
            }

            return b.ToString();
        }
    }
}
