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
        public static float Clamp(this float val, float min, float max)
        {
            if (max < min) throw new ArgumentException("float.Clamp(min, max): min must be less than or equal to max");

            return val < min ? min : val > max ? max : val;
        }
    }
}
