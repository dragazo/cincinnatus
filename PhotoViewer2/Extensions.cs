using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Cincinnatus
{
    public static class Extensions
    {
        /// <summary>
        /// returns a float that is between min and max inclusive
        /// </summary>
        /// <param name="val">value to be clamped</param>
        /// <param name="min">minimum value of result. must be less than or equal to max</param>
        /// <param name="max">maximum value of result. must be greater than or equal to min</param>
        /// <returns></returns>
        public static float Clamp(this float val, float min, float max)
        {
            if (max < min) throw new ArgumentException("float.Clamp(min, max): min must be less than or equal to max");

            return val < min ? min : (val > max ? max : val);
        }

        /// <summary>
        /// Gets a new Bitmap that is a portion of this Bitmap
        /// </summary>
        /// <param name="img">The Bitmap to process</param>
        /// <param name="x">The starting X coordinate of the subimage</param>
        /// <param name="y">THe starting Y coordinate of the subimage</param>
        /// <param name="width">The width of the resulting subimage</param>
        /// <param name="height">The height of the resulting subimage</param>
        public static Bitmap SubImage(this Bitmap img, int x, int y, int width, int height)
        {
            Bitmap slice = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(slice);

            // just to be safe
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            g.DrawImageUnscaled(img, -x, -y);

            g.Dispose();
            return slice;
        }
    }
}
