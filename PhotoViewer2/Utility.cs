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
        /// Opens and returns a file if it exists. returns null if fail
        /// </summary>
        /// <param name="path">file path to image</param>
        public static Image TryGetImage(string path)
        {
            try { return Image.FromFile(path); }
            catch (Exception) { }

            return null;
        }

        /// <summary>
        /// transforms cammel case names into normal text by adding spaces before capitals
        /// </summary>
        /// <param name="str">string to transform</param>
        public static string TransformCammelCase(string str)
        {
            StringBuilder b = new StringBuilder();

            for (int i = 0; i < str.Length; i++)
            {
                if (i != 0 && str[i] >= 'A' && str[i] <= 'Z') b.Append(' ');
                b.Append(str[i]);
            }

            return b.ToString();
        }
    }
}
